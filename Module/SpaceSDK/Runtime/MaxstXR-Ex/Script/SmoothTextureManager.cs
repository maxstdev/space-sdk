//#define ENABLE_FILE_CACHE
using Maxst.Passport;
using MaxstXR.Place;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace MaxstXR.Extension
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class SmoothTextureManager : InjectorBehaviour
    {
        private const long ValidSeconds = 60 * 30;
        private const long ESTIMATED_EXPIRATION_TIME = 30;

        [DI(DIScope.component, DIComponent.place)] protected SceneViewModel SceneViewModel { get; }

        [SerializeField] static public string TexturesDirectory;

        [SerializeField] private string TextureExtension = ".ktx2";

        [SerializeField] private string TextureJPGExtension = ".jpg";

        [SerializeField] private string TextureKTXExtension = ".ktx2";

        private string resSpace = null;
        private string resSpot = null;
        [SerializeField] private string imageUrl = null;
        [SerializeField] private long policyExpireTime = 0;

        private Dictionary<string, SmoothSharedTexture> Textures { get; } = new Dictionary<string, SmoothSharedTexture>();

        private readonly object lockObj = new object();

        private void Start()
        {
            UnityThread.initUnityThread();
            GetComponent<Collider>().isTrigger = true; // it must be a trigger
            GetComponent<Rigidbody>().isKinematic = true; // it is not physical

#if DETAIL_DEBUG
            StartCoroutine(DebugTextures());
#endif
        }

        private void OnDestroy()
        {
            try
            {
                foreach (var entry in Textures)
                {
                    entry.Value.Release(true);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
            Textures.Clear();
            TexturesDirectory = null;
        }

        public IEnumerator DebugTextures()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                Debug.Log($"DebugTextures sharedTexture count : {Textures.Count}");
                if (Textures.Count > 9)
                {
                    foreach (var t in Textures.Values)
                    {
                        Debug.Log($"DebugTextures sharedTexture : {t.Texture2d != null}/{t.RefCount}/{t.TexturePath}");
                    }
                }
            }
        }

        public async Task LoadTextureBounds(PovKeyFrame keyFrame, List<int> bounds,
            CancellationTokenSource cancellation, UnityAction<SmoothSharedTexture> injectAction,
            ITransitionDelegate transitionDelegate = null)
        {
            var baseTexturePath = "image8k_split" + "/" + keyFrame.NextPov.Name;

            await CheckTexturesDirectory();
            await RefreshTokenIfNotExists();

            foreach (var bound in bounds)
            {
                var texturePath = baseTexturePath + "_" + bound;
                if (!GenSmoothSharedTexture(texturePath, bound, keyFrame.NextPov, injectAction, out var sharedTexture))
                {
                    continue;
                }

                try
                {
                    var texture = await LoadTextureByImage(texturePath, cancellation, keyFrame, transitionDelegate);
                    lock (lockObj)
                    {
                        //Debug.Log("LoadTexture sharedTexture texture inject : " + texturePath);
                        sharedTexture.Inject(texture);
                    }
                }
                catch (Exception)
                {
                    //Debug.LogWarning(e);
                    if (sharedTexture.Texture2d != null)
                    {
                        sharedTexture.Release();
                    }
                    else
                    {
                        AllowToBeUnloaded(texturePath);
                    }
                    break;
                }
            }
        }


        public async Task<SmoothSharedTexture> LoadTexture(PovKeyFrame keyFrame,
            CancellationTokenSource cancellation, UnityAction<SmoothSharedTexture> injectAction,
            ITransitionDelegate transitionDelegate = null)
        {
            var texturePath = "image2k" + "/" + keyFrame.NextPov.Name;

            await CheckTexturesDirectory();
            await RefreshTokenIfNotExists();

            if (!GenSmoothSharedTexture(texturePath, null, keyFrame.NextPov, injectAction, out var sharedTexture))
            {
                return sharedTexture;
            }

            try
            {
                var texture = await LoadTextureByImage(texturePath, cancellation, keyFrame, transitionDelegate);
                lock (lockObj)
                {
                    //Debug.Log("LoadTexture sharedTexture texture inject : " + texturePath);
                    sharedTexture.Inject(texture);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                keyFrame.KeyFrameStatus = TransitionStatus.DownloadError;
                transitionDelegate?.DownloadException(keyFrame, e);
            }
            return sharedTexture;
        }

        private bool GenSmoothSharedTexture(string texturePath, int? index, PovController povController,
            UnityAction<SmoothSharedTexture> injectAction, out SmoothSharedTexture sharedTexture)
        {
            lock (lockObj)
            {
                if (Textures.TryGetValue(texturePath, out sharedTexture))
                {
                    if (sharedTexture.Texture2d != null)
                    {
                        //Debug.Log("LoadTexture sharedTexture.Texture2d already exsit : " + texturePath);
                        injectAction?.Invoke(sharedTexture);
                    }
                    else
                    {
                        //Debug.Log("LoadTexture sharedTexture.Texture2d not set ,.. and AddListener : " + texturePath);
                        sharedTexture.OnInject.AddListener(injectAction);
                    }
                    return false;
                }

                sharedTexture = new SmoothSharedTexture(texturePath, index, povController, this);
                sharedTexture.OnInject.AddListener(injectAction);
                Textures.Add(texturePath, sharedTexture);
                return true;
            }
        }

        private async Task CheckTexturesDirectory()
        {
            if (TexturesDirectory == null)
            {
                var pathSrc = new TaskCompletionSource<bool>();
                //StartCoroutine(XRAPI.Instance.VRImagePath((path) =>
                //{
                //    TexturesDirectory = path;
                //    pathSrc.SetResult(true);
                //}));

                await XRServiceManager.Instance(gameObject).VRImagePath((path) =>
                {
                    TexturesDirectory = path;
                    pathSrc.SetResult(true);
                });

                await pathSrc.Task;
            }
        }

        private async Task RefreshTokenIfNotExists()
        {
            bool complete = false;
            var tcs = new TaskCompletionSource<bool>();
            //StartCoroutine(XRAPI.Instance.RefreshToken(() =>
            //{
            //    complete = true;
            //    tcs.SetResult(complete);
            //}));

            await XRServiceManager.Instance(gameObject).RefreshToken(() =>
            {
                complete = true;
                tcs.SetResult(complete);
            });

            await tcs.Task;
        }

        private async Task<Texture2D> LoadTextureByImage(string texturePath,
            CancellationTokenSource tokenSource, PovKeyFrame keyFrame, ITransitionDelegate transitionDelegate)
        {
            string ImagePath = await GetImageUrl(SceneViewModel.CurrentTextureKey());
            string combine_texturePath = ImagePath.Replace("*", texturePath + TextureJPGExtension);
            //if (updateProgress) { NumStartedLoading += 1u; }

            //var testPath = XRServiceManager.Instance.TestVRImagePath() + texturePath + TextureJPGExtension;
            //await DefaultLoadTexture(testPath, tokenSource, downloadDelegate);
            return await RequestGetTexture(combine_texturePath, tokenSource, keyFrame, transitionDelegate); ;
        }

        private async Task<Texture2D> RequestGetTexture(string texturePath,
            CancellationTokenSource cancellation, PovKeyFrame keyFrame, ITransitionDelegate transitionDelegate)
        {
#if ENABLE_FILE_CACHE
            var texture = await LoadCacheOrUrl(texturePath, true);
#else
            var handlerTexture = new DownloadHandlerTexture();
            var www = UnityWebRequestTexture.GetTexture(texturePath);
            //SetHeaders(www);
            www.downloadHandler = handlerTexture;
            var request = www.SendWebRequest();

            keyFrame.KeyFrameStatus = TransitionStatus.DownloadStarted;
            transitionDelegate?.DownloadStart(keyFrame);
            var progressValue = 0f;
            transitionDelegate?.DownloadProgess(keyFrame, progressValue);
            while (!www.isDone)
            {
                if (cancellation.Token.IsCancellationRequested)
                {
                    //Debug.LogWarning("LoadTextureByImage ThrowIfCancellationRequested : " + texturePath);
                    www.downloadHandler.Dispose();
                    www.Dispose();
                    cancellation.Token.ThrowIfCancellationRequested();
                }
                progressValue = Mathf.Max(progressValue, www.downloadProgress);
                transitionDelegate?.DownloadProgess(keyFrame, progressValue * 100);
                await Task.Yield();
            }

            if (www.error != null)
            {
                Debug.Log("LoadTextureByImage : " + www.error);
                Debug.Log("Error Path : " + texturePath);
                keyFrame.KeyFrameStatus = TransitionStatus.DownloadError;
                transitionDelegate?.DownloadException(keyFrame, www);
                www.Dispose();
                return null;
            }

            transitionDelegate?.DownloadProgess(keyFrame, 100);
            var texture = handlerTexture.texture;
            www.Dispose();
#endif
            //if (updateProgress) { NumFinishedLoading += 1u; }
            keyFrame.KeyFrameStatus = TransitionStatus.DownloadEnded;
            transitionDelegate?.DownloadComplete(keyFrame);
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }

        private bool IsPolicyExpired()
        {
            return MeasureRemainTimeSeconds() < ESTIMATED_EXPIRATION_TIME;
        }

        private long MeasureRemainTimeSeconds()
        {
            return policyExpireTime - CurrentTimeSeconds();
        }

        private long CurrentTimeSeconds()
        {
            return (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        private async Task<string> GetImageUrl(string id)
        {
            return VersionController.Instance.CurrentMode == VersionController.Mode.Modern
                ? await GetModernImageUrl(id)
                : await GetLegacyImageUrl(id);
        }

        private async Task<string> GetModernImageUrl(string id)
        {
            var completionSource = new TaskCompletionSource<string>();

            if (string.IsNullOrEmpty(id))
            {
                completionSource.SetException(new Exception("Space is null"));
                return await completionSource.Task;
            }

            if (id == resSpace && !string.IsNullOrEmpty(imageUrl) && !IsPolicyExpired())
            {
                completionSource.SetResult(imageUrl);
                return await completionSource.Task;
            }

            var token = await XRTokenManager.Instance.GetActiveToken(TokenRepo.Instance.passportConfig);
            var ob = SpaceConsole.Instance.GetImageUrl(token, id)
                        .SubscribeOn(Scheduler.MainThreadEndOfFrame)
                        .ObserveOn(Scheduler.MainThread)
                        .Subscribe(data =>
                        {
                            resSpace = id;
                            imageUrl = data.url;
                            policyExpireTime = CurrentTimeSeconds() + ValidSeconds;
                            completionSource.SetResult(data.url);
                        }, error =>
                        {
                            completionSource.SetException(error);
                        });

            return await completionSource.Task;
        }

        private async Task<string> GetLegacyImageUrl(string id)
        {
            var completionSource = new TaskCompletionSource<string>();

            if (string.IsNullOrEmpty(id))
            {
                completionSource.SetException(new Exception("Spot is null"));
                return await completionSource.Task;
            }

            if (id == resSpot && !string.IsNullOrEmpty(imageUrl) && !IsPolicyExpired())
            {
                completionSource.SetResult(imageUrl);
                return await completionSource.Task;
            }

            var token = await XRTokenManager.Instance.GetActiveToken(TokenRepo.Instance.passportConfig);
            var contentType = XRTokenManager.Instance.contentType;
            var ob = Place.XRService.Instance.GetImageUrl(id, token, contentType)
                        .SubscribeOn(Scheduler.MainThreadEndOfFrame)
                        .ObserveOn(Scheduler.MainThread)
                        .Subscribe(data =>
                        {
                            resSpot = id;
                            imageUrl = data;
                            policyExpireTime = CurrentTimeSeconds() + ValidSeconds;
                            completionSource.SetResult(data);
                        }, error =>
                        {
                            completionSource.SetException(error);
                        });

            return await completionSource.Task;
        }

        public void AllowToBeUnloaded(string texturePath)
        {
            if (Textures.Remove(texturePath, out var sharedtexture))
            {
                //sharedtexture
            }
        }

        public void SetHeaders(UnityWebRequest www)
        {
            var dic = new Dictionary<string, string> {
                { "Authorization", XRTokenManager.Instance.authorization }
            };

            foreach (var header in dic)
            {
                www.SetRequestHeader(header.Key, header.Value);
            }
        }

#if ENABLE_FILE_CACHE
	private string GetCachePath()
	{
#if UNITY_EDITOR
		return Application.temporaryCachePath;
#elif UNITY_ANDROID
        return Path.Combine(Application.temporaryCachePath, "KTX-Cache");
#else
        return Application.temporaryCachePath;
#endif
	}
	private async Task<Texture2D> LoadCacheOrUrl(string url, bool fileSave)
	{
		var cachePath = url.Replace(maxstAR.XRAPI.imageDownloadUrl, GetCachePath());
		var cacheFolder = Path.GetDirectoryName(cachePath);
		var isExists = File.Exists(cachePath);
		var requestUrl = isExists ? "file://" + cachePath : url;
		var www = UnityWebRequestTexture.GetTexture(requestUrl);
		if (!isExists) SetHeaders(www);
		await www.SendWebRequest();

		if (www.error != null)
		{
			Debug.LogError($"LoadCacheOrUrl Error loading {requestUrl} : {www.error}");
			www.Dispose();
			return null;
		}

		var texture = DownloadHandlerTexture.GetContent(www);
		www.Dispose();
		//Debug.Log($"LoadCacheOrUrl {requestUrl} : {texture.width}/{texture.height}/{texture.format}");
		if (fileSave && !isExists) SaveToTexture2D(texture, cacheFolder, cachePath);
		return texture;
	}

	private IEnumerator LoadCacheOrUrl(string url, bool fileSave, Action<Texture2D> complte)
	{
		var cachePath = url.Replace(maxstAR.XRAPI.imageDownloadUrl, GetCachePath());
		var cacheFolder = Path.GetDirectoryName(cachePath);
		var isExists = File.Exists(cachePath);
		var requestUrl = isExists ? "file://" + cachePath : url;
		var www = UnityWebRequestTexture.GetTexture(requestUrl);
		if (!isExists) SetHeaders(www);
		yield return www.SendWebRequest();

		if (www.error != null)
		{
			Debug.LogError($"LoadCacheOrUrl Error loading {requestUrl} : {www.error}");
			www.Dispose();
			yield break;
		}

		var texture = DownloadHandlerTexture.GetContent(www);
		www.Dispose();
		//Debug.Log($"LoadCacheOrUrl {requestUrl} : {texture.width}/{texture.height}/{texture.format}");
		if (fileSave && !isExists) SaveToTexture2D(texture, cacheFolder, cachePath);
		complte(texture);
	}

	private void SaveToTexture2D(Texture2D texture, string cacheFolder, string cachePath)
	{
		if (!Directory.Exists(cacheFolder)) Directory.CreateDirectory(cacheFolder);
		File.WriteAllBytes(cachePath, texture.EncodeToJPG(100));
	}
#endif
    }
}
