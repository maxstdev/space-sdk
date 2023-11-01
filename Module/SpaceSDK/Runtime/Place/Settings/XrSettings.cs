using UniRx;
using UnityEngine;

namespace MaxstXR.Place
{
    public class XrSettings : Injector
    {
        public Camera XrCamera { get; set; } = null;
        public Camera MinimapCamera { get; set; } = null;

        private VPSTrackable localizerVPSTrackable = null;

        private Quaternion rotation = Quaternion.identity;
        private Vector3 position = Vector3.zero;

        private Quaternion minimapRotation = Quaternion.identity;
        private Vector3 minimapPosition = Vector3.zero;

        private Vector3 poi3dsignEulerAngles = Vector3.zero;

        private Vector3 minimapEulerAngles = Vector3.zero;
        private Vector3 minimapPoiEulerAngles = Vector3.zero;
        private Vector3 minimapAnchorEulerAngles = Vector3.zero;

        protected XrSettings()
        {

        }

        public readonly ReactiveProperty<string> LocalizerLocation = new(string.Empty);
        public readonly ReactiveProperty<string> OldNavigationLocation = new(string.Empty);
        public readonly ReactiveProperty<string> NavigationLocation = new(string.Empty);
        public readonly ReactiveProperty<SpotController> SpotController = new(null);

        public VPSTrackable LocalizerVPSTrackable { get { return localizerVPSTrackable; } set { localizerVPSTrackable = value; } }

        public ref Quaternion Rotation { get { return ref rotation; } }
        public ref Vector3 Position { get { return ref position; } }

        public ref Quaternion MinimapRotation { get { return ref minimapRotation; } }
        public ref Vector3 MinimapPosition { get { return ref minimapPosition; } }

        public ref Vector3 Poi3DsignEulerAngles { get { return ref poi3dsignEulerAngles; } }

        public ref Vector3 MinimapEulerAngles { get { return ref minimapEulerAngles; } }
        public ref Vector3 MinimapPoiEulerAngles { get { return ref minimapPoiEulerAngles; } }
        public ref Vector3 MinimapAnchorEulerAngles { get { return ref minimapAnchorEulerAngles; } }

        public void SetCameraPose(ref Vector3 position, ref Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
            SetMinimapEulerAngles(ref rotation);
            SetPoi3DEulerAngles(ref rotation);
        }

        public void SetMinimapCameraPose(ref Vector3 position, ref Quaternion rotation)
        {
            this.minimapPosition = position;
            this.minimapRotation = rotation;
        }

        private void SetMinimapEulerAngles(ref Quaternion rotation)
        {
            float rotationY = rotation.eulerAngles.y;
            minimapEulerAngles = new Vector3(90, rotationY + 90, 0);
            minimapPoiEulerAngles = new Vector3(90, rotationY, 0);
            minimapAnchorEulerAngles = new Vector3(0, 0, rotationY + 113);
        }

        private void SetPoi3DEulerAngles(ref Quaternion rotation)
        {
            float rotationY = rotation.eulerAngles.y;
            this.poi3dsignEulerAngles = new Vector3(0, rotationY, 0);
        }
    }
}
