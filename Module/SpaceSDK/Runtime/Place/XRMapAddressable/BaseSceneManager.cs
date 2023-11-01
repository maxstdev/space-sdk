using Maxst.Passport;
using maxstAR;
using MaxstXR.Extension;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UnityEngine;


namespace MaxstXR.Place
{
    public enum TrackerStatus
    {
        TrackingEnd,
        TrackingStart,
        TrackingStartDelayed,
    }

    public abstract class BaseSceneManager : InjectorBehaviour
    {
        public const string MESH_TAG = "Mesh";
        public const string MAP_PLANE_OBJ_NAME = "Plane";

        [DI(DIScope.component, DIComponent.place)] protected XrSettings XrSettings { get; }
        [DI(DIScope.component, DIComponent.place)] protected CustomerRepo CustomerRepo { get; }
        [DI(DIScope.component, DIComponent.place)] protected SceneViewModel SceneViewModel { get; }
        [DI(DIScope.component, DIComponent.minimap)] protected MinimapPoiEvent MinimapPoiEvent { get; }
        [DI(DIScope.component, DIComponent.minimap)] protected MinimapViewModel MinimapViewModel { get; }

        [SerializeField] private List<GameObject> disableObjects = new List<GameObject>();
        [SerializeField] private List<GameObject> occlusionObjects = new List<GameObject>();

        [SerializeField] protected Material buildingMaterial;
        [SerializeField] protected Material runtimeBuildingMaterial;
        [SerializeField] protected Material ibrCullBackMaterial;
        [SerializeField] protected Material ibrCullFrontMaterial;

        [SerializeField] private bool isOcclusion = true;

        [field: SerializeField] public GameObject TrackableRoot { get; private set; }
        [field: SerializeField] public GameObject WorldContent { get; private set; }
        [field: SerializeField] public GameObject MinimapContent { get; private set; }

        [SerializeField] private int targetFrameRate = 30;

        [SerializeField] private LayerMask NormalLayerMask;
        [SerializeField] private LayerMask RecognizeLayerMask;

        protected CameraBackgroundBehaviour cameraBackgroundBehaviour = null;
        protected GameObject xrCamera = null;
        [SerializeField] protected GameObject XRStudio = null;
        protected List<VPSTrackable> vPSTrackablesList = new List<VPSTrackable>();
        protected System.Action VpsListUpadate { get; set; } = null;

        private string currentLocalizerLocation = "";

        private bool isCameraDeviceStarted = false;
        private TrackerStatus trackerStatus = TrackerStatus.TrackingEnd;
        private System.IDisposable tokenSubscription;

        protected void Setting()
        {
            //TopologyViewModel.Awake(gameObject);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;
        }

        protected void Permission()
        {
#if UNITY_ANDROID
        AndroidRuntimePermissions.Permission[] result =
            AndroidRuntimePermissions.RequestPermissions(
                "android.permission.WRITE_EXTERNAL_STORAGE",
                "android.permission.CAMERA",
                "android.permission.ACCESS_FINE_LOCATION",
                "android.permission.ACCESS_COARSE_LOCATION");
        if (result[0] == AndroidRuntimePermissions.Permission.Granted 
            && result[1] == AndroidRuntimePermissions.Permission.Granted)
        {
            Debug.Log("We have all the permissions!");
        }
        else
        {
            Debug.Log("Some permission(s) are not granted...");
        }
#endif // UNITY_ANDROID
        }

        protected void Initialize()
        {
            ARManager arManagr = FindObjectOfType<ARManager>();
            if (arManagr == null)
            {
                Debug.LogError("Can't find ARManager. You need to add ARManager prefab in scene.");
                return;
            }
            else
            {
                xrCamera = arManagr.gameObject;
                XrSettings.XrCamera = xrCamera.GetComponent<Camera>();
                ApplyLayerMask(XrSettings.XrCamera, NormalLayerMask);
            }

            cameraBackgroundBehaviour = arManagr.GetCameraBackgroundBehaviour();
            if (cameraBackgroundBehaviour == null)
            {
                Debug.LogError("Can't find CameraBackgroundBehaviour.");
            }

            foreach (GameObject eachObject in disableObjects)
            {
                if (eachObject != null)
                {
                    eachObject.SetActive(false);
                }
            }

            XRStudio = FindObjectOfType<XRStudioController>().gameObject;

            SceneViewModel.CurrentRecognitionState.AsObservable().Subscribe(UpdateRecognitionState).AddTo(this);
            //XRViewModel.CanacelAllCommand();
        }

        protected void ConfigVPSTrackables()
        {
            vPSTrackablesList.Clear();

            VPSTrackable[] trackables = TrackableRoot.GetComponentsInChildren<VPSTrackable>(true);
            if (trackables != null)
            {
                vPSTrackablesList.AddRange(trackables);
            }
            else
            {
                Debug.LogError("You need to add VPSTrackables.");
            }

            occlusionObjects.Clear();
            foreach (var t in trackables)
            {
                occlusionObjects.Add(t.gameObject);
            }

            VpsListUpadate?.Invoke();
        }

        protected void ApplyOcclusion()
        {
            if (XRStudio.GetComponent<XRStudioController>().ARMode)
            {
#if UNITY_EDITOR
                if (isOcclusion)
                {
                    ChangeMeshMaterial(runtimeBuildingMaterial);
                }
                else
                {
                    ChangeMeshMaterial(buildingMaterial);
                }
#else
                ChangeMeshMaterial(runtimeBuildingMaterial);
#endif
            }
            else
            {
                ChangeMeshMaterial(ibrCullBackMaterial);
            }
            ConfigMeshTag();
        }

        protected void StartTracker()
        {
            if (Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.WindowsEditor)
            {
                string simulatePath = XRStudio.GetComponent<XRStudioController>().xrSimulatePath;
                Debug.Log(simulatePath);
                if (Directory.Exists(simulatePath))
                {
                    Debug.Log(simulatePath);
                    CameraDevice.GetInstance().Start(simulatePath);
                    MaxstAR.SetScreenOrientation((int)ScreenOrientation.Portrait);
                }
            }
            else
            {
                if (CameraDevice.GetInstance().IsFusionSupported(CameraDevice.FusionType.ARCamera))
                {
                    Debug.Log(">>> BaseSceneManager.StartTracker Not editor, isFusionSupported");

                    if (XRStudio.GetComponent<XRStudioController>().ARMode)
                    {
                        CameraDevice.GetInstance().Start();
                        isCameraDeviceStarted = true;
                    }
                }
                else
                {
                    TrackerManager.GetInstance().RequestARCoreApk();
                }
            }

            /**
             * Token을 얻기 위해서는 StartTracker가 필요함.
             * 향후 Tracker와 인증 관련 내부 로직이 변경되면 굳이 StartTracker 호출 할 필요 없음
             * VR에서의 Tracker 동작은 내부 로직과 관련이 없기 때문에 불필요한 호출 임.
             */
            //if (XRStudio.GetComponent<XRStudioController>().ARMode)
            //{
            //TrackerManager.GetInstance().StartTracker();
            //if (isLgSpot) TrackerManager.GetInstance().AddTrackerData("{\"vps_server\":\"p0000_m0011_lgseocho\"}");
            //}
            StartTrackerWhenPlaceConfigured();
        }

        private void StartTrackerWhenPlaceConfigured()
        {
            if (trackerStatus == TrackerStatus.TrackingStart)
            {
                return;
            }

            if (SceneViewModel.CurrentPlace != null)
		    {
			    trackerStatus = TrackerStatus.TrackingStart;
                if (XRStudio.GetComponent<XRStudioController>().ARMode)
                {
                    TrackerManager.GetInstance().StartTrackerAtPlace((int)SceneViewModel.CurrentPlace.placeId);
                }
			    SubscribeToken();
			    //TrackerManager.GetInstance().SetSecretIdSecretKey(XRAPI.secretId, XRAPI.secretKey);
			    Debug.Log(">>> BaseSceneManager.StartTrackerAtPlace : " + SceneViewModel.CurrentPlace.placeId);
		    }
		    else
		    {
			    trackerStatus = TrackerStatus.TrackingStartDelayed;
			    Debug.Log(">>> BaseSceneManager.StartTrackerAtPlace : " + TrackerStatus.TrackingStartDelayed);
		    }
        }

        protected void StartTrackerWithRefreshSubscribe()
        {
            UnsubscribeToken();
            trackerStatus = TrackerStatus.TrackingStart;
            if (XRStudio.GetComponent<XRStudioController>().ARMode)
            {
                TrackerManager.GetInstance().StartTrackerAtPlace((int)SceneViewModel.CurrentPlace.placeId);
            }
            SubscribeToken();
        }

        protected void StopTracker(bool cameraStop = true)
        {
            trackerStatus = TrackerStatus.TrackingEnd;
            if (isCameraDeviceStarted && cameraStop)
            {
                CameraDevice.GetInstance().Stop();
                Debug.Log(">>> BaseSceneManager.StopTracker- CameraDevice.GetInstance().Stop()");
            }

            if (XRStudio != null)
            {
                if (XRStudio.GetComponent<XRStudioController>().ARMode)
                {
                    TrackerManager.GetInstance().StopTracker();
                }
            }
            

            UnsubscribeToken();
            Debug.Log(">>> BaseSceneManager.StopTracker- TrackerManager.GetInstance().StopTracker()");
        }

        protected void DisposeTracker()
        {
            StopTracker();
            if (XRStudio != null)
            {
                if (XRStudio.GetComponent<XRStudioController>().ARMode)
                {
                    TrackerManager.GetInstance().DestroyTracker();
                }
            }
        }

        protected void UpdateCurrentMode()
        {
            if (XRStudio != null)
            {
                if (XRStudio.GetComponent<XRStudioController>().ARMode)
                {
                    UpdateARMode();
                }
                else
                {
                    UpdateVRMode();
                }
            }
        }

        public bool FindMinimapResource()
        {
            if (TrackableRoot != null && TrackableRoot.transform.GetChild(0) != null)
            {
                Transform tf = TrackableRoot.transform.GetChild(0);

                for (int i = 0; i < tf.childCount; i++)
                {
                    if (tf.GetChild(i).name.ToLower() == "sphere")          // Minimap SpriteRenderer
                    {
                        SpriteRenderer sr = tf.GetChild(i).GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        protected void InvisableMinimap()
        {
            //XRViewModel.CancelCommand(XR.XRViewCommand.ENABLE_MAPBUTTON);
        }

        private void UpdateARMode()
        {
            TrackerManager.GetInstance().UpdateFrame(true);
            ARFrame arFrame = TrackerManager.GetInstance().GetARFrame();
            TrackedImage trackedImage = arFrame.GetTrackedImage();

            if (trackedImage.IsTextureId())
            {
                System.IntPtr[] cameraTextureIds = trackedImage.GetTextureIds();
                cameraBackgroundBehaviour.UpdateCameraBackgroundImage(cameraTextureIds);
            }
            else
            {
                cameraBackgroundBehaviour.UpdateCameraBackgroundImage(trackedImage);
            }

            var recognitionState = arFrame.GetARLocationRecognitionState();
            if (recognitionState == ARLocationRecognitionState.ARLocationRecognitionStateNormal)
            {
                Matrix4x4 targetPose = arFrame.GetTransform();

                var p = MatrixUtils.PositionFromMatrix(targetPose);
                var q = MatrixUtils.QuaternionFromMatrix(targetPose);
                var s = MatrixUtils.ScaleFromMatrix(targetPose);
                xrCamera.transform.position = p;
                xrCamera.transform.rotation = q;
                xrCamera.transform.localScale = s;
                XrSettings.SetCameraPose(ref p, ref q);

                string localizerLocation = arFrame.GetARLocalizerLocation();

                //Debug.Log("UpdateARMode : " + localizerLocation);
                if (currentLocalizerLocation != localizerLocation)
                {
                    foreach (VPSTrackable eachTrackable in vPSTrackablesList)
                    {
                        bool isLocationInclude = false;
                        foreach (string eachLocation in eachTrackable.localizerLocation)
                        {
                            if (localizerLocation == eachLocation)
                            {
                                isLocationInclude = true;
                                currentLocalizerLocation = localizerLocation;
                                XrSettings.LocalizerLocation.Value = localizerLocation;
                                XrSettings.OldNavigationLocation.Value = XrSettings.NavigationLocation.Value;
                                XrSettings.NavigationLocation.Value = eachTrackable.navigationLocation;
                                //Debug.Log("UpdateARMode : " + localizerLocation);
                                break;
                            }
                        }
                        eachTrackable.gameObject.SetActive(isLocationInclude);
                    }
                }
            }
            else
            {
                foreach (VPSTrackable eachTrackable in vPSTrackablesList)
                {
                    eachTrackable.gameObject.SetActive(false);
                }

                currentLocalizerLocation = string.Empty;
                XrSettings.LocalizerLocation.Value = string.Empty;
                XrSettings.OldNavigationLocation.Value = string.Empty;
                XrSettings.NavigationLocation.Value = string.Empty;
            }
            SceneViewModel.CurrentRecognitionState.Value = recognitionState;
        }

        private void UpdateVRMode()
        {
            VPSTrackable activeVPSTrackable = null;
            foreach (VPSTrackable eachTrackable in vPSTrackablesList)
            {
                if (eachTrackable.gameObject.activeSelf)
                {
                    activeVPSTrackable = eachTrackable;
                    break;
                }
            }
            var recognitionState = ARLocationRecognitionState.ARLocationRecognitionStateNormal;
            if (activeVPSTrackable != null)
            {
                string localizerLocation =
                    activeVPSTrackable.localizerLocation != null && activeVPSTrackable.localizerLocation.Count > 0
                    ? activeVPSTrackable.localizerLocation[0] : "";
                currentLocalizerLocation = localizerLocation;
                XrSettings.LocalizerLocation.Value = localizerLocation;
                XrSettings.OldNavigationLocation.Value = XrSettings.NavigationLocation.Value;
                XrSettings.NavigationLocation.Value = activeVPSTrackable.navigationLocation;
                WorldContent.SetActive(true);
                MinimapContent.SetActive(true);
            }
            else
            {
                recognitionState = ARLocationRecognitionState.ARLocationRecognitionStateNotAvailable;
                currentLocalizerLocation = string.Empty;
                XrSettings.LocalizerLocation.Value = string.Empty;
                XrSettings.OldNavigationLocation.Value = string.Empty;
                XrSettings.NavigationLocation.Value = string.Empty;
                WorldContent.SetActive(false);
                MinimapContent.SetActive(false);
            }
            var p = xrCamera.transform.position;
            var q = xrCamera.transform.rotation;
            XrSettings.SetCameraPose(ref p, ref q);
            SceneViewModel.CurrentRecognitionState.Value = recognitionState;
        }

        private void ChangeMeshMaterial(Material material)
        {
            //material.renderQueue = 1900;
            foreach (GameObject eachGameObject in occlusionObjects)
            {
                if (eachGameObject == null)
                {
                    continue;
                }

                var cullingRenderer = eachGameObject.GetComponentsInChildren<MeshRenderer>();
                foreach (var eachRenderer in cullingRenderer)
                {
                    if (eachRenderer.sharedMaterials.Length > 0)
                    {
                        var materials = new Material[eachRenderer.sharedMaterials.Length];
                        for (int i = 0; i < eachRenderer.sharedMaterials.Length; ++i)
                        {
                            materials[i] = material;
                        }
                        eachRenderer.sharedMaterials = materials;
                    }
                }
            }
        }

        private void UpdateRecognitionState(ARLocationRecognitionState recognitionState)
        {
            if (recognitionState == ARLocationRecognitionState.ARLocationRecognitionStateNormal)
            {
                ApplyLayerMask(XrSettings.XrCamera, RecognizeLayerMask);
            }
            else
            {
                ApplyLayerMask(XrSettings.XrCamera, NormalLayerMask);
            }
        }

        private void ConfigMeshTag()
        {
            var groundLayer = PlaceResources.Instance(gameObject).GroundLayer;
            var structureLayer = PlaceResources.Instance(gameObject).StructureLayer;
            var minimapLayer = PlaceResources.Instance(gameObject).MinimapLayer;

            foreach (var eachGameObject in occlusionObjects)
            {
                var parentObject = eachGameObject.transform.parent.gameObject;
                parentObject.layer = 0;

                var meshColliders = eachGameObject.GetComponentsInChildren<MeshCollider>();
                if (meshColliders.IsNotEmpty())
                {
                    foreach (var meshCollider in meshColliders)
                    {
                        if (meshCollider.gameObject.name == MAP_PLANE_OBJ_NAME)
                        {
                            meshCollider.gameObject.layer = groundLayer;
                        }
                        else if (meshCollider.gameObject.name.Contains("plan_base"))
                        {
                            meshCollider.gameObject.layer = groundLayer;

                            MoveGroundObjects(parentObject, meshCollider.gameObject);
                        }
                        else if (meshCollider.gameObject.name.Contains("ground"))
                        {
                            meshCollider.gameObject.layer = groundLayer;
                            MoveGroundObjects(parentObject, meshCollider.gameObject);
                        }
                        else
                        {
                            meshCollider.gameObject.layer = structureLayer;
                            meshCollider.gameObject.GetComponent<MeshRenderer>().enabled = false;
                        }
                    }
                }

                var spriteRenderers = parentObject.GetComponentsInChildren<SpriteRenderer>();
                if (spriteRenderers.IsNotEmpty())
                {
                    foreach (var spriteRenderer in spriteRenderers)
                    {
                        spriteRenderer.gameObject.layer = minimapLayer;
                    }
                }
            }
        }

        private void ApplyLayerMask(Camera camera, LayerMask layerMask)
        {
            if (camera == null) return;
            camera.cullingMask = layerMask;
        }

        private void MoveGroundObjects(GameObject parentObj, GameObject groundObj)
        {
            groundObj.transform.SetParent(parentObj.transform);
        }

        protected void OnSubscribe()
        {
            //CustomerRepo.currentPlace.AddObserver(this, OnPlaceUpdate);
            //CustomerRepo.currentSpot.AddObserver(this, OnSpotUpdate);
            //XRViewModel.trackerStartEvent.AddObserver(this, StartTrackerWithRefreshSubscribe);
            //XRViewModel.trackerStopEvent.AddObserver(this, StopTracker);
            //XRViewModel.InitRecognitionState.AddObserver(this, OnInitRecognitionState);
        }

        protected void OnInitRecognitionState()
        {
            CustomerRepo.CachePoIList?.Clear();
            vPSTrackablesList.Clear();
            var IbrManager = FindObjectOfType<SmoothIbrManager>(true);
            if (IbrManager) 
            {
                IbrManager.ClearAllSplitMaterial(true);
            }
            currentLocalizerLocation = string.Empty;
            XrSettings.LocalizerLocation.Value = string.Empty;
            XrSettings.OldNavigationLocation.Value = string.Empty;
            XrSettings.NavigationLocation.Value = string.Empty;
            SceneViewModel.CurrentRecognitionState.Value = ARLocationRecognitionState.ARLocationRecognitionStateNotAvailable;
        }

        protected void OnUnsubscribe()
        {
            //CustomerRepo.currentPlace.RemoveAllObserver(this);
            //CustomerRepo.currentSpot.RemoveAllObserver(this);
            //XRViewModel.trackerStartEvent.RemoveAllObserver(this);
            //XRViewModel.trackerStopEvent.RemoveAllObserver(this);
            //XRViewModel.InitRecognitionState.RemoveAllObserver(this);
        }

        private void OnPlaceUpdate(Place place)
        {
            //if (!CustomerRepo.currentPlace.IsNew) return;
            if (place == null) return;

            switch (trackerStatus)
            {
                case TrackerStatus.TrackingStart:
                    StopTracker();
                    StartTracker();
                    break;
                case TrackerStatus.TrackingStartDelayed:
                    StartTracker();
                    break;
                default:
                    break;
            }
            //XRAPIUtil.ConfigPlace(place);
        }

        private void OnSpotUpdate(Spot spot)
        {
            //if (!CustomerRepo.currentSpot.IsNew) return;
            if (spot == null) return;
            //XRAPIUtil.ConfigSpot(spot);
            //XRAPIUtil.ConfigAddDeviceInfo();
        }

        private void SubscribeToken()
        {
            tokenSubscription = TokenRepo.Instance
                .clientTokenReactiveProperty
                .DistinctUntilChanged()
                .Subscribe((credentialsToken) =>
                {
                    if (!string.IsNullOrEmpty(credentialsToken?.access_token))
                    {
                        TrackerManager.GetInstance().SetAccessToken(credentialsToken?.access_token);
                        Debug.Log($"BaseSceneManager SubscribeToken : {credentialsToken?.access_token}");
                    }
                });
        }

        private void UnsubscribeToken()
        {
            if (tokenSubscription != null)
            {
                tokenSubscription.Dispose();
                tokenSubscription = null;
            }
        }
    }
}
