using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace MaxstXR.Extension
{

    public class Transition
    {
        public const float WAIT_EFFECT_START = 0.3f;
        public const float WAIT_TIMEOUT = 3f;
        public PovKeyFrame KeyFrame { get; private set; }
        public ITransitionDelegate TransitionDelegate { get; private set; } = null;
        public object SourceObject { get; set; } = null;

        private readonly bool keepPosition = false;
        private readonly bool keepRotation = false;
        private readonly float timeScaleAtPos = 1.0f;
        private readonly float timeScaleAtRotate = 1.0f;
        private float timeAtPos = 0;
        private float timeAtRotate = 0;
        private float waitTime = 0;

        public bool isReady = false;
        public SmoothSharedTexture sharedTexture = null;
        public CancellationTokenSource cancellation = null;
        public HighQualityState highQualityState = HighQualityState.Idle;
        

        public Transition(PovKeyFrame keyFrame, CancellationTokenSource cancellation, ITransitionDelegate transitionDelegate)
        {
            KeyFrame = keyFrame;
            keepPosition = KeyFrame.CurrentPosition == KeyFrame.NextPosition;
            keepRotation = KeyFrame.CurrentRotate == KeyFrame.NextRotate;
            timeScaleAtPos = KeyFrame.DurationTimeAtPos == 0f ? 100f : 1.0f / KeyFrame.DurationTimeAtPos;
            timeScaleAtRotate = KeyFrame.DurationTimeAtRotate == 0f ? 100f : 1.0f / KeyFrame.DurationTimeAtRotate;
            this.cancellation = cancellation;
            TransitionDelegate = transitionDelegate;
            //Debug.Log($"Transition Config fromPov -> toPov : {fromPov.gameObject.name} /\n {toPov.gameObject.name}");
            //if (keepPosition)
            //    Debug.Log($"Transition Config position : {fromPosition}/{toPosition}");
            //if (keepRotation)
            //    Debug.Log($"Transition Config rotation : {KeyFrame.CurrentRotate.eulerAngles}/{KeyFrame.NextRotate.eulerAngles}");
        }

        public void UpdatePositionAndRotation(Transform transform)
        {
            var nextPosition = Vector3.Lerp(KeyFrame.CurrentPosition, KeyFrame.NextPosition, keepPosition ? 0 : Mathf.Min(timeAtPos, 1.0f));
            var nextRotation = Quaternion.Slerp(KeyFrame.CurrentRotate.Value, KeyFrame.NextRotate.Value, keepRotation ? 0 : Mathf.Min(timeAtRotate, 1.0f));
            //Debug.Log($"Transition UpdatePositionAndRotation fromPov -> toPov : {fromPov.gameObject.name} /\n {toPov.gameObject.name}");
            //Debug.Log($"Transition UpdatePositionAndRotation : {fromPosition}/{toPosition}/{nextPosition}/{time}");
            transform.SetPositionAndRotation(nextPosition, nextRotation);
        }

        public void UpdateAnimationParameter(UnityEvent<float, float> aniTimeEvent)
        {
            timeAtPos = Mathf.Clamp01(timeAtPos + Time.smoothDeltaTime * timeScaleAtPos);
            timeAtRotate = Mathf.Clamp01(timeAtRotate + Time.smoothDeltaTime * timeScaleAtRotate);
            aniTimeEvent?.Invoke(timeAtPos, timeAtRotate);
            //Debug.Log($"Transition UpdateAnimationParameter : {timeAtPos}/{timeAtRotate}/{timeScaleAtPos}/{timeScaleAtRotate}");
        }

        public void ForceTimeEnd()
        {
            timeAtPos = 1.0f;
            timeAtRotate = 1.0f;
        }

        public bool IsAnimationStart()
        {
            return timeAtPos == 0.0f;
        }

        public bool IsAnimationEnd()
        {
            return false == (timeAtPos < 1.0f || timeAtRotate < 1.0f);
        }

        public bool OnWaitForReady()
        {
            waitTime += Time.deltaTime;
            return waitTime > WAIT_TIMEOUT;
        }
    }
}
