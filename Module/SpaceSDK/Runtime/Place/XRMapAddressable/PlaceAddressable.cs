using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace MaxstXR.Place
{
    public static class PlaceAddressable
    {
        private static Dictionary<string, AsyncOperationHandle<IList<IResourceLocation>>> loadedAssets = new Dictionary<string, AsyncOperationHandle<IList<IResourceLocation>>>();

        public static void LoadPov(PlaceScriptableObjects spot, GameObject XRStudio)
        {
            XRStudio.transform.DestroyAllChildren();
            if (spot)
            {
                foreach (var t in spot.IteratorPov())
                {
                    var go = GameObject.Instantiate(t, XRStudio.transform);
                    go.name = t.name;
                }
            }
        }

        public static void LoadMap(PlaceScriptableObjects spot, GameObject TrackableRoot)
        {
            TrackableRoot.transform.DestroyAllChildren();
            if (spot)
            {
                foreach (var t in spot.IteratorTrackable())
                {
                    var go = GameObject.Instantiate(t, TrackableRoot.transform);
                    go.name = t.name;
                }
            }
        }

        public static async UniTask<PlaceScriptableObjects> LoadPlaceSOAsync(string assetKey)
        {
            return await LoadAssetFromAddressableAsync<PlaceScriptableObjects>(assetKey);
        }

        public static async UniTask<T> LoadAssetFromAddressableAsync<T>(string assetKey)
        {
            if (loadedAssets != null && loadedAssets.Keys.Count > 0)
            {
                loadedAssets.Clear();
            }

            //var locations = await Addressables.LoadResourceLocationsAsync(assetKey);
            
            var locations = Addressables.LoadResourceLocationsAsync(assetKey);

            await locations.Task;

            //if (locations != null)
            {
                loadedAssets.Add(assetKey, locations);
            }

            foreach (var location in locations.Result)
            {
                if (location.ResourceType == typeof(T))
                {
                    var handler = await Addressables.LoadAssetAsync<T>(location);

                    return handler;
                }
            }
            return default(T);
        }

        public static void UnloadAsset()
        {
            Debug.Log($"UnloadAddressable, Asset : {loadedAssets.Keys.Count}");

            //AssetBundle.UnloadAllAssetBundles(true);
            Resources.UnloadUnusedAssets();

            if (loadedAssets.Keys.Count > 0)
            {
                foreach (var assetKey in loadedAssets.Keys)
                {
                    var loaded = loadedAssets[assetKey];

                    Debug.Log($"====== loaded release : {assetKey}");
                    Addressables.Release(loaded);
                }
            }

            loadedAssets.Clear();
        }
    }
}
