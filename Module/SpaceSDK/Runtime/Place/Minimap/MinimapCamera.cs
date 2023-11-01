using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.XR;

namespace MaxstXR.Place
{
    public class MinimapCamera : InjectorBehaviour
    {
        public const float MINIMAP_CAMERA_POS_Y = 50F;
        public const float MINIMAP_USER_POS_Y = 30F;
        public const float MINIMAP_POI_POS_Y = 20F;
        public const float MINIMAP_NAVI_POS_Y = 0F;

        [DI(DIScope.component, DIComponent.place)] protected XrSettings XrSettings { get; }
        [DI(DIScope.component, DIComponent.place)] protected SceneViewModel SceneViewModel { get; }
        [DI(DIScope.component, DIComponent.minimap)] protected MinimapViewModel MinimapViewModel { get; }
        [DI(DIScope.component, DIComponent.minimap)] protected MinimapInputOp MinimapInputOp { get; }
        [DI(DIScope.singleton)] private MinimapPoiEnv Env { get; }

        [SerializeField] private Camera minimapCamera;
        [SerializeField] private SpriteRenderer minimapMyIconRenderer;
        [SerializeField] private List<Material> billboardOrthographicMaterials = new();

        [Tooltip("Text Mesh Pro Billboard Material Config")]
        [SerializeField] private List<Material> textMeshProBillboardSettingMaterials = new();
        [SerializeField] private Shader textMeshProBillboardShader;

        private Action cameraUpdate = null;

        private void Awake()
        {
            XrSettings.MinimapCamera = minimapCamera;
            MinimapInputOp.Env.ResetZoomConstant();
        }

        private void OnEnable()
        {
            XrSettings.MinimapCamera = minimapCamera;
            ConfigTextMeshProBillboard();
            StartCoroutine(InitCameraRenderSize());
            MinimapViewModel.CameraModeEvent.AddObserver(this, UpdateMode);
            MinimapViewModel.VisibleNormalizePoint.AddObserver(this, UpdateVisibleNormalizePoint);
            SceneViewModel.TrackableBounds.AddObserver(this, OnTrackableBounds);
            XrSettings.NavigationLocation.AsObservable().Subscribe(OnChangeNavigationLocation).AddTo(this);
        }

        private void OnDisable()
        {
            MinimapViewModel.CameraModeEvent.RemoveAllObserver(this);
            MinimapViewModel.VisibleNormalizePoint.RemoveAllObserver(this);
            SceneViewModel.TrackableBounds.RemoveAllObserver(this);
        }

        private void Update()
        {
            var s = Env.RelativeScale().x;
            if (s > 0)
            {
                foreach (var m in billboardOrthographicMaterials)
                {
                    if (m) m.SetFloat("_Orthographic_Size", s);
                }
            }
        }

        private void LateUpdate()
        {
            cameraUpdate?.Invoke();
        }

        private void ConfigTextMeshProBillboard()
        {
            if (textMeshProBillboardShader && textMeshProBillboardSettingMaterials.IsNotEmpty())
            {
                foreach (var m  in textMeshProBillboardSettingMaterials)
                {
                    if (m) m.shader = textMeshProBillboardShader;
                }
            }
        }

        private void UpdateVisibleNormalizePoint(Vector2 lbPoint, Vector2 rtPoint, Vector2 rbPoint)
        {
            if (!MinimapViewModel.VisibleNormalizePoint.IsNew) return;
            MinimapViewModel.MeasureCameraVisibleSize(minimapCamera, ref lbPoint, ref rtPoint, ref rbPoint);
        }

        private void OnChangeNavigationLocation(string navigationLocation)
        {
            if (MinimapInputOp.Env.UpdateCurrentNavigation(navigationLocation))
            {
                MinimapInputOp.Env.UpdateZoomConstant();
                MinimapInputOp.AdjustmentOrthographicSize(minimapCamera);
                MinimapViewModel.MeasureCameraRenderSize(minimapCamera);
                MinimapViewModel.MeasureCameraVisibleSize(minimapCamera);
            }
        }

        private void OnTrackableBounds(Dictionary<string, Bounds> bounds)
        {
            if (MinimapInputOp.Env.UpdateTrackableBounds(bounds))
            {
                MinimapInputOp.Env.UpdateZoomConstant();
                MinimapInputOp.AdjustmentOrthographicSize(minimapCamera);
                MinimapViewModel.MeasureCameraRenderSize(minimapCamera);
                MinimapViewModel.MeasureCameraVisibleSize(minimapCamera);
            }
        }

        private void UpdateMode(MinimapCameraMode mode)
        {
            switch (mode)
            {
                case MinimapCameraMode.FollowCamera:
                    cameraUpdate = FollowXRCamera;
                    break;
                case MinimapCameraMode.Free:
                    cameraUpdate = FreelyXRCamera;
                    break;
                default:
                    break;
            }
        }

        private void FollowXRCamera()
        {
            var pos = XrSettings.Position;
            var q = XrSettings.Rotation;

            pos.y = MINIMAP_CAMERA_POS_Y;
            transform.position = pos;
            
            var temp = Quaternion.Euler(90, q.eulerAngles.y, 0);
            transform.rotation = temp;
            minimapMyIconRenderer.transform.localPosition = new Vector3(0, 0, 10);
            minimapMyIconRenderer.transform.localRotation = Quaternion.identity;
            UpdateLocation(ref pos, ref temp);

            AdjustSize();
            AdjustOffset();
        }

        private void AdjustOffset()
        {
            var movement = (MinimapInputOp.Env.Offset * MinimapInputOp.Env.VisibleHeight) * -minimapCamera.transform.forward;
            transform.Translate(movement);
        }

        private void FreelyXRCamera()
        {
            var pos = XrSettings.Position;
            var q = XrSettings.Rotation;

            pos.y = MINIMAP_CAMERA_POS_Y;

            minimapMyIconRenderer.transform.position = pos;
            minimapMyIconRenderer.transform.Translate(new Vector3(0, 0, 10), Space.Self);
            minimapMyIconRenderer.transform.rotation = Quaternion.Euler(90, q.eulerAngles.y, 0);

            var temp = minimapMyIconRenderer.transform.rotation;
            UpdateLocation(ref pos, ref temp);

            AdjustSize();
        }

        private void AdjustSize()
        {
            
            var s = Env.RelativeScale().x * 5;
            if (s > 0)
            {
                minimapMyIconRenderer.size = new Vector2(s, s);
            }

        }

        private void UpdateLocation(ref Vector3 pos, ref Quaternion q)
        {
            XrSettings.SetMinimapCameraPose(ref pos, ref q);
        }

        private IEnumerator InitCameraRenderSize()
        {
            yield return new WaitForEndOfFrame();
            MinimapInputOp.Env.UpdateCameraAspect(minimapCamera.aspect);
            MinimapViewModel.MeasureCameraRenderSize(minimapCamera);
        }
    }
}