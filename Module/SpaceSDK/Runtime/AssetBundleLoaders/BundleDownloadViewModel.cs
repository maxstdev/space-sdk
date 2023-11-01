using Cysharp.Threading.Tasks;
using Maxst.Passport;
using MaxstUtils;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MaxstXR.Place
{
	public class BundleDownloadViewModel
	{
		public LiveEvent<Place, Spot> NotifyInitialized = new();
		public LiveEvent<Place, Spot> NotifyCatalogUpdated = new();
		public LiveEvent<Place, Spot, long> NotifySizeDownloaded = new();
		public LiveEvent<Place, Spot, bool> NotifyDownloadFinished = new();
		public LiveEvent<Place, Spot, DownloadProgressStatus> NotifyDownloadProgress = new();
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

		public async UniTask InitializedSystemAsync(Place place, Spot spot, string label) {
            this.label = label;

#if FORCE_ADDRESSABLES_CLEAR_CACHE
			await Addressables.ClearDependencyCacheAsync(label, true);
#endif
            var clientToken = TokenRepo.Instance.GetClientToken().access_token;
            ResourceAPIServiceHelper.AddClientToken(clientToken);

            var BearerAccessClientToken = ResourceAPIServiceHelper.BearerAccessClientToken;

            MapSpot mapResource = await GetMapResource(BearerAccessClientToken, place);
            //string remoteCatalogPath = await GetResourcePublicInfo(clientToken, mapResource.resource.GetResourcePath());
            
			string remoteCatalogPath = mapResource.resource.GetResourcePath();
            
            Addressables.WebRequestOverride = (unityWebRequest) => {
                Debug.Log($"ModifyWebRequest Uri {unityWebRequest.uri}");
                unityWebRequest.uri = new Uri(unityWebRequest.uri.ToString());
                unityWebRequest.SetRequestHeader("token", BearerAccessClientToken);
            };

            Debug.Log($"remoteCatalogPath : {remoteCatalogPath}");
            await Addressables.LoadContentCatalogAsync(remoteCatalogPath).Task;

            NotifyInitialized.Post(place, spot);
        }

		private async UniTask<MapSpot> GetMapResource(string bearerToken, Place place) {
            return await ResourceAPIServiceHelper.ReqMapSpots(bearerToken, place.placeId);
        }

		private async UniTask<string> GetResourcePublicInfo(string bearerToken, string resourcePath) {
            return await ResourceAPIServiceHelper.GetResourcePublicInfo(bearerToken, resourcePath);
        }

		public async UniTask UpdateCatalogAsync(Place place, Spot spot)
		{
			var handle = await Addressables.CheckForCatalogUpdates().Task;
			if (handle.Count > 0)
			{
				await Addressables.UpdateCatalogs(handle);
			}

			NotifyCatalogUpdated.Post(place, spot);
		}

		public async UniTask DownloadSizeAsync(Place place, Spot spot)
		{
			AsyncOperationHandle<long> size = Addressables.GetDownloadSizeAsync(label);
			await size.Task;
			totalSize = size.Result;
			NotifySizeDownloaded.Post(place, spot, totalSize);
		}

		public async UniTask StartDownloadAsync(Place place, Spot spot)
		{
			DownloadHandler = Addressables.DownloadDependenciesAsync(label);
			await DownloadHandler.Task;
			NotifyDownloadFinished.Post(place, spot, DownloadHandler.Status == AsyncOperationStatus.Succeeded);
		}

		public void UpdateDownloadStatus(Place place, Spot spot)
		{
			if (DownloadHandler.IsValid()
				&& false == DownloadHandler.IsDone
				&& DownloadHandler.Status != AsyncOperationStatus.Failed)
			{
				var status = DownloadHandler.GetDownloadStatus();
				long curSize = status.DownloadedBytes;
				long remainedSize = totalSize - curSize;

				NotifyDownloadProgress.Post(place, spot,
					new DownloadProgressStatus(status.DownloadedBytes, totalSize, remainedSize, status.Percent));
			}
		}
	}
}