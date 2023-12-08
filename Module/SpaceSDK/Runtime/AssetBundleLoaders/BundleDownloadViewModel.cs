using Cysharp.Threading.Tasks;
using Maxst.Passport;
using MaxstUtils;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MaxstXR.Place
{
	public class BundleDownloadViewModel
	{
		public LiveEvent<string> NotifyInitialized = new();
		public LiveEvent<string> NotifyCatalogUpdated = new();
		public LiveEvent<string, long> NotifySizeDownloaded = new();
		public LiveEvent<string, bool> NotifyDownloadFinished = new();
		public LiveEvent<string, DownloadProgressStatus> NotifyDownloadProgress = new();
		private string label;
		private AsyncOperationHandle DownloadHandler;
		private long totalSize;
		private ResourceAPIServiceHelper ResourceAPIServiceHelper;

        protected BundleDownloadViewModel()
		{
		
		}

        public void Awake(GameObject parent)
        {
            ResourceAPIServiceHelper = ResourceAPIServiceHelper.Build(parent);
        }

		public async UniTask InitializedSystemAsync(string space, string label, string key = null) {
            this.label = label;

			if (DownloadHandler.IsValid())
				Addressables.Release(DownloadHandler);

#if FORCE_ADDRESSABLES_CLEAR_CACHE
			await Addressables.ClearDependencyCacheAsync(label, true);
#endif
            var clientToken = TokenRepo.Instance.GetClientToken().access_token;
            ResourceAPIServiceHelper.AddClientToken(clientToken);

            var BearerAccessClientToken = ResourceAPIServiceHelper.BearerAccessClientToken;

			MapSpot mapResource = null;
			if (VersionController.Instance.CurrentMode == VersionController.Mode.Modern)
			{
                mapResource = await GetMapResource(BearerAccessClientToken, key);
            }
			else
			{
				long.TryParse(key, out var lk);
                mapResource = await GetMapResource(BearerAccessClientToken, lk);
            }

            //string remoteCatalogPath = await GetResourcePublicInfo(clientToken, mapResource.resource.GetResourcePath());

            string remoteCatalogPath = mapResource.resource.GetResourcePath();
            
            Addressables.WebRequestOverride = (unityWebRequest) => {
                Debug.Log($"ModifyWebRequest Uri {unityWebRequest.uri}");
                unityWebRequest.uri = new Uri(unityWebRequest.uri.ToString());
                unityWebRequest.SetRequestHeader("token", BearerAccessClientToken);
            };

            Debug.Log($"remoteCatalogPath : {remoteCatalogPath}");
            await Addressables.LoadContentCatalogAsync(remoteCatalogPath).Task;

            NotifyInitialized.Post(space);
        }

		private async UniTask<MapSpot> GetMapResource(string bearerToken, string key) {
            return await ResourceAPIServiceHelper.ReqMapSpots(bearerToken, key);
        }

        private async UniTask<MapSpot> GetMapResource(string bearerToken, long id)
        {
            return await ResourceAPIServiceHelper.ReqMapSpots(bearerToken, id);
        }

        //private async UniTask<string> GetResourcePublicInfo(string bearerToken, string resourcePath) {
        //    return await ResourceAPIServiceHelper.GetResourcePublicInfo(bearerToken, resourcePath);
        //}

		public async UniTask UpdateCatalogAsync(string space)
		{
			var handle = await Addressables.CheckForCatalogUpdates().Task;
			if (handle.Count > 0)
			{
				await Addressables.UpdateCatalogs(handle);
			}

			NotifyCatalogUpdated.Post(space);
		}

		public async UniTask DownloadSizeAsync(string space)
		{
			AsyncOperationHandle<long> size = Addressables.GetDownloadSizeAsync(label);
			await size.Task;
			totalSize = size.Result;
			NotifySizeDownloaded.Post(space, totalSize);
		}

		public async UniTask StartDownloadAsync(string space)
		{
			DownloadHandler = Addressables.DownloadDependenciesAsync(label);
			await DownloadHandler.Task;
			NotifyDownloadFinished.Post(space, DownloadHandler.Status == AsyncOperationStatus.Succeeded);
		}

		public void UpdateDownloadStatus(string space)
		{
			if (DownloadHandler.IsValid()
				&& false == DownloadHandler.IsDone
				&& DownloadHandler.Status != AsyncOperationStatus.Failed)
			{
				var status = DownloadHandler.GetDownloadStatus();
				long curSize = status.DownloadedBytes;
				long remainedSize = totalSize - curSize;

				NotifyDownloadProgress.Post(space, new DownloadProgressStatus(status.DownloadedBytes, totalSize, remainedSize, status.Percent));
			}
		}
	}
}