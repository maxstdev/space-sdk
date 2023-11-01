using System;
using UnityEngine;

namespace MaxstXR.Extension
{

    public enum KeyFrameType
    {
        Continuous = 0,
        ContinuousWith8K,
        Wrap,
        WrapWith8K,
    }

    [Serializable]
    public class PovKeyFrame
    {
        public int Index { get; private set; } = 0;
        public PovController CurrentPov { get; private set; }
        public PovController NextPov { get; private set; }
        public Vector3 CurrentPosition { get; private set; }
        public Vector3 NextPosition { get; private set; }
        public Quaternion CurrentRotate { get; private set; }
        public Quaternion NextRotate { get; private set; }

        public KeyFrameType KeyFrameType { get; set; } = KeyFrameType.Continuous;
        public bool IsLastKeyFrame { get; set; } = false;
        public float DurationTimeAtPos { get; set; } = 0f;
        public float DurationTimeAtRotate { get; set; } = 0f;

        public PovKeyFrame(int index, PovController currentPov, PovController nextPov, Quaternion currentRotate)
        {
            Index = index;
            CurrentPov = currentPov;
            NextPov = nextPov;
            CurrentPosition = CurrentPov.transform.position;
            NextPosition = NextPov.transform.position;
            CurrentRotate = currentRotate;
            NextRotate = CurrentPosition.ToRotate(NextPosition, CurrentRotate);
        }

        public PovKeyFrame(int index, PovController currentPov, PovController nextPov, Quaternion currentRotate, Quaternion nextRotate)
        {
            Index = index;
            CurrentPov = currentPov;
            NextPov = nextPov;
            CurrentPosition = CurrentPov.transform.position;
            NextPosition = NextPov.transform.position;
            CurrentRotate = currentRotate;
            NextRotate = nextRotate;
        }

        public PovKeyFrame(int index, SmoothCameraManager smoothCameraManager, PovController nextPov)
        {
            Index = index;
            CurrentPov = smoothCameraManager.povController;
            NextPov = nextPov;
            CurrentPosition = CurrentPov.transform.position;
            NextPosition = NextPov.transform.position;
            CurrentRotate = smoothCameraManager.transform.rotation;
            NextRotate = CurrentPosition.ToRotate(NextPosition, CurrentRotate);
        }

        public HighQualityState GetHighQualityState()
        {
            return KeyFrameType switch
            {
                KeyFrameType.WrapWith8K => HighQualityState.Download,
                KeyFrameType.ContinuousWith8K => HighQualityState.Download,
                _ => HighQualityState.Cancel,
            };
        }

    }
}
