using UnityEngine;

namespace MaxstXR.Place
{
    public class PovEnv : ChunkEnv
    {
        private bool isCheckKey = true;
        private MinimapElementFilter filter = new MinimapElementFilter();
        protected PovEnv() : base()
        {
            renderLimitPerFrame = 300;
        }

        public override bool IsInitialize()
        {
            return !string.IsNullOrEmpty(XrSettings.NavigationLocation.Value);
        }

        public override float UnitDistance()
        {
            return POV_RENDER_DISTANCE;
        }

        public override bool IsCheckKey()
        {
            return isCheckKey;
        }

        public override string VisibleNavigationLocation()
        {
            return "Temp";
        }

        public override ref Vector3 CurrentPosition()
        {
            return ref XrSettings.Position;
        }

        public override void UpdateMapComplete()
        {
            isCheckKey = true;
        }

        public int InboundArea()
        {
            return POV_VAILD_MAX_UNIT;
        }

        public int RenderArea()
        {
            return POV_VAILD_MAX_UNIT;
        }

        public override ChunkElementFilter GetElementFilter()
        {
            return filter;
        }
    }
}