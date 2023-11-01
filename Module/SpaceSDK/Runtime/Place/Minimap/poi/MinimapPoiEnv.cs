using System;
using UnityEngine;

namespace MaxstXR.Place
{
    public class MinimapPoiEnv : ChunkEnv
    {
        private const float SCALE_SCALAR = 0.02F;
        private const float WIDTH_SCALAR = 0.015F;
        private int renderUnit = 1;
        private int inboundUnit = 1;
        private bool isCheckKey = true;
        private readonly MinimapElementFilter filter = new MinimapElementFilter();

        protected MinimapPoiEnv() : base()
        {

        }

        public void UpdateRenderSize(float w, float h)
        {
            var s = Math.Max(w, h) * SCALE_SCALAR;
            relativeScale.Set(s, s, s);
            relativeWidth = Math.Max(w, h) * WIDTH_SCALAR;
        }

        public void UpdateVisibleSize(float w, float h)
        {
            var s = Math.Max(w, h);
            renderUnit = Mathf.FloorToInt(s / MINIMAP_RENDER_DISTANCE);
            inboundUnit = renderUnit + MINIMAP_VALID_ALPHA_UNIT;
            isCheckKey = false;
            //Debug.Log($"MinimapPoiEnv UpdateVisibleSize {s}/{renderUnit}/{inboundUnit}");
        }

        public override bool IsInitialize()
        {
            return !string.IsNullOrEmpty(XrSettings.NavigationLocation.Value);
        }

        public override float UnitDistance()
        {
            return MINIMAP_RENDER_DISTANCE;
        }

        public override bool IsCheckKey() {
            return isCheckKey; 
        }

        public override string VisibleNavigationLocation()
        {
            return XrSettings.NavigationLocation.Value;
        }

        public override ref Vector3 CurrentPosition()
        {
            return ref XrSettings.MinimapPosition;
        }

        public override void UpdateMapComplete() 
        {
            isCheckKey = true;
        }

        public int InboundArea()
        {
            return inboundUnit;
        }

        public int RenderArea()
        {
            return renderUnit;
        }
        public override ChunkElementFilter GetElementFilter()
        {
            return filter;
        }
    }
}
