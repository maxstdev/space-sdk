using UnityEngine;

namespace MaxstXR.Place
{
    public abstract class ChunkEnv : Injector
    {
        public const float WORLD_RENDER_DISTANCE = 20F;
        public const float WORLD_VAILD_DISTANCE = 40F;

        public const float MINIMAP_RENDER_DISTANCE = 50F;
        public const int MINIMAP_VALID_ALPHA_UNIT = 1;

		public const float POV_RENDER_DISTANCE = 20F;
		public const int POV_VAILD_MAX_UNIT = 5;

		protected Vector3 relativeScale = Vector3.one;
        protected float relativeWidth = 1F;

        protected ChunkEnv() : base()
        {
        }

        [DI(DIScope.component, DIComponent.place)] protected XrSettings XrSettings { get; }

        public int renderLimitPerFrame = 100;

        public abstract bool IsInitialize();
        public abstract float UnitDistance();
        public abstract string VisibleNavigationLocation();
        public abstract ref Vector3 CurrentPosition();
        public abstract ChunkElementFilter GetElementFilter();
        public virtual bool IsCheckKey() { return true; }
        public virtual float RelativeWidth() { return relativeWidth; }
        public virtual ref Vector3 RelativeScale() { return ref relativeScale; }
        public virtual ref Quaternion RelativeRotate() { return ref XrSettings.MinimapRotation; }
        public virtual ref Vector3 Poi3dEulerAngles() { return ref XrSettings.Poi3DsignEulerAngles; }
        public virtual void UpdateMapComplete() { }
    }
}
