using System;
using UnityEngine;
using static MaxstXR.Extension.PovAnimation;

namespace MaxstXR.Extension
{
    public enum KeyFrameSource
    {
        Editor = 0,
        Animation,
        LifeCycle,
        Keyboard,
        MouseOrTouch,
        ExternalValue,
    }

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
        public PovController CurrentPov { get; private set; } = null;
        public PovController NextPov { get; private set; } = null;
        public Vector3 CurrentPosition { get; private set; } = Vector3.zero;
        public Vector3 NextPosition { get; private set; } = Vector3.zero;
        public Quaternion? CurrentRotate { get; private set; } = null;
        public Quaternion? NextRotate { get; private set; } = null;
        public KeyFrameSource KeyFrameSource { get; set; } = KeyFrameSource.Animation; 
        public KeyFrameType KeyFrameType { get; set; } = KeyFrameType.Continuous;
        public bool IsLastKeyFrame { get; set; } = false;
        public float DurationTimeAtPos { get; set; } = 0f;
        public float DurationTimeAtRotate { get; set; } = 0f;

        public TransitionStatus KeyFrameStatus { get; set; } = TransitionStatus.Idle;
        public PovStatus RequestCancelStatus { get; set; } = PovStatus.Idle;

        public PovKeyFrame(PovController currentPov, PovController nextPov)
        {
            Index = 0;
            CurrentPov = currentPov;
            NextPov = nextPov;
        }

        public PovKeyFrame(PovController currentPov, PovController nextPov, Quaternion? currentRotate, Quaternion? nextRotate)
        {
            Index = 0;
            CurrentPov = currentPov;
            NextPov = nextPov;
            CurrentPosition = CurrentPov.transform.position;
            NextPosition = NextPov.transform.position;
            CurrentRotate = currentRotate;
            NextRotate = nextRotate;
        }

        public PovKeyFrame(int index, PovController currentPov, PovController nextPov, Quaternion? currentRotate)
        {
            Index = index;
            CurrentPov = currentPov;
            NextPov = nextPov;
            CurrentPosition = CurrentPov.transform.position;
            NextPosition = NextPov.transform.position;
            CurrentRotate = currentRotate;
            NextRotate = CurrentRotate.HasValue ? CurrentPosition.ToRotate(NextPosition, (Quaternion)CurrentRotate) : null;
        }

        public PovKeyFrame(int index, PovController currentPov, PovController nextPov, Quaternion? currentRotate, Quaternion? nextRotate)
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
            //Always if the current rotation value is set
            NextRotate = CurrentPosition.ToRotate(NextPosition, CurrentRotate.Value);
        }

        public void CalulateDurationTime(float durationTimeAtPos = 0, float durationTimeAtRotate = 0)
        {
            DurationTimeAtPos = durationTimeAtPos == 0f ?
                Vector3.Distance(CurrentPosition, NextPosition) / SmoothCameraManager.DistancePerSecond : durationTimeAtPos;
            if (durationTimeAtRotate == 0f && CurrentRotate.HasValue) 
            {
                if (!NextRotate.HasValue) NextRotate = CurrentPosition.ToRotate(NextPosition, CurrentRotate.Value);
                DurationTimeAtRotate = Quaternion.Angle(CurrentRotate.Value, NextRotate.Value) / SmoothCameraManager.RotatePerSecond;
            }
            else
            {
                DurationTimeAtRotate = durationTimeAtRotate;
            }
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

        public void UpdateRotate(Quaternion currentQuaternion, Quaternion nextQuaternion)
        {
            this.CurrentRotate = currentQuaternion;
            this.NextRotate = nextQuaternion;
        }

    }
}
