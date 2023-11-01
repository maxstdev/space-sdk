using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "AddressableLoader", menuName = "ScriptableObjects/AddressableLoaderSO", order = 1)]
public class AddressableLoaderSO : ScriptableObject
{
    [SerializeField] private LoaderProperty property;
    [SerializeField] private bool clearCache = false;
    [SerializeField] private bool enableLocalTest = false;

    private const string STANDALONE_WINDOWS64 = "StandaloneWindows64/";
    private const string STANDALONE_OSX = "StandaloneOSX/";
    private const string WEBGL = "WebGL/";

    private string Platform = STANDALONE_WINDOWS64;
    
    private readonly List<string> allAddressablePath = new();

    public LoaderProperty Property => property;

    public void OnEnable()
    {
#if UNITY_EDITOR_WIN
        Platform = STANDALONE_WINDOWS64;
#elif UNITY_EDITOR_OSX
        Platform = STANDALONE_OSX;
#elif UNITY_WEBGL
        Platform = WEBGL;
#endif
        Debug.Log($"Application.platform : {Application.platform}, set Platform : {Platform}");

#if UNITY_EDITOR
        Addressables.WebRequestOverride = ModifyWebRequest;
#endif
        AsyncInit();
    }

    public void OnDisable()
    {
        Debug.Log("OnDisable Clear the cache");
        if (clearCache) _ = Clear();
    }

    private async void AsyncInit()
    {
        Debug.Log("AddressableLoader Init!");
        string remoteCatalogPath = property.AddressableRemote + Platform + property.Catalog;
        var content = await Addressables.LoadContentCatalogAsync(remoteCatalogPath).Task;
        Debug.Log($"remote catalog path : {remoteCatalogPath}");
        //content.Keys.ToList().ForEach(key =>
        //{
        //    Debug.Log(key);
        //});
        //Debug.LogWarning(String.Join(" ", content.Keys));
        Debug.Log("AyncInit ClearThe Cache");
        if (clearCache) await Clear();
    }

    private void ModifyWebRequest(UnityWebRequest unityWebRequest)
    {
        if (enableLocalTest)
        {
            Debug.Log($"ModifyWebRequest Before {unityWebRequest.uri}");
            unityWebRequest.uri = new Uri(unityWebRequest.uri.ToString().Replace(property.AddressableRemote + Platform, property.AddressableLocal));
            Debug.Log($"ModifyWebRequest After {unityWebRequest.uri}");
        }

        // Add unityWebRequest.SetRequestHeader
    }

    public async Task<T> LoadAddressable<T>(string assetKey, Action<float> progressAssetLoad, Action endAssetLoad) where T : class
    {
        if (assetKey == null || assetKey == "") throw new Exception("GameObject AddressablePath is Empty");

        Debug.Log("AddressableLoader spotAssetKey : " + assetKey);
        try
        {
            var gameObjectLoadedr = CreateLoadAssetAsync<T>(assetKey);
            var bytes = gameObjectLoadedr.GetDownloadStatus().TotalBytes;
            Debug.Log("totalBytes = " + bytes);
            var goSO = await AssetAsync(gameObjectLoadedr, progressAssetLoad);

            endAssetLoad?.Invoke();
            return goSO;
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            return null;
        }
    }

    private AsyncOperationHandle<T> CreateLoadAssetAsync<T>(string assetKey)
    {
        return Addressables.LoadAssetAsync<T>(assetKey);
    }

    private async Task<T> AssetAsync<T>(AsyncOperationHandle<T> loader, Action<float> progressAssetLoad)
    {
        // The first attempt is unconditionally downloaded.
        while (!loader.IsDone)
        {
            if (loader.Status == AsyncOperationStatus.Failed)
            {
                break;
            }
            //Debug.LogWarning(loader.Status);
            //Debug.LogWarning(loader.PercentComplete);
            //Debug.LogWarning(loader.GetDownloadStatus().DownloadedBytes);

            // TODO : Change the number of digits.
            progressAssetLoad?.Invoke(loader.PercentComplete);
            await Task.Yield();
        }

        var rValue = await loader.Task;

        if (loader.OperationException != null)
        {
            throw loader.OperationException;
        }

        return rValue;
    }

    private async Task Clear()
    {
        AssetBundle.UnloadAllAssetBundles(true);
        Resources.UnloadUnusedAssets();

        if (allAddressablePath != null && allAddressablePath.Count > 0)
        {
            allAddressablePath.ForEach(async path =>
            {
                var _ = await Addressables.ClearDependencyCacheAsync(path, true).Task;
            });
        }

        var assetKey = property.PropertiesSOAssetKey;

        if (assetKey != null && assetKey != "")
        {
            var _ = await Addressables.ClearDependencyCacheAsync(assetKey, true).Task;
        }

        //Caching.ClearCache();

        Debug.Log("AddressableLoader Cleared");
    }

    //private async Task<SpotScriptableObjects> SpotLoaded()
    //{
    //    downloadCount += 1;
    //    var loader = Addressables.LoadAssetAsync<SpotScriptableObjects>(stopResourceLoadKey);
    //    var spotScriptableObjects = await loader.Task;
    //    doneCount += 1;
    //    return spotScriptableObjects;
    //}

    //private async Task<GameObject> StopLoadedInstantiate()
    //{
    //    var loader = Addressables.InstantiateAsync(stopResourceLoadKey);
    //    var spotScriptableObjects = await loader.Task;
    //    return spotScriptableObjects;
    //}

    //Addressables.LoadAssetAsync<SpotScriptableObjects>().Completed += OnSpotLoaded;
    //Addressables.LoadAssetAsync<SpotScriptableObjects>("Assets/Maps/SpotResources/SpotResource_Maxst.asset").Completed += OnSpotLoaded;
    //Addressables.DownloadDependenciesAsync

    //resourceLocations = await Addressables.LoadResourceLocationsAsync(labelName.labelString).Task;
    //var dependenciesHandler = Addressables.DownloadDependenciesAsync(resourceLocations);
    //var totalBytes = dependenciesHandler.GetDownloadStatus().TotalBytes;
}
