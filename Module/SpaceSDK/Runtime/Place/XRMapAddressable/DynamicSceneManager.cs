using Cysharp.Threading.Tasks;
using MaxstXR.Extension;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace MaxstXR.Place
{
    public class DynamicSceneManager : BaseSceneManager
    {
        [SerializeField] MapEventDispatcher mapEventDispatcher;
        private SmoothCameraManager cameraManager;
        private KnnManager knnManager;

        private void Awake()
        {
            Setting();
            Permission();
            Initialize();
        }

        private void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            ApplyOcclusion();
            StartTracker();
            VpsListUpadate += NotifyMinimapBounds;
            if (SceneViewModel.PlaceScriptableObjects)
            {
                LoadSpaceSO(SceneViewModel.PlaceScriptableObjects).Forget();
            }
        }

        private void OnEnable()
        {
            OnSubscribe();
        }

        private void OnDisable()
        {
            PlaceAddressable.UnloadAllAssets();
            OnUnsubscribe();
        }

        private void Update()
        {
            UpdateCurrentMode();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                StopTracker();
            }
            else
            {
                StartTracker();
            }
        }

        private void OnDestroy()
        {
            OnInitRecognitionState();
            mapEventDispatcher.MapUnLoaded();
            DisposeTracker();
        }

        public async UniTask LoadSpaceSO(PlaceScriptableObjects placeSO)
        {
            if (placeSO.ProcessMapping())
            {
                await ProcessLoadPov(placeSO);
                await LoadSpace(placeSO);
                mapEventDispatcher.MapLoaded();
            }
            else
            {
                mapEventDispatcher.MapUnLoaded();
            }
        }

        //public async UniTask LoadPlaceSO(PlaceScriptableObjects placeSO, Place place)
        //{
        //    if (placeSO.ProcessMapping())
        //    {
        //        await ProcessLoadPov(placeSO, place);
        //        await LoadPlace(placeSO, place);
        //        mapEventDispatcher.MapLoaded();
        //    }
        //    else
        //    {
        //        mapEventDispatcher.MapUnLoaded();
        //    }
        //}

        public GameObject FindMinimapObject()
        {
            if (TrackableRoot != null && TrackableRoot.transform.GetChild(0) != null)
            {
                Transform tf = TrackableRoot.transform.GetChild(0);

                for (int i = 0; i < tf.childCount; i++)
                {
                    if (tf.GetChild(i).name.ToLower() == "sphere")          // Minimap SpriteRenderer
                    {
                        //SpriteRenderer sr = tf.GetChild(i).GetComponent<SpriteRenderer>();
                        //if (sr != null && sr.sprite != null)
                        //{
                        //    return true;
                        //}
                        return tf.GetChild(i).gameObject;
                    }
                }
            }
            return null;
        }

        private void LoadKnn()
        {
            cameraManager = GameObject.FindObjectOfType<SmoothCameraManager>();
            knnManager = GameObject.FindObjectOfType<KnnManager>();
            cameraManager.Reload();
        }

        private void OnLoadedTrackable()
        {
            ConfigVPSTrackables();
            ApplyOcclusion();
        }

        private async UniTask ProcessLoadPov(PlaceScriptableObjects placeSO)
        {
            PlaceAddressable.LoadPov(placeSO, XRStudio);
            await OnPovLoadComplete(placeSO);
        }

        private async UniTask ProcessLoadMap(PlaceScriptableObjects placeSO)
        {
            PlaceAddressable.LoadMap(placeSO, TrackableRoot);
            await OnMapLoadComplete(placeSO);
        }

        private async UniTask LoadSpace(PlaceScriptableObjects placeSO)
        {
            await ProcessLoadMap(placeSO);
        }

        private async UniTask OnPovLoadComplete(PlaceScriptableObjects placeSO)
        {
            await UniTask.WaitForEndOfFrame(this);

            if (XRStudioController.Instance.ARMode)
            {
                placeSO.HidePov(XRStudio);
            }
            else
            {
                placeSO.ShowSelectedPovObjs(XRStudio, ibrCullBackMaterial, ibrCullFrontMaterial, SceneViewModel.CurrentMapKey(), (sc) =>
                {
                    XrSettings.SpotController.Value = sc;
                });
            }
        }

        private async UniTask OnMapLoadComplete(PlaceScriptableObjects placeSO)
        {
            await UniTask.WaitForEndOfFrame(this);

#if false
            MinimapViewModel.SpaceMapLoadComplete.Post(SceneViewModel.CurrentSpace);
#endif
            SceneViewModel.PlaceLoadComplete.Post();

            OnLoadedTrackable();

            if (VersionController.Instance.CurrentMode == VersionController.Mode.Modern)
            {
                placeSO.ShowSelectedMapObjs(XRStudio, TrackableRoot);
            }
            else
            {
                placeSO.ShowSelectedMapObjs(XRStudio, TrackableRoot, SceneViewModel.CurrentMapKey());
            }

            var pc = XRStudio.GetComponentInChildren<PovController>(true);
            await StartCoroutine(ApplyPose(new KnnStartPose
            {
                Position = pc.transform.position,
                Rotation = Quaternion.identity,
            }));
        }

        private void NotifyMinimapBounds()
        {
            var trackableBounds = new Dictionary<string, Bounds>();

            foreach (var trackable in vPSTrackablesList)
            {
                if (string.IsNullOrEmpty(trackable.spaceId)) continue;

                trackableBounds.Add(trackable.spaceId, GetMaxBounds(trackable.gameObject));
                //Bounds bounds = GetMaxBounds(trackable.gameObject);
                //Debug.Log($"NotifyMinimapBound : {bounds}/{bounds.size}/{bounds.extents.magnitude}");
                //yield return null;
            }

            SceneViewModel.TrackableBounds.Post(trackableBounds);
        }

        private Bounds GetMaxBounds(GameObject g)
        {
            var b = new Bounds(g.transform.position, Vector3.zero);
            foreach (var r in g.GetComponentsInChildren<MeshRenderer>())
            {
                if (r.gameObject.name != MAP_PLANE_OBJ_NAME)
                {
                    b.Encapsulate(r.bounds);
                }
            }
            return b;
        }

        private IEnumerator ApplyPose(KnnStartPose knnStartPose)
        {
            yield return new WaitForEndOfFrame();
            LoadKnn();
            MoveNearestKnn(out Vector3 _, knnStartPose);
        }

        private void MoveNearestKnn(out Vector3 destPos, KnnStartPose knnStartPose)
        {
            var pov = knnManager.FindNearest(knnStartPose.Position);
            Transform nearstKNN = pov.transform;
            knnStartPose.Position = nearstKNN.position;
            cameraManager.WarpToPose(knnStartPose.Position, knnStartPose.Rotation);
            destPos = nearstKNN.position;
        }
    }
}