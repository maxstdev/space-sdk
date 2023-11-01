using Cysharp.Threading.Tasks;
using Maxst.Passport;
using Maxst.Token;
using UnityEngine;

namespace MaxstXR.Place
{
    public class SpaceSDKSample : InjectorBehaviour
    {
        [DI(DIScope.component, DIComponent.place)] private SceneViewModel SceneViewModel { get; }
        [DI(DIScope.component, DIComponent.place)] private BundleDownloadViewModel BundleDownloadViewModel { get; }

        [SerializeField] private BundleDownloadController downloadController;
        [SerializeField] private PlaceScriptableObjects placeScriptableObjects;

        private void Start()
        {
            if (SceneViewModel.CurrentPlace is null)
            {
                TokenRepo.Instance.passportConfig = SpaceSDKSampleAuthConfig.Instance;
                LoadAfterSelectPlace();
            }
        }

        private void OnEnable()
        {
            BundleDownloadViewModel.NotifyInitialized.AddObserver(this, OnNotifyInitialized);
            BundleDownloadViewModel.NotifyCatalogUpdated.AddObserver(this, OnNotifyCatalogUpdated);
            BundleDownloadViewModel.NotifyDownloadProgress.AddObserver(this, OnNotifyDownloadProgress);
            BundleDownloadViewModel.NotifySizeDownloaded.AddObserver(this, OnNotifySizeDownloaded);
            BundleDownloadViewModel.NotifyDownloadFinished.AddObserver(this, OnNotifyDownloadFinished);
        }

        private void OnDisable()
        {
            BundleDownloadViewModel.NotifyInitialized.RemoveAllObserver(this);
            BundleDownloadViewModel.NotifyCatalogUpdated.RemoveAllObserver(this);
            BundleDownloadViewModel.NotifyDownloadProgress.RemoveAllObserver(this);
            BundleDownloadViewModel.NotifySizeDownloaded.RemoveAllObserver(this);
            BundleDownloadViewModel.NotifyDownloadFinished.RemoveAllObserver(this);
        }


        private async void LoadAfterSelectPlace()
        {
            var dsv = DynamicSceneView.Instance(gameObject);
            var place = await SceneViewModel.GetPlaceAsync(dsv, null, false);
            Debug.Log($"LoadAfterSelectPlace place : {place.placeUniqueName}");
            var spot = await SceneViewModel.GetSpotAsync(dsv, place, null, true);
            Debug.Log($"LoadAfterSelectPlace spot : {spot.SpotName}");
            downloadController.StartFetchProcessAsync(place, spot);
        }

        private async UniTask ProcessNextSceneAsync(Place place, Spot spot)
        {
            placeScriptableObjects = await PlaceAddressable.LoadPlaceSOAsync(place.PlaceUniqueName);
            SceneViewModel.PlaceScriptableObjects = placeScriptableObjects;
            Debug.Log($"ProcessNextSceneAsync complete : {place.PlaceUniqueName}");
            await FindObjectOfType<DynamicSceneManager>().LoadPlaceSO(placeScriptableObjects, spot);
        }

        private void OnNotifyDownloadFinished(Place place, Spot spot, bool success)
        {
            downloadController.GoNextStatus();
            ProcessNextSceneAsync(place, spot).Forget();
        }

        private void OnNotifySizeDownloaded(Place place, Spot spot, long size)
        {
            Debug.Log("OnNotifySizeDownloaded called");
            if (size > 0)
            {
                downloadController.GoNextStatus();
                return;
            }

            ProcessNextSceneAsync(place, spot).Forget();
        }

        private void OnNotifyDownloadProgress(Place place, Spot spot, DownloadProgressStatus status)
        {
            var cur = FileController.GetSizeFormatString(status.downloadedBytes, FileController.SizeUnits.MB);
            var total = FileController.GetSizeFormatString(status.totalBytes, FileController.SizeUnits.MB);
        }

        private void OnNotifyCatalogUpdated(Place place, Spot spot)
        {
            downloadController.GoNextStatus();
        }

        private void OnNotifyInitialized(Place place, Spot spot)
        {
            downloadController.GoNextStatus();
        }
    }
}
