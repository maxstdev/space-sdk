using ExifLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityImageLoader.Cache;

public class Davinci : MonoBehaviour, DavinciDelegate
{
	public const bool ENABLE_GLOBAL_LOGS = true;

	public string FilePath => Path.Combine(FileBoost.FolderPath, uniqueHash);
	public CacheMode CacheMode { get; private set; } = CacheMode.MemoryAndFileCache;
	public bool Success { get; private set; } = false;
	private bool IsComponentDestroyed { get; set; } = false;

	private Dictionary<string, string> headers = new Dictionary<string, string>();
	private bool enableLog = false;
	private float fadeTime = 1;


	private enum RendererType
	{
		none,
		uiImage,
		rawImage,
		renderer,
		spriteRenderer,
        sprite,
    }

	private RendererType rendererType = RendererType.none;
	private GameObject targetObj;
	private string url = null;
    private UnityAction<Sprite> targetSpriteAction;

    private Texture2D loadingPlaceholder;
	private Texture2D errorPlaceholder;

	private UnityAction onStartAction;
	private UnityAction onDownloadedAction;
	private UnityAction OnLoadedAction;
	private UnityAction onEndAction;

	private UnityAction<int> onDownloadProgressChange;
	private UnityAction<string> onErrorAction;

	private string uniqueHash;


	private void OnDestroy()
	{
		TextureProvider.Instance.Clear(url, this);
	}

	/// <summary>
	/// Get instance of davinci class
	/// </summary>
	/// 
	public static Davinci get()
	{
		var davinci = DavinciManager.Instance.gameObject.AddComponent<Davinci>();
		return davinci;
	}

	/// <summary>
	/// Set image url for download.
	/// </summary>
	/// <param name="url">Image Url</param>
	/// <returns></returns>
	public Davinci load(string url)
	{
		if (enableLog)
			Debug.Log("[Davinci] Url set : " + url);

		this.url = url;
		return this;
	}

	/// <summary>
	/// Set fading animation time.
	/// </summary>
	/// <param name="fadeTime">Fade animation time. Set 0 for disable fading.</param>
	/// <returns></returns>
	public Davinci setFadeTime(float fadeTime)
	{
		if (enableLog)
			Debug.Log("[Davinci] Fading time set : " + fadeTime);

		this.fadeTime = fadeTime;
		return this;
	}

	/// <summary>
	/// Set target Image component.
	/// </summary>
	/// <param name="image">target Unity UI image component</param>
	/// <returns></returns>
	public Davinci into(Image image)
	{
		if (enableLog)
			Debug.Log("[Davinci] Target as UIImage set : " + image);

		rendererType = RendererType.uiImage;
		this.targetObj = image.gameObject;
		return this;
	}

	/// <summary>
	/// Set target RawImage component.
	/// </summary>
	/// <param name="rawImage">target Unity UI RawImage component</param>
	/// <returns></returns>
	public Davinci into(RawImage rawImage)
	{
		if (enableLog)
			Debug.Log("[Davinci] Target as UIImage set : " + rawImage);

		rendererType = RendererType.rawImage;
		this.targetObj = rawImage.gameObject;
		return this;
	}

	/// <summary>
	/// Set target Renderer component.
	/// </summary>
	/// <param name="renderer">target renderer component</param>
	/// <returns></returns>
	public Davinci into(Renderer renderer)
	{
		if (enableLog)
			Debug.Log("[Davinci] Target as Renderer set : " + renderer);

		rendererType = RendererType.renderer;
		this.targetObj = renderer.gameObject;
		return this;
	}

	public Davinci into(SpriteRenderer renderer)
	{
		if (enableLog)
			Debug.Log("[Davinci] Target as Renderer set : " + renderer);

		rendererType = RendererType.spriteRenderer;
		this.targetObj = renderer.gameObject;
		return this;
	}

    public Davinci into(UnityAction<Sprite> targetSpriteAction, GameObject targetObj)
    {
        if (enableLog)
            Debug.Log("[Davinci] Target as Renderer set : " + targetObj.name);

        rendererType = RendererType.sprite;
        this.targetObj = targetObj;
        this.targetSpriteAction = targetSpriteAction;
        return this;
    }

    #region Actions
    public Davinci withStartAction(UnityAction action)
	{
		this.onStartAction = action;

		if (enableLog)
			Debug.Log("[Davinci] On start action set : " + action);

		return this;
	}

	public Davinci withDownloadedAction(UnityAction action)
	{
		this.onDownloadedAction = action;

		if (enableLog)
			Debug.Log("[Davinci] On downloaded action set : " + action);

		return this;
	}

	public Davinci withDownloadProgressChangedAction(UnityAction<int> action)
	{
		this.onDownloadProgressChange = action;

		if (enableLog)
			Debug.Log("[Davinci] On download progress changed action set : " + action);

		return this;
	}

	public Davinci withLoadedAction(UnityAction action)
	{
		this.OnLoadedAction = action;

		if (enableLog)
			Debug.Log("[Davinci] On loaded action set : " + action);

		return this;
	}

	public Davinci withErrorAction(UnityAction<string> action)
	{
		this.onErrorAction = action;

		if (enableLog)
			Debug.Log("[Davinci] On error action set : " + action);

		return this;
	}

	public Davinci withEndAction(UnityAction action)
	{
		this.onEndAction = action;

		if (enableLog)
			Debug.Log("[Davinci] On end action set : " + action);

		return this;
	}
	#endregion

	/// <summary>
	/// Show or hide logs in console.
	/// </summary>
	/// <param name="enable">'true' for show logs in console.</param>
	/// <returns></returns>
	public Davinci setEnableLog(bool enableLog)
	{
		this.enableLog = enableLog;

		if (enableLog)
			Debug.Log("[Davinci] Logging enabled : " + enableLog);

		return this;
	}

	/// <summary>
	/// Set the sprite of image when davinci is downloading and loading image
	/// </summary>
	/// <param name="loadingPlaceholder">loading texture</param>
	/// <returns></returns>
	public Davinci setLoadingPlaceholder(Texture2D loadingPlaceholder)
	{
		this.loadingPlaceholder = loadingPlaceholder;

		if (enableLog)
			Debug.Log("[Davinci] Loading placeholder has been set.");

		return this;
	}

	/// <summary>
	/// Set image sprite when some error occurred during downloading or loading image
	/// </summary>
	/// <param name="errorPlaceholder">error texture</param>
	/// <returns></returns>
	public Davinci setErrorPlaceholder(Texture2D errorPlaceholder)
	{
		this.errorPlaceholder = errorPlaceholder;

		if (enableLog)
			Debug.Log("[Davinci] Error placeholder has been set.");

		return this;
	}

	/// <summary>
	/// Enable cache
	/// </summary>
	/// <returns></returns>
	public Davinci setCached(bool cached)
	{
		setCached(cached ? CacheMode.MemoryAndFileCache : CacheMode.NonCache);
		return this;
	}

	public Davinci setCached(CacheMode cacheMode)
	{
		CacheMode = cacheMode;

		if (enableLog)
			Debug.Log($"[Davinci] Cache enabled : {CacheMode}");

		return this;
	}

	/// <summary>
	/// Reqeust Headers
	/// </summary>
	/// <returns></returns>
	public Davinci SetHeaders(Dictionary<string, string> headers)
	{
		this.headers = headers;
		return this;
	}

	/// <summary>
	/// Start davinci process.
	/// </summary>
	public void start()
	{
		if (url == null)
		{
			error("Url has not been set. Use 'load' funtion to set image url.");
			return;
		}

		try
		{
			var uri = new Uri(url);
			this.url = uri.AbsoluteUri;
		}
		catch (Exception ex)
		{
			error($"Url is not correct. {ex}");
			return;
		}

		if (rendererType == RendererType.none || targetObj == null)
		{
			error("Target has not been set. Use 'into' function to set target component.");
			return;
		}

		if (enableLog)
			Debug.Log("[Davinci] Start Working.");

		SetLoadingImage();
		onStartAction?.Invoke();
		uniqueHash = FileBoost.CreateMD5(url);

		TextureProvider.Instance.RequestTexture2D(url, this);
	}

    public string ToFileUrl()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
		var localUrl = FilePath;
#else
        var localUrl = "file://" + FilePath;
#endif
        return localUrl;
    }

    IEnumerator DownloadProgress(UnityWebRequestAsyncOperation operation, Action<int> callback)
	{
		int progress = 0;
		while (!operation.isDone)
		{
			progress = (int)(operation.progress * 100);
			callback?.Invoke(progress);
			yield return null;
		}

		if (progress < 100)
		{
			callback?.Invoke(100);
		}
	}

	private void SetHeaders(UnityWebRequest www)
	{
		foreach (var header in headers)
		{
			www.SetRequestHeader(header.Key, header.Value);
		}
	}

	private IEnumerator DownloaderTexture(string url, Action<SharedTexture> complete, Action<string> error, Action<int> progress)
	{
		if (enableLog)
			Debug.Log($"[Davinci] DownloaderTexture started. {url}");

		var www = UnityWebRequestTexture.GetTexture(url);
		SetHeaders(www);
		www.downloadHandler = new DownloadHandlerTexture();
		UnityWebRequestAsyncOperation operation = www.SendWebRequest();
		yield return DownloadProgress(operation, progress);

		if (www.error != null)
		{
			error?.Invoke(www.error);
			yield break;
		}

		var texture = DownloadHandlerTexture.GetContent(www);

		var exif = ExifReader.ReadJpeg(www.downloadHandler.data, url);
        if ((exif.IsValid) && exif.Orientation != 0)
        {
            texture = ExifApplicator.ApplyExifRotation(texture, exif.Orientation);
        }

        www.Dispose();
		www = null;

        onDownloadedAction?.Invoke();
		complete?.Invoke(new SharedTexture(texture, this.url));
	}

	private IEnumerator FileLoadTexture(string filePath, Action<SharedTexture> complete)
	{
		if (enableLog)
			Debug.Log($"[Davinci] FileLoadTexture started. {filePath}");

		var fileData = File.ReadAllBytes(filePath);
        var exif = ExifReader.ReadJpeg(File.ReadAllBytes(filePath), url);

        var texture = new Texture2D(2, 2);
		texture.LoadImage(fileData);

        if ((exif.IsValid) && (exif.Orientation != 0))
        {
            texture = ExifApplicator.ApplyExifRotation(texture, exif.Orientation);
        }

        onDownloadedAction?.Invoke();
		complete?.Invoke(new SharedTexture(texture, this.url));
        yield break;
	}

    private IEnumerator CopyLocalFile(string filePath, Action<SharedTexture> complete, Action<string> error, Action<int> progress)
	{
        if (enableLog)
            Debug.Log($"[Davinci] CopyLocalFile started. {filePath}");

        FileBoost.Instance.CheckFolder();
		var bytes = File.ReadAllBytes(filePath);
        var davinciFile = new DavinciFile(FilePath, bytes.Length);
        davinciFile.Save(bytes);
		progress?.Invoke(100);
        davinciFile.Set(FileBoost.Instance.DiscCache);
		yield return FileLoadTexture(davinciFile.Path, (st) =>
        {
            st.SetReferenceFile(davinciFile);
            complete?.Invoke(st);
        });
    }

    private IEnumerator DownloaderFile(string url, Action<SharedTexture> complete, Action<string> error, Action<int> progress)
    {
        if (enableLog)
            Debug.Log($"[Davinci] DownloaderFile started. {url}");

        var www = new UnityWebRequest(url);
		SetHeaders(www);
		www.downloadHandler = new DownloadHandlerBuffer();
		UnityWebRequestAsyncOperation operation = www.SendWebRequest();
		yield return DownloadProgress(operation, progress);

		if (www.error != null)
		{
			error?.Invoke(www.error);
			yield break;
			
		}

		if (enableLog)
			Debug.Log($"[Davinci] DownloaderFile saved. / {FilePath}");

		FileBoost.Instance.CheckFolder();
		var davinciFile = new DavinciFile(FilePath, www.downloadHandler.data.Length);
		davinciFile.Save(www.downloadHandler.data);
		davinciFile.Set(FileBoost.Instance.DiscCache);

#if UNITY_WEBGL && !UNITY_EDITOR
		
		Texture2D texture = new(2, 2);
		texture.LoadImage(www.downloadHandler.data);

		var exif = ExifReader.ReadJpeg(www.downloadHandler.data, url);
		if (exif.IsValid)
        {
            texture = ExifApplicator.ApplyExifRotation(texture, exif.Orientation);
        }
		
		onDownloadedAction?.Invoke();

		var st = new SharedTexture(texture, this.url);
		st.SetReferenceFile(davinciFile);
		complete?.Invoke(st);
		www.Dispose();
#else
        www.Dispose();
		yield return DownloaderTexture("file://" + FilePath, (st) =>
		{
			st.SetReferenceFile(davinciFile);
			complete?.Invoke(st);
		}, error, null);
#endif

	}

	private void SetLoadingImage()
    {
		if (loadingPlaceholder == null) return;

        switch (rendererType)
        {
            case RendererType.renderer:
                Renderer renderer = targetObj.GetComponent<Renderer>();
                renderer.material.mainTexture = loadingPlaceholder;
                break;
            case RendererType.spriteRenderer:
                SpriteRenderer spriteRenderer = targetObj.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = Sprite.Create(loadingPlaceholder,
                     new Rect(0, 0, loadingPlaceholder.width, loadingPlaceholder.height),
                     new Vector2(0.5f, 0.5f));
                break;
            case RendererType.sprite:
                targetSpriteAction?.Invoke(Sprite.Create(loadingPlaceholder,
                     new Rect(0, 0, loadingPlaceholder.width, loadingPlaceholder.height),
                     new Vector2(0.5f, 0.5f)));
                break;
            case RendererType.uiImage:
                Image image = targetObj.GetComponent<Image>();
                Sprite sprite = Sprite.Create(loadingPlaceholder,
                     new Rect(0, 0, loadingPlaceholder.width, loadingPlaceholder.height),
                     new Vector2(0.5f, 0.5f));
                image.sprite = sprite;
                break;

            case RendererType.rawImage:
                RawImage rawImage = targetObj.GetComponent<RawImage>();
                rawImage.texture = loadingPlaceholder;
                break;
        }

    }

	private IEnumerator ImageLoader(Texture2D texture)
    {
        if (enableLog)
            Debug.Log("[Davinci] Start loading image.");

        if (texture == null)
        {
            error("texture is null");
            yield break;
        }

        if (targetObj == null)
		{
			error("targetObj is null");
			yield break;
		}

		Color color;
		switch (rendererType)
        {
            case RendererType.renderer:
                Renderer renderer = targetObj.GetComponent<Renderer>();
				if (renderer == null || renderer.material == null)
				{
					break;
				}

				renderer.material.mainTexture = texture;
                float maxAlpha;

                if (fadeTime > 0 && renderer.material.HasProperty("_Color"))
                {
                    color = renderer.material.color;
                    maxAlpha = color.a;

                    color.a = 0;

                    renderer.material.color = color;
                    float time = Time.time;
                    while (color.a < maxAlpha)
                    {
                        color.a = Mathf.Lerp(0, maxAlpha, (Time.time - time) / fadeTime);

                        if (renderer != null)
                            renderer.material.color = color;

                        yield return null;
                    }
                }

                break;
            case RendererType.spriteRenderer:
                SpriteRenderer spriteRenderer = targetObj.GetComponent<SpriteRenderer>();
				if (spriteRenderer == null)
				{
					break;
				}

				spriteRenderer.sprite = Sprite.Create(texture,
                        new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                if (fadeTime > 0 && spriteRenderer.material.HasProperty("_Color"))
                {
                    color = spriteRenderer.material.color;
                    maxAlpha = color.a;

                    color.a = 0;

                    spriteRenderer.material.color = color;
                    float time = Time.time;
                    while (color.a < maxAlpha)
                    {
                        color.a = Mathf.Lerp(0, maxAlpha, (Time.time - time) / fadeTime);

                        if (spriteRenderer != null)
                            spriteRenderer.material.color = color;

                        yield return null;
                    }
                }
                break;
            case RendererType.sprite:
                MaskableGraphic maskableGraphic = targetObj.GetComponent<MaskableGraphic>();
                if (targetSpriteAction != null || !maskableGraphic)
                {
                    break;
                }
                targetSpriteAction?.Invoke(Sprite.Create(texture,
                        new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));

                if (fadeTime > 0)
                {
                    color = maskableGraphic.color;
                    maxAlpha = color.a;

                    color.a = 0;

                    maskableGraphic.color = color;
                    float time = Time.time;
                    while (color.a < maxAlpha)
                    {
                        color.a = Mathf.Lerp(0, maxAlpha, (Time.time - time) / fadeTime);
                        if (maskableGraphic != null)
							maskableGraphic.material.color = color;
                        yield return null;
                    }
                }
                break;
            case RendererType.uiImage:
                Image image = targetObj.GetComponent<Image>();
				if (image == null)
				{
					break;
				}

				Sprite sprite = Sprite.Create(texture,
                        new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                var aspectRatioFitter = image.GetComponent<AspectRatioFitter>();
                if (aspectRatioFitter != null)
                {
                    Debug.Log("Text width : " + texture.width + ", height : " + texture.height);
                    var textureRatio = (float)texture.width / texture.height;
                    aspectRatioFitter.aspectRatio = textureRatio;
                }

                image.sprite = sprite;
                color = image.color;
                maxAlpha = color.a;

                if (fadeTime > 0)
                {
                    color.a = 0;
                    image.color = color;

                    float time = Time.time;
                    while (color.a < maxAlpha)
                    {
                        color.a = Mathf.Lerp(0, maxAlpha, (Time.time - time) / fadeTime);

                        if (image != null)
                            image.color = color;
                        yield return null;
                    }
                }
                break;

            case RendererType.rawImage:
                RawImage rawImage = targetObj.GetComponent<RawImage>();
				if (rawImage == null)
				{
					break;
				}

				rawImage.texture = texture;
                color = rawImage.color;
                maxAlpha = color.a;

                if (fadeTime > 0)
                {
                    color.a = 0;
                    rawImage.color = color;

                    float time = Time.time;
                    while (color.a < maxAlpha)
                    {
                        color.a = Mathf.Lerp(0, maxAlpha, (Time.time - time) / fadeTime);

                        if (rawImage != null)
                            rawImage.color = color;
                        yield return null;
                    }
                }
                break;
			default:
				break;
        }

        OnLoadedAction?.Invoke();

        if (enableLog)
            Debug.Log("[Davinci] Image has been loaded.");

        Success = true;
        Finish();
    }

    

    private void error(string message)
    {
        Success = false;

        if (enableLog)
            Debug.LogError("[Davinci] Error : " + message);

        onErrorAction?.Invoke(message);

		if (errorPlaceholder != null)
		{
			MainThreadDispatcher.StartCoroutine(ImageLoader(errorPlaceholder));
		}
		else
		{
			Finish();
		}
    }

    private void Finish()
    {
		if (enableLog)
            Debug.Log("[Davinci] Operation has been finished.");

        onEndAction?.Invoke();
		Invoke(nameof(Destroyer), 0.5f);
    }

	private void Destroyer()
	{
		if (IsComponentDestroyed)
		{
			return;
		}
		Destroy(this);
		IsComponentDestroyed = true;
	}

#region DavinciDelegate
	void DavinciDelegate.DownloaderTexture(string url, Action<SharedTexture> complete, Action<string> error, Action<int> progress)
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		MainThreadDispatcher.StartCoroutine(FileLoadTexture(url, complete));
#else
		MainThreadDispatcher.StartCoroutine(DownloaderTexture(url, complete, error, progress));
#endif
	}

	void DavinciDelegate.DownloaderFile(string url, Action<SharedTexture> complete, Action<string> error, Action<int> progress)
	{
		if (url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("file://"))
		{
            MainThreadDispatcher.StartCoroutine(DownloaderFile(url, complete, error, progress));
        }
		else
		{
            MainThreadDispatcher.StartCoroutine(CopyLocalFile(url, complete, error, progress));
        }
	}

	void DavinciDelegate.ImageLoader(SharedTexture sharedTexture)
	{
		if (targetObj)
		{
			targetObj.TryGetOrAddComponent<SharedTextureBehaviour>().Config(sharedTexture.Retain());
            MainThreadDispatcher.StartCoroutine(ImageLoader(sharedTexture.Texture));
        }
		
	}

	void DavinciDelegate.OnProgressChange(int progress)
	{
		onDownloadProgressChange?.Invoke(progress);
		if (enableLog)
			Debug.Log($"[Davinci] Downloading progress : {progress}%");
	}

	void DavinciDelegate.OnErrorMessage(string message)
	{
		error(message);
	}
#endregion DavinciDelegate

	public static void ClearCache(string url)
    {
		FileBoost.Instance.Clear(url);
		TextureProvider.Instance.Clear(url);
	}

    public static void ClearAllCachedFiles()
    {
		FileBoost.Instance.Clear();
		TextureProvider.Instance.Clear();
	}
}
