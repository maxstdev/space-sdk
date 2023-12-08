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
        private static Dictionary<string, AsyncOperationHandle> loadedAssets = new Dictionary<string, AsyncOperationHandle>();

        public static void LoadPov(PlaceScriptableObjects space, GameObject XRStudio)
        {
            XRStudio.transform.DestroyAllChildren();
            if (space)
            {
                foreach (var t in space.IteratorPov())
                {
                    var go = GameObject.Instantiate(t, XRStudio.transform);
                    go.name = t.name;
                }
            }
        }

        public static void LoadMap(PlaceScriptableObjects space, GameObject TrackableRoot)
        {
            TrackableRoot.transform.DestroyAllChildren();
            if (space)
            {
                foreach (var t in space.IteratorTrackable())
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
            if (loadedAssets.ContainsKey(assetKey))
            {
                UnloadAllAssets();
            }

            var handler = Addressables.LoadAssetAsync<T>(assetKey);
            await handler.Task;

            if (handler.Result != null)
            {
                loadedAssets.Add(assetKey, handler);
            }

            return handler.Result;
        }

        public static void UnloadAsset(string assetKey)
        {
            if (loadedAssets.ContainsKey(assetKey))
            {
                Addressables.Release(loadedAssets[assetKey]);
                loadedAssets.Remove(assetKey);
            }
        }

        public static void UnloadAllAssets()
        {
            foreach (var handle in loadedAssets.Values)
            {
                Addressables.Release(handle);
            }
            loadedAssets.Clear();
        }
    }
}
