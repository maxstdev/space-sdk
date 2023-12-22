using Cysharp.Threading.Tasks;
using Maxst.Passport;
using Maxst.Settings;
using UnityEngine;

namespace MaxstXR.Place
{
    public class SpaceSDKSample : InjectorBehaviour
    {
        [DI(DIScope.component, DIComponent.place)] private SceneViewModel SceneViewModel { get; }
        [DI(DIScope.component, DIComponent.place)] private BundleDownloadViewModel BundleDownloadViewModel { get; }

        [SerializeField] private BundleDownloadController downloadController;
        [SerializeField] private PlaceScriptableObjects placeScriptableObjects;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            VersionController.Instance.SwitchMode(VersionController.Mode.Modern, 
                SpaceStep.PUBLIC,
                EnvType.Prod);
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(SceneViewModel.CurrentMapKey()))
            {
                TokenRepo.Instance.passportConfig =
                    EnvAdmin.Instance.CurrentEnv.Value == EnvType.Alpha ? 
                    SpaceSDKSampleAuthConfigAlpha.Instance : SpaceSDKSampleAuthConfig.Instance;
                //LoadAfterSelectPlace();
                LoadAfterSelectSpace();
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

        private async void LoadAfterSelectSpace()
        {
            var dsv = DynamicSceneView.Instance(gameObject);
            var space = await SceneViewModel.GetSpaceAsync(dsv, null, false);
            if (space == null)
            {
                Debug.Log("space is null");
                return;
            }

            Debug.Log($"LoadAfterSelectPlace place : {space}");
            downloadController.StartFetchProcessAsync(space);
        }

        private async UniTask ProcessNextSceneAsync(string space)
        {
            placeScriptableObjects = await PlaceAddressable.LoadPlaceSOAsync(space);
            SceneViewModel.PlaceScriptableObjects = placeScriptableObjects;
            Debug.Log($"ProcessNextSceneAsync complete : {space}");
            await FindObjectOfType<DynamicSceneManager>().LoadSpaceSO(placeScriptableObjects);
        }

        //private async void LoadAfterSelectPlace()
        //{
        //    var dsv = DynamicSceneView.Instance(gameObject);
        //    var space = await SceneViewModel.GetPlaceAsync(dsv, null, false);
        //    Debug.Log($"LoadAfterSelectPlace place : {space.placeUniqueName}");
        //    var spot = await SceneViewModel.GetSpotAsync(dsv, place, null, true);
        //    Debug.Log($"LoadAfterSelectPlace spot : {spot.SpotName}");
        //    downloadController.StartFetchProcessAsync(place);
        //}

        //private async UniTask ProcessNextSceneAsync(Place place, Spot spot)
        //{
        //    placeScriptableObjects = await PlaceAddressable.LoadPlaceSOAsync(place.PlaceUniqueName);
        //    SceneViewModel.PlaceScriptableObjects = placeScriptableObjects;
        //    Debug.Log($"ProcessNextSceneAsync complete : {place.PlaceUniqueName}");
        //    await FindObjectOfType<DynamicSceneManager>().LoadPlaceSO(placeScriptableObjects, spot);
        //}

        private void OnNotifyDownloadFinished(string space, bool success)
        {
            downloadController.GoNextStatus();
            ProcessNextSceneAsync(space).Forget();
        }

        private void OnNotifySizeDownloaded(string space, long size)
        {
            Debug.Log("OnNotifySizeDownloaded called");
            if (size > 0)
            {
                downloadController.GoNextStatus();
                return;
            }

            ProcessNextSceneAsync(space).Forget();
        }

        private void OnNotifyDownloadProgress(string space, DownloadProgressStatus status)
        {
            var cur = FileController.GetSizeFormatString(status.downloadedBytes, FileController.SizeUnits.MB);
            var total = FileController.GetSizeFormatString(status.totalBytes, FileController.SizeUnits.MB);
        }

        private void OnNotifyCatalogUpdated(string space)
        {
            downloadController.GoNextStatus();
        }

        private void OnNotifyInitialized(string space)
        {
            downloadController.GoNextStatus();
        }
    }
}
