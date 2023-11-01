using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityImageLoader.Cache;

public class FileBoost
{
	public static string FolderPath => Path.Combine(Application.persistentDataPath, "davinci");

	private static FileBoost _instance;
	private static readonly object _lock = new object();

	public LRUDiscCache DiscCache { get; private set; } = new LRUDiscCache(1024 * 1024 * 512); // 512mb(defalut is 16mb) 

	public static FileBoost Instance
	{
		get
		{
			lock (_lock)
			{
				if (_instance == null) _instance = new FileBoost();
			}
			return _instance;
		}
	}

	[RuntimeInitializeOnLoadMethod]
	private static void FileOnLoad()
	{
		FileBoost.Instance.OnPrepared();
	}

	private void OnPrepared()
	{
		DiscCache.OnPrepared(FolderPath);
	}

	public DavinciFile Hit(string path)
	{
		return DiscCache.Hit(path);
	}

	public void CheckFolder()
	{
		if (!Directory.Exists(FolderPath))
		{
			Directory.CreateDirectory(FolderPath);
		}
	}

	public void Clear()
	{
		try
		{
			if (Directory.Exists(FolderPath)) Directory.Delete(FolderPath, true);

			if (Davinci.ENABLE_GLOBAL_LOGS)
				Debug.Log("[Davinci] All Davinci cached files has been cleared.");
		}
		catch (Exception ex)
		{
			if (Davinci.ENABLE_GLOBAL_LOGS)
				Debug.LogError($"[Davinci] Error while removing cached file: {ex}");
		}
	}

	public void Clear(string url)
	{
		try
		{
			var path = Path.Combine(FolderPath, CreateMD5(url));
			if (File.Exists(path)) File.Delete(path);

			if (Davinci.ENABLE_GLOBAL_LOGS)
				Debug.Log($"[Davinci] Cached file has been cleared: {url}");
		}
		catch (Exception ex)
		{
			if (Davinci.ENABLE_GLOBAL_LOGS)
				Debug.LogError($"[Davinci] Error while removing cached file: {ex}");
		}
	}

	public static string CreateMD5(string input)
	{
		// Use input string to calculate MD5 hash
		using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
		{
			byte[] inputBytes = Encoding.ASCII.GetBytes(input);
			byte[] hashBytes = md5.ComputeHash(inputBytes);

			// Convert the byte array to hexadecimal string
			var sb = new StringBuilder();
			for (int i = 0; i < hashBytes.Length; i++)
			{
				sb.Append(hashBytes[i].ToString("X2"));
			}
			return sb.ToString();
		}
	}
}
