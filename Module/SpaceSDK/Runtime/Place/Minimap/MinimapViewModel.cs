using MaxstUtils;
using UnityEngine;
using UnityEngine.Events;

namespace MaxstXR.Place
{
    public enum MinimapCameraMode
    {
        FollowCamera,
        Free
    }

    public class MinimapViewModel
    {
        public readonly LiveEvent<bool> ChangeCurrentModeEvent = new LiveEvent<bool>();
        public readonly Event<bool> MinimapMaximizeEvent = new Event<bool>();
        public readonly LiveEvent<bool> DragAllow = new LiveEvent<bool>();
        public readonly LiveEvent<float> ZoomInOut = new LiveEvent<float>();

        public readonly Event<MinimapCameraMode> CameraModeEvent = new Event<MinimapCameraMode>(MinimapCameraMode.FollowCamera);
        public readonly Event<Vector2, Vector2, Vector2> VisibleScreenPoint = new Event<Vector2, Vector2, Vector2>();
        public readonly Event<Vector2, Vector2, Vector2> VisibleNormalizePoint = new Event<Vector2, Vector2, Vector2>();
        public readonly LiveEvent<Vector3> CameraAnimationEvent = new LiveEvent<Vector3>();

        public readonly Event<float, float> RenderSize = new Event<float, float>();
        public readonly Event<float, float> VisibleSize = new Event<float, float>();
        //public readonly LiveEvent<Place> PlaceMapLoadComplete = new LiveEvent<Place>();
        public readonly LiveEvent<Space> SpaceMapLoadComplete = new LiveEvent<Space>();

        protected MinimapViewModel()
        {

        }

        public void ResetSize()
        {
            RenderSize.Post(0, 0);
            VisibleSize.Post(0, 0);
        }

        public void MeasureCameraRenderSize(Camera camera)
        {
            var leftBottom = camera.ViewportToWorldPoint(new Vector3(0, 0, 0));
            var rightTop = camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
            var rightBottom = camera.ViewportToWorldPoint(new Vector3(1, 0, 0));
            var renderWidth = leftBottom.Distance2D(rightBottom);
            var renderHeight = rightBottom.Distance2D(rightTop);
            RenderSize.Post(renderWidth, renderHeight);
            //Debug.Log($"MeasureCameraRenderSize RenderWidth : {renderWidth} / RenderHeight : {renderHeight}");
        }
        public void MeasureCameraVisibleSize(Camera camera)
        {
            var lbNormalizePoint = VisibleNormalizePoint.First;
            var rtNormalizePoint = VisibleNormalizePoint.Second;
            var rbNormalizePoint = VisibleNormalizePoint.Third;
            MeasureCameraVisibleSize(camera,
                ref lbNormalizePoint, ref rtNormalizePoint, ref rbNormalizePoint);
        }
        public void MeasureCameraVisibleSize(Camera camera, ref Vector2 lbNormalizePoint,
            ref Vector2 rtNormalizePoint, ref Vector2 rbNormalizePoint)
        {
            var leftBottom = camera.ViewportToWorldPoint(lbNormalizePoint);
            var rightTop = camera.ViewportToWorldPoint(rtNormalizePoint);
            var rightBottom = camera.ViewportToWorldPoint(rbNormalizePoint);
            var visibleWidth = leftBottom.Distance2D(rightBottom);
            var visibleHeight = rightBottom.Distance2D(rightTop);
            VisibleSize.Post(visibleWidth, visibleHeight);
            //Debug.Log($"MeasureCameraVisibleSize VisibleWidth : {visibleWidth} / VisibleHeight : {visibleHeight}");
        }
    }
}