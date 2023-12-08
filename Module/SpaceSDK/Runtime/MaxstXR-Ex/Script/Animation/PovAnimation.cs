using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace MaxstXR.Extension
{
    public class PovAnimation : MonoBehaviour, ITransitionDelegate
    {
        public enum PovStatus
        {
            Idle = 0,
            Prepare,
            Play,
            Pause,
            Stop,
        }

        [SerializeField] protected List<PovKeyFrame> frames = new List<PovKeyFrame>();

        [field: SerializeField] public UnityEvent<PovKeyFrame> PovStart { get; private set; } = new UnityEvent<PovKeyFrame>();
        [field: SerializeField] public UnityEvent<PovKeyFrame> PovResume { get; private set; } = new UnityEvent<PovKeyFrame>();
        [field: SerializeField] public UnityEvent<PovKeyFrame> PovPause { get; private set; } = new UnityEvent<PovKeyFrame>();
        [field: SerializeField] public UnityEvent<PovKeyFrame> PovStop { get; private set; } = new UnityEvent<PovKeyFrame>();
        [field: SerializeField, HideInInspector] public PovKeyFrame CurrentKeyFrame { get; protected set; } = null;
        [SerializeField, HideInInspector] protected SmoothCameraManager smoothCameraManager;

        public ReactiveProperty<PovStatus> AnimationStatus { get; } = new ReactiveProperty<PovStatus>(PovStatus.Idle);

        private Coroutine processCoroutine = null;
        protected CancellationTokenSource cancellationTokenSoruce;

        protected virtual void Awake()
        {
            smoothCameraManager = GetComponent<SmoothCameraManager>();
        }

        public async void Generate(List<PathModel> paths, UnityAction complete)
        {
            await UniTask.WaitUntil(() => smoothCameraManager);
            AnimationStatus.Value = PovStatus.Idle;
            GenerateFrame(paths);
            AnimationStatus.Value = PovStatus.Prepare;
            complete?.Invoke();
        }

        public void Procedure(PovKeyFrame frame = null)
        {
            if (cancellationTokenSoruce != null)
            {
                cancellationTokenSoruce.Cancel();
                cancellationTokenSoruce = null;
            }

            cancellationTokenSoruce ??= new();

            StopProcessCoroutine();
            processCoroutine = StartCoroutine(FramesProcess(frame));
        }

        public async UniTask AnimationPlay()
        {
            if (frames.IsEmpty()) throw new OperationCanceledException("Frames is Empty");

            if (CurrentKeyFrame == null)
            {
                Procedure();
            }
            else if (CurrentKeyFrame.IsLastKeyFrame)
            {
                Procedure();
            }
            else
            {
                Procedure(CurrentKeyFrame);
            }

            await UniTask.WaitUntil(() =>
            {
                return AnimationStatus.Value == PovStatus.Play;
            });
            AnimationStatus.Value = PovStatus.Play;
        }

        public async UniTask AnimationPause()
        {
            if (frames.IsEmpty()) throw new OperationCanceledException("Frames is Empty");
            StopProcessCoroutine();
            CurrentKeyFrame.RequestCancelStatus = PovStatus.Pause;
            await UniTask.WaitUntil(() =>
            {
                return CurrentKeyFrame.RequestCancelStatus == PovStatus.Idle;
            });
            AnimationStatus.Value = PovStatus.Pause;
        }


        public async UniTask AnimationForceStop()
        {
            if (frames.IsEmpty()) throw new OperationCanceledException("Frames is Empty");
            cancellationTokenSoruce?.Cancel();
            StopProcessCoroutine();
            cancellationTokenSoruce = null;
            CurrentKeyFrame.RequestCancelStatus = PovStatus.Stop;
            await UniTask.WaitUntil(() =>
            {
                return CurrentKeyFrame.RequestCancelStatus == PovStatus.Idle;
            });
            AnimationStatus.Value = PovStatus.Stop;
        }

        protected bool IsAnimationStop()
        {
            if (cancellationTokenSoruce == null) return true; 
            return (cancellationTokenSoruce.IsCancellationRequested);
        }

        protected virtual void GenerateFrame(List<PathModel> paths)
        {
            frames.Clear();
            KeyFrameGenerator.ConvertPathModel(paths, smoothCameraManager, frames);
        }

        private void StopProcessCoroutine()
        {
            if (processCoroutine != null)
            {
                StopCoroutine(processCoroutine);
                processCoroutine = null;
            }
        }

        private IEnumerator FramesProcess(PovKeyFrame lastFrame = null)
        {
            if (IsAnimationStop())
            {
                StopProcessCoroutine();
                yield break;
            }

            //Debug.Log($"FramesProcess : {frames.Count}");
            int startIndex = lastFrame == null || lastFrame.IsLastKeyFrame ? 0 : lastFrame.Index + 1;

            for (var i = startIndex; i < frames.Count; ++i)
            {
                if (IsAnimationStop())
                {
                    StopProcessCoroutine();
                    yield break;
                }

                if (CurrentKeyFrame != null)
                    CurrentKeyFrame.KeyFrameStatus = TransitionStatus.Idle;
                CurrentKeyFrame = frames[i];
                smoothCameraManager.HandlePovKeyFrame(CurrentKeyFrame, this);
                UpdateStatusAtPlay(CurrentKeyFrame);
                yield return new WaitUntil(() => smoothCameraManager.IsAddableTransitions());
            }

            processCoroutine = null;
            yield break;
        }

        private void UpdateStatusAtPlay(PovKeyFrame keyFrame)
        {
            switch (AnimationStatus.Value)
            {
                case PovStatus.Pause:
                    PovResume?.Invoke(keyFrame);
                    AnimationStatus.Value = PovStatus.Play;
                    break;
                case PovStatus.Idle:
                case PovStatus.Stop:
                case PovStatus.Prepare:
                    PovStart?.Invoke(keyFrame);
                    AnimationStatus.Value = PovStatus.Play;
                    break;
                default:
                    break;
            }
        }

        void ITransitionDelegate.DownloadStart(PovKeyFrame keyFrame)
        {

        }

        void ITransitionDelegate.DownloadProgess(PovKeyFrame keyFrame, float f)
        {

        }

        void ITransitionDelegate.DownloadComplete(PovKeyFrame keyFrame)
        {

        }

        void ITransitionDelegate.DownloadException(PovKeyFrame keyFrame, Exception e)
        {

        }

        void ITransitionDelegate.DownloadException(PovKeyFrame keyFrame, UnityWebRequest www)
        {

        }

        void ITransitionDelegate.AnimationStarted(PovKeyFrame keyFrame)
        {

        }

        void ITransitionDelegate.AnimationFinished(PovKeyFrame keyFrame)
        {
            if (keyFrame.KeyFrameSource == KeyFrameSource.Animation)
            {
                switch (AnimationStatus.Value)
                {
                    case PovStatus.Pause:
                        PovPause?.Invoke(keyFrame);
                        break;
                    case PovStatus.Play:
                        if (keyFrame.IsLastKeyFrame)
                        {
                            AnimationStatus.Value = PovStatus.Stop;
                            PovStop?.Invoke(keyFrame);
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //need interrupt?? 
            }

            if (keyFrame.RequestCancelStatus != PovStatus.Idle)
            {
                keyFrame.RequestCancelStatus = PovStatus.Idle;
            }
        }
    }
}
