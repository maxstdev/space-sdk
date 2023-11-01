using Castle.Core.Internal;
using UnityEngine;

namespace MaxstXR.Place
{
    public class PoIEnv : ChunkEnv
    {
        private PoIElementFilter filter = new PoIElementFilter();

        protected PoIEnv() : base()
        {
            
        }

        public override bool IsInitialize()
        {
            return !string.IsNullOrEmpty(XrSettings.NavigationLocation.Value);
        }

        public override float UnitDistance()
        {
            return WORLD_RENDER_DISTANCE;
        }

        public override string VisibleNavigationLocation()
        {
            return XrSettings.NavigationLocation.Value;
        }

        public override ref Vector3 CurrentPosition()
        {
            return ref XrSettings.Position;
        }

        public override ChunkElementFilter GetElementFilter()
        {
            return filter;
        }
    }
}
