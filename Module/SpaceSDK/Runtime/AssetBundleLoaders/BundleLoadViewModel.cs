using Cysharp.Threading.Tasks;
using MaxstUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MaxstXR.Place
{
    public class BundleLoadViewModel : Injector
    {
        public readonly LiveEvent BundleLoadComplete = new();
        public readonly LiveEvent<DownloadHandler> CustomDownloadComplete = new();
        public readonly Dictionary<string, AssetBundle> loadedAssetBundles = new();

        public Hash128 Hash { get; private set; }

        protected BundleLoadViewModel()
        {
            //Debug.Log("BundleLoadViewModel Construct");
        }

        public IEnumerator GetLatestHash(string manifestUrl, string bundleName)
        {
            using UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(manifestUrl);
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"GetLatestHash {manifestUrl}, error : {uwr.error}");
            }
            else
            {
                var bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                var manifestBundle = bundle.name;
                AssetBundleManifest manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                yield return Hash;
                Hash = manifest.GetAssetBundleHash(bundleName);
                Debug.Log($">>>> hash of AssetBundleManifest is {Hash}");
                bundle.Unload(true);
            }
        }

        public IEnumerator DownloadOrCacheBundles(string manifestUrl, Action onLoadAction)
        {
            yield return LoadBundleProcess(manifestUrl, (bundle) =>
                {
                    Debug.Log($">>> Latest version of bundle {bundle.name} is (already) cached");
                });
            onLoadAction();
        }

        public IEnumerator LoadBundleProcess(string manifestUrl, Action<AssetBundle> onLoadAction)
        {
            yield return GetLatestHash(manifestUrl, AssetMap.GetCommonBundleName(Application.platform));
            var bundleUrl = AssetMap.GetCommonBundleUrl(Application.platform);
            yield return DownloadBundle(bundleUrl, Hash, onLoadAction);
        }

        public async UniTask<T> LoadAssetProcessAsync<T>(string assetName)
        {
            return await LoadAssetFromAddressableAsync<T>(assetName);
        }

        //below is old version

        public IEnumerator DownloadBundle(string bundleName, Hash128 hash, Action<UnityEngine.AssetBundle> onLoadAction)
        {
            using UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(bundleName, hash);

            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                var bundle = DownloadHandlerAssetBundle.GetContent(uwr);

                if (bundle == null)
                {
                    Debug.Log(">>> Failed to load asset bundle- bundle is null");
                    yield break;
                }
                //var names = bundle.GetAllAssetNames();
                //foreach (var name in names)
                //{
                //    Debug.LogFormat(">>>{0}// {1}", bundle.name, name);
                //}
                onLoadAction(bundle);
                bundle.Unload(false);
            }
        }

        public IEnumerator DownLoadBundles(List<string> bundleUrl)
        {
            //isBundleLoadComplete = false;
            foreach (var url in bundleUrl)
            {
                using UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(url);
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(uwr.error);
                    break;
                }
                else
                {
                    var bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    var names = bundle.GetAllAssetNames();
                    foreach (var name in names)
                    {
                        Debug.LogFormat(">>>{0}// {1}", bundle.name, name);
                    }
                }
            }
            BundleLoadComplete.Post();
            //isBundleLoadComplete = true;
            Debug.Log(">>> Bundles load complete! ");
        }

        public async UniTask<T> LoadAssetFromAddressableAsync<T>(string assetKey)
        {
            var handle = Addressables.LoadAssetAsync<T>(assetKey);
            var asset = await handle.Task;
            return handle.Status == AsyncOperationStatus.Succeeded ? asset : default;
        }

        public IEnumerator LoadAsset<T>(string bundleUrl, Hash128 hash, string assetName, Action<T> onLoad) where T : class
        {
            if (loadedAssetBundles.TryGetValue(bundleUrl, out var cachedAssetBundle))
            {
                var assetBundleRequest = cachedAssetBundle.LoadAssetAsync<T>(assetName);
                yield return assetBundleRequest;
                onLoad(assetBundleRequest.asset as T);
                yield break;
            }

            using UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl, hash);
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                var bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                loadedAssetBundles.Add(bundleUrl, bundle);
                if (bundle == null)
                {
                    Debug.Log(">>> Failed to load asset bundle- bundle is null");
                    onLoad(default);
                    yield break;
                }
                var assetBundleRequest = bundle.LoadAssetAsync<T>(assetName);
                yield return assetBundleRequest;
                onLoad(assetBundleRequest.asset as T);
            }
        }

        public void UnloadAllBundles()
        {
            //Asset 관련해서 다시 로드하는 로직 임시 제한

            /*
            IsAssetLoadComplete = false;
            foreach(var entry in loadedAssetBundles)
            {
                entry.Value.Unload(true);
            }
            loadedAssetBundles.Clear();
            AssetBundle.UnloadAllAssetBundles(true);
            */
        }

        public IEnumerator CustomDownloadBundle(string bundleUrl)
        {
            UnityWebRequest request = UnityWebRequest.Get(bundleUrl);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                yield break;
            }

            CustomDownloadComplete.Post(request.downloadHandler);
        }

        public async UniTask<string> GetServerBundleHashOrNull(string remoteBundleManifestUrl, string bundleName)
        {
            using UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(remoteBundleManifestUrl);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"GetLatestHash {remoteBundleManifestUrl}, error : {request.error}");
                return null;
            }

            var bundle = DownloadHandlerAssetBundle.GetContent(request);
            if (bundle is null)
            {
                return null;
            }
            var manifestBundle = bundle.name;
            AssetBundleManifest manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (manifest is null)
            {
                return null;
            }
            string hash = manifest.GetAssetBundleHash(bundleName).ToString();
            bundle.Unload(true);
            return hash;
        }
    }
}
