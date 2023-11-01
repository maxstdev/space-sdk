using UnityEngine;
using UnityEngine.UI;

namespace MaxstXR.Place
{
    public class MinimapBehaviour : InjectorBehaviour
    {
        [DI(DIScope.component, DIComponent.minimap)] private MinimapViewModel MinimapViewModel { get; }

        [SerializeField] private Sprite freeSprite;
        [SerializeField] private Sprite followSprite;
        [SerializeField] private Image modeImage;

        private Sprite GetCameraModeImage(MinimapCameraMode mode) => mode == MinimapCameraMode.FollowCamera ? followSprite : freeSprite;

        private void OnEnable()
        {
            MinimapViewModel.CameraModeEvent.AddObserver(this, ApplyCameraMode); 
        }

        private void OnDisable()
        {
            MinimapViewModel.CameraModeEvent.RemoveAllObserver(this);
        }

        public void OnChagnedCameraMode()
        {
            MinimapViewModel.CameraModeEvent.Post(
                MinimapViewModel.CameraModeEvent.Value == MinimapCameraMode.FollowCamera ?
                MinimapCameraMode.Free : MinimapCameraMode.FollowCamera);
        }

        private void ApplyCameraMode(MinimapCameraMode mode)
        {
            modeImage.sprite = GetCameraModeImage(mode);
        }
    }
}
