using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace MaxstXR.Extension
{

    public class Transition
    {
        public const float WAIT_EFFECT_START = 0.3f;
        public const float WAIT_TIMEOUT = 3f;

        public PovController ToPov { get; private set; }
        public PovController FromPov { get; private set; }

        public Vector3 FromPosition { get; private set; } = Vector3.zero;
        public Vector3 ToPosition { get; private set; } = Vector3.zero;
        public Quaternion FromRotation { get; private set; } = Quaternion.identity;
        public Quaternion ToRotation { get; private set; } = Quaternion.identity;
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
        

        public Transition(PovController fromPov, PovController toPov,
            Quaternion fromQuaternion, Quaternion toQuaternion,
            float durationTimeAtPos, float durationTimeAtRotate,
            CancellationTokenSource cancellation, ITransitionDelegate transitionDelegate)
        {
            this.FromPov = fromPov;
            this.ToPov = toPov;
            FromPosition = fromPov.transform.position;
            ToPosition = toPov.transform.position;
            keepPosition = FromPosition == ToPosition;
            FromRotation = fromQuaternion;
            ToRotation = toQuaternion;
            keepRotation = FromRotation == ToRotation;
            timeScaleAtPos = durationTimeAtPos == 0f ? 100f : 1.0f / durationTimeAtPos;
            timeScaleAtRotate = durationTimeAtRotate == 0f ? 100f : 1.0f / durationTimeAtRotate;
            this.cancellation = cancellation;
            TransitionDelegate = transitionDelegate;
            //Debug.Log($"Transition Config fromPov -> toPov : {fromPov.gameObject.name} /\n {toPov.gameObject.name}");
            //Debug.Log($"Transition Config position : {fromPosition}/{toPosition}");
        }

        public void UpdatePositionAndRotation(Transform transform)
        {
            var nextPosition = Vector3.Lerp(FromPosition, ToPosition, keepPosition ? 0 : Mathf.Min(timeAtPos, 1.0f));
            var nextRotation = Quaternion.Slerp(FromRotation, ToRotation, keepRotation ? 0 : Mathf.Min(timeAtRotate, 1.0f));
            //Debug.Log($"Transition UpdatePositionAndRotation fromPov -> toPov : {fromPov.gameObject.name} /\n {toPov.gameObject.name}");
            //Debug.Log($"Transition UpdatePositionAndRotation : {fromPosition}/{toPosition}/{nextPosition}/{time}");
            transform.SetPositionAndRotation(nextPosition, nextRotation);
        }

        public void UpdateAnimationParameter(UnityEvent<float, float> aniTimeEvent)
        {
            timeAtPos = Mathf.Clamp01(timeAtPos + Time.smoothDeltaTime * timeScaleAtPos);
            timeAtRotate = Mathf.Clamp01(timeAtRotate + Time.smoothDeltaTime * timeScaleAtRotate);
            aniTimeEvent?.Invoke(timeAtPos, timeAtRotate);
            //Debug.Log($"Transition UpdateAnimationParameter : {timeAtPos}/{timeAtRotate}");
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
