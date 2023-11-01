using System.Collections.Generic;
#if UNITY_WEBGL && !UNITY_EDITOR
using UnityEngine.Networking;
#endif
using UnityImageLoader.Cache;

public class TextureProvider
{
	private static TextureProvider _instance;
	private static readonly object _lock = new object();
	public static TextureProvider Instance 
	{
		get 
		{
			lock (_lock)
			{
				if (_instance == null) _instance = new TextureProvider();
			}
			return _instance; 
		} 
	}

	private readonly Dictionary<string, List<Davinci>> reserveInstance = new Dictionary<string, List<Davinci>>();
	public LRUMemoryCache MemoryCache { get; private set; } = new LRUMemoryCache(1024 * 1024 * 64); // 64mb(defalut is 16mb) 

	private TextureProvider()
	{
	}

	public void RequestTexture2D(string url, Davinci davinci)
	{
		if (NonCacheRequest(url, davinci))
		{
			return;
		}

		if (davinci.CacheMode.IsValid(CacheMode.MemoryCache))
		{
			var texture = Hit(url);
			if (texture != null)
			{
				//Debug.Log($"[Davinci] RequestTexture2D url cache texture / {url}");
				if (davinci.CacheMode.IsValid(CacheMode.MemoryAndFileCache))
				{
					FileBoost.Instance.Hit(davinci.FilePath);
				}
				ProcessImageLoader(davinci, texture);
				return;
			}
		}

		Registration(url, davinci, out bool isNew, out int count);

		if (!isNew)
		{
			//Debug.Log($"[Davinci] RequestTexture2D waiting {count} / {url}");
			return;
		}

		if (davinci.CacheMode == CacheMode.MemoryAndFileCache)
		{
			var davinciFile = FileBoost.Instance.Hit(davinci.FilePath);
			if (davinciFile != null)
			{
				ProcessFileTexture(url, davinci);
			}
			else
			{
				ProcessDownloaderFile(url, davinci);
			}
		}
		else
		{
			ProcessDownloaderTexture(url, davinci);
		}
	}

	private void Registration(string url, Davinci davinci, out bool isNew, out int count)
	{
		lock (_lock)
		{
			if (reserveInstance.TryGetValue(url, out var list))
			{
				if (!list.Contains(davinci))
				{
					list.Add(davinci);
				}
				isNew = false;
				count = list.Count;
			}
			else
			{
				reserveInstance[url] = new List<Davinci>() {
					davinci,
				};
				isNew = true;
				count = 1;
			}
			//Debug.Log($"[Davinci] Registration : {providerDic[url].Count}/{url}");
		}
	}

	private bool NonCacheRequest(string url, Davinci davinci)
	{
		if (davinci.CacheMode != CacheMode.NonCache) return false;
		return ProcessDownloaderTexture(url, davinci);
	}

	private bool ProcessDownloaderTexture(string url, Davinci davinci)
	{
		if (davinci is DavinciDelegate davinciDelegate)
		{
			davinciDelegate.DownloaderTexture(url,
				(t) =>
				{
					if (davinci.CacheMode.IsValid(CacheMode.MemoryCache))
					{
						t.Set(MemoryCache);
						if (reserveInstance.TryGetValue(url, out var providers))
						{
							foreach (var provider in providers)
							{
								ProcessImageLoader(provider, t);
							}
						}
						Clear(url);
					}
					else
					{
						ProcessImageLoader(davinciDelegate, t);
					}
				},
				error =>
				{
					if (davinci.CacheMode.IsValid(CacheMode.MemoryCache))
					{
						if (reserveInstance.TryGetValue(url, out var providers))
						{
							foreach (var provider in providers)
							{
								ProcessProgressError(provider, error);
							}
						}
						Clear(url);
					}
					else
					{
						ProcessProgressError(davinciDelegate, error);
					}
				},
				progress =>
				{
					if (davinci.CacheMode.IsValid(CacheMode.MemoryCache))
					{
						if (reserveInstance.TryGetValue(url, out var providers))
						{
							foreach (var provider in providers)
							{
								ProcessProgressChange(provider, progress);
							}
						}
					}
					else
					{
						ProcessProgressChange(davinciDelegate, progress);
					}
				});
		}
		return true;
	}

    private void ProcessFileTexture(string url, Davinci davinci)
	{
		if (davinci is DavinciDelegate davinciDelegate)
		{
			//Debug.Log($"[Davinci] ProcessFileTexture / {url} / {davinci.FilePath}");
			var requestUrl = davinci.ToFileUrl();
            davinciDelegate.DownloaderTexture(requestUrl,
				(t) =>
				{
					if (davinci.CacheMode.IsValid(CacheMode.MemoryCache))
					{
						t.Set(MemoryCache);
					}

					if (reserveInstance.TryGetValue(url, out var providers))
					{
						foreach (var provider in providers)
						{
							ProcessImageLoader(provider, t);
						}
					}
					Clear(url);
				},
				error =>
				{
					if (reserveInstance.TryGetValue(url, out var providers))
					{
						foreach (var provider in providers)
						{
							ProcessProgressError(provider, error);
						}
					}
					Clear(url);
				},
				progress =>
				{
					if (reserveInstance.TryGetValue(url, out var providers))
					{
						foreach (var provider in providers)
						{
							ProcessProgressChange(provider, progress);
						}
					}
				});
		}
	}

	private void ProcessDownloaderFile(string url, Davinci davinci)
	{
		if (davinci is DavinciDelegate davinciDelegate)
		{
            //Debug.Log($"[Davinci] ProcessDownloaderFile / {url} / {davinci.FilePath}");
#if UNITY_WEBGL && !UNITY_EDITOR
			var requestUrl = url.StartsWith("file://") ?
                UnityWebRequest.UnEscapeURL(url.Replace("file://", string.Empty)) : url;
#else
            var requestUrl = url;
#endif
            davinciDelegate.DownloaderFile(requestUrl,
				(t) =>
				{
					if (davinci.CacheMode.IsValid(CacheMode.MemoryCache))
					{
						t.Set(MemoryCache);
					}

					if (reserveInstance.TryGetValue(url, out var providers))
					{
						foreach (var provider in providers)
						{
							ProcessImageLoader(provider, t);
						}
					}
					Clear(url);
				},
				error =>
				{
					if (reserveInstance.TryGetValue(url, out var providers))
					{
						foreach (var provider in providers)
						{
							ProcessProgressError(provider, error);
						}
					}
					Clear(url);
				},
				progress =>
				{
					if (reserveInstance.TryGetValue(url, out var providers))
					{
						foreach (var provider in providers)
						{
							ProcessProgressChange(provider, progress);
						}
					}
				});
		}
	}

	private void ProcessImageLoader(DavinciDelegate davinciDelegate, SharedTexture t)
	{
		davinciDelegate.ImageLoader(t);
	}

	private void ProcessProgressError(DavinciDelegate davinciDelegate, string message)
	{
		davinciDelegate.OnErrorMessage(message);
	}

	private void ProcessProgressChange(DavinciDelegate davinciDelegate, int progress)
	{
		davinciDelegate.OnProgressChange(progress);
	}

	private SharedTexture Hit(string url)
	{
		return MemoryCache.Hit(url);
	}

	public void ClearCache()
	{
		lock (_lock)
		{
			MemoryCache.Clear();
		}
	}

	public void Clear()
	{
		lock (_lock)
		{
			reserveInstance.Clear();
		}
	}

	public void Clear(string url)
	{
		lock (_lock)
		{
			reserveInstance.Remove(url);
		}
	}

	public void Clear(string url, Davinci davinci)
	{
		lock (_lock)
		{
			if (reserveInstance.TryGetValue(url, out var value))
			{
				var findProvider = value.Find((d => davinci == d));
				if (findProvider != null)
				{
					value.Remove(findProvider);
				}

				if (value.Count == 0)
				{
					reserveInstance.Remove(url);
				}
			}

			//Debug.Log($"[Davinci] Clear : {value?.Count ?? 0}/{url}");
		}
	}
}
