using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaxstXR.Place
{
    public class InputEventEnv
    {
        private Vector3 minPos = Vector3.zero;
        private Vector3 maxPos = Vector3.zero;

        public bool IsEstimation { get; private set; } = false;
        public float Aspect { get; private set; } = 1.777778F;
        public float MinZoom { get; private set; } = 5F;
        public float MaxZoom { get; private set; } = 8F;
        public float ZoomSensitivity { get; private set; } = 1F;
        public float VisibleWidth { get; private set; } = 1F;
        public float VisibleHeight { get; private set; } = 1F;
        public float RenderWidth { get; private set; } = 1F;
        public float RenderHeight { get; private set; } = 1F;
        public float ScreenWidth { get; private set; } = 1F;
        public float ScreenHeight { get; private set; } = 1F;
        public string CurrentNavigationLocation { get; private set; } = "";
        public float Offset { get; set; } = 0F;

        public readonly Dictionary<string, Bounds> trackableBounds = new Dictionary<string, Bounds>();

        public void UpdateCameraAspect(float aspect)
        {
            Aspect = aspect;
        }

        public bool UpdateCurrentNavigation(string navigationLocation)
        {
            CurrentNavigationLocation = navigationLocation;
            return UpdateCurrentLimitRange();
        }

        public void UpdateVisibleSize(float w, float h)
        {
            VisibleWidth = w == 0F ? 1F : w;
            VisibleHeight = h == 0F ? 1F : h;
        }

        public void UpdateScreenSize(float w, float h)
        {
            ScreenWidth = w == 0F ? 1F : w;
            ScreenHeight = h == 0F ? 1F : h;
        }

        public bool UpdateTrackableBounds(Dictionary<string, Bounds> bounds)
        {
            trackableBounds.Clear();
            foreach (var entry in bounds)
            {
                trackableBounds.Add(entry.Key, entry.Value);
            }
            return UpdateCurrentLimitRange();
        }

        public void AdjustmentPosition(ref Vector3 pos)
        {
            //Debug.Log($"AdjustmentPosition 1 {pos}");
            pos.x = Math.Min(Math.Max(pos.x, minPos.x), maxPos.x);
            pos.z = Math.Min(Math.Max(pos.z, minPos.z), maxPos.z);
            //Debug.Log($"AdjustmentPosition 2 {pos}");
        }

        public void ResetZoomConstant()
        {
            MaxZoom = 8F;
            MinZoom = 5F;
            //ZoomSensitivity = (MaxZoom - MinZoom) * 0.05F;
            ZoomSensitivity = 0.5F;
        }
        public void UpdateZoomConstant()
        {
            if (IsEstimation)
            {
                var ws = Math.Abs(maxPos.x - minPos.x);
                var hs = Math.Abs(maxPos.z - minPos.z);
                MaxZoom = Math.Max(ws / (Aspect * 2F), hs * 0.5F);
                //MinZoom = MaxZoom > 100F ? MaxZoom * 0.05F : 5F;
                MinZoom = MaxZoom > 100F ? 70F : 5F;
            }
            else
            {
                MaxZoom = 8F;
                MinZoom = 5F;
            }
            ZoomSensitivity = (MaxZoom - MinZoom) * 0.05F;
            // ZoomSensitivity = 0.5F;
            //Debug.Log($"UpdateZoomConstant {MaxZoom}/{ZoomSensitivity}");
        }

        private bool UpdateCurrentLimitRange()
        {
            //Debug.Log($"UpdateCurrentLimitRange {CurrentNavigationLocation}");
            if (!string.IsNullOrEmpty(CurrentNavigationLocation)
                 && trackableBounds.TryGetValue(CurrentNavigationLocation, out Bounds bounds))
            {
                var center = bounds.center;
                var extents = bounds.extents;
                minPos.x = center.x - extents.x;
                minPos.z = center.z - extents.z;
                maxPos.x = center.x + extents.x;
                maxPos.z = center.z + extents.z;
                IsEstimation = true;
                //Debug.Log($"UpdateCurrentLimitRange {minPos}/{maxPos}");
            }
            else
            {
                IsEstimation = false;
            }
            return IsEstimation;
        }
    }

    public class MinimapInputOp
    {
        public InputEventEnv Env { get; private set; } = new InputEventEnv();

        protected MinimapInputOp()
        {

        }

        public void NormalizeFromScreen(ref Vector2 src)
        {
            src.x /= Env.ScreenWidth;
            src.y /= Env.ScreenHeight;
        }

#if EXAMPLE_TEMP
    public void PanandZoombyMouseorTouch(Camera camera, ref Vector2[] normalizeDelta)
    {
        if (IsPlatformStandalone())
        {
            PanorZoomAsMouse(camera, ref normalizeDelta);
        }
        else
        {
            PanorZoomAsTouch(camera, ref normalizeDelta);
        }
    }

    private void PanorZoomAsMouse(Camera camera, ref Vector2[] normalizeDelta)
    {
        if (Input.touchCount == 0)
        {
            ZoomAsWheel(camera);
        }
        else
        {
            PanTranslate(camera, ref normalizeDelta);
        }
    }

    private void PanorZoomAsTouch(Camera camera, ref Vector2[] normalizeDelta)
    {
        if (Input.touchCount == 1)
        {
            PanTranslate(camera, ref normalizeDelta);
        }
        else
        {
            ZoomAsPinch(camera);
        }
    }
#endif

        public bool IsPlatformStandalone()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WebGLPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                    return true;
                default:
                    return false;
            }
        }

        public void PanTranslate(Camera camera, ref Vector2 normalizeDelta)
        {
            var translate = new Vector3(
                normalizeDelta.x * Env.VisibleWidth,
                normalizeDelta.y * Env.VisibleHeight, 0);
            camera.transform.Translate(translate);

            if (Env.IsEstimation)
            {
                var p = camera.transform.position;
                Env.AdjustmentPosition(ref p);
                camera.transform.position = p;
            }
        }

        public bool ZoomAsWheel(Camera camera, float mouseScrollDeltaY)
        {
            if (mouseScrollDeltaY != 0F)
            {
                Env.UpdateZoomConstant();
                float targetZoom = camera.orthographicSize;
                targetZoom -= mouseScrollDeltaY * Env.ZoomSensitivity;
                targetZoom = Mathf.Clamp(targetZoom, Env.MinZoom, Env.MaxZoom);
                camera.orthographicSize = targetZoom;
                return true;
            }
            return false;
        }

        public void ZoomAsPinch(Camera camera)
        {
            Env.UpdateZoomConstant();
            Touch touchOne = Input.GetTouch(0);
            Touch touchTwo = Input.GetTouch(1);

            Vector2 touchOneOld = touchOne.position - touchOne.deltaPosition;
            Vector2 touchTwoOld = touchTwo.position - touchTwo.deltaPosition;

            float prevPinchLength = (touchOneOld - touchTwoOld).magnitude;
            float currentPinchLength = (touchOne.position - touchTwo.position).magnitude;
            float pinchOffset = currentPinchLength - prevPinchLength;
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize - pinchOffset, Env.MinZoom, Env.MaxZoom);
        }

        public void AdjustmentOrthographicSize(Camera camera)
        {
            if (camera.orthographicSize < Env.MinZoom)
            {
                camera.orthographicSize = Env.MinZoom;
            }
            else if (camera.orthographicSize > Env.MaxZoom)
            {
                camera.orthographicSize = Env.MaxZoom;
            }
        }
    }
}