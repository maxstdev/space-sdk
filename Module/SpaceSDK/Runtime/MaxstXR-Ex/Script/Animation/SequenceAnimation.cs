using Cysharp.Threading.Tasks;
using MaxstXR.Place;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace MaxstXR.Extension
{
    public class SequenceAnimation : PovAnimation
    {
        public enum SeqStatus
        {
            Idle = 100,
            Play,
            Pause,
            Stop,
        }

        [SerializeField, HideInInspector] private List<ISequenceProperty> sequences = new List<ISequenceProperty>();

        [field: SerializeField] public UnityEvent<ISequenceProperty> SequenceStart { get; private set; } = new UnityEvent<ISequenceProperty>();
        [field: SerializeField] public UnityEvent<ISequenceProperty> SequenceResume { get; private set; } = new UnityEvent<ISequenceProperty>();
        [field: SerializeField] public UnityEvent<ISequenceProperty> SequencePause { get; private set; } = new UnityEvent<ISequenceProperty>();
        [field: SerializeField] public UnityEvent<ISequenceProperty> SequenceStop { get; private set; } = new UnityEvent<ISequenceProperty>();
        [field: SerializeField] public UnityEvent<ISequenceProperty> SequenceUpdate { get; private set; } = new UnityEvent<ISequenceProperty>();
        [field: SerializeField, HideInInspector] public ISequenceProperty CurrentSequence { get; protected set; } = null;
        public ReactiveProperty<SeqStatus> SequenceStatus { get; } = new ReactiveProperty<SeqStatus>(SeqStatus.Idle);

        private string CurrentMapKey;
        private string CurrentUniqueNameKey;

        protected override void Awake()
        {
            base.Awake();
            AnimationStatus.AsObservable().Subscribe(s =>
            {
                switch (s)
                {
                    case PovStatus.Play:
                        OnReceivedPovPlay();
                        break;
                    case PovStatus.Pause:
                        OnReceivedPovPause();
                        break;
                    case PovStatus.Stop:
                        OnReceivedPovStop();
                        break;
                }
            }).AddTo(this);
        }

        public async void Config(string currentMapKey, string currentUniqueNameKey,  List<ISequenceProperty> sp, UnityAction complete)
        {
            CurrentMapKey = currentMapKey;
            CurrentUniqueNameKey = currentUniqueNameKey;
            await UniTask.WaitUntil(() => smoothCameraManager);
            sequences.Clear();
            sequences.AddRange(sp);
            SequenceStatus.Value = SeqStatus.Idle;
            complete?.Invoke();
        }

        public async void StepProcess(ISequenceProperty property = null)
        {
            frames.Clear();

            // check if prev selected
            bool hasPrev = false;
            ISequenceProperty PrevSequence = null;

            if (CurrentSequence != null)
            {
                hasPrev = true;
                PrevSequence = CurrentSequence;
            }

            if (property != null)
            {
                CurrentSequence = sequences[property.Index];
                await ConvertSequenceProperty(property, smoothCameraManager, frames);
            }
            else
            {
                CurrentSequence = sequences.FirstOrDefault();
                await ConvertSequenceProperty(sequences.FirstOrDefault(), smoothCameraManager, frames);
            }

            SequenceUpdate.Invoke(CurrentSequence);

            // deselect prev frame
            if (hasPrev)
            {
                PrevSequence.IsSelect.Value = false;
            }

            CurrentSequence.IsSelect.Value = true;
            //Debug.Log($"StepProcess : {property?.Index ?? 0}/{frames.Count}");
            Procedure();
        }

        public async UniTask ReqPlaySequence()
        {
            if (SequenceStatus.Value != SeqStatus.Play)
                await AnimationPlay();
            else
                throw new OperationCanceledException("SequenceStatus is Play");
        }

        public async UniTask ReqPauseSequence()
        {
            if (SequenceStatus.Value == SeqStatus.Play)
                await AnimationPause();
            else
                throw new OperationCanceledException("SequenceStatus is not Play");
        }

        public async UniTask ReqStopSequence()
        {
            if (SequenceStatus.Value == SeqStatus.Play)
                await AnimationForceStop();
            else
                throw new OperationCanceledException("SequenceStatus is not Play");
        }

        private async UniTask ConvertSequenceProperty(ISequenceProperty sequence, SmoothCameraManager smoothCameraManager, List<PovKeyFrame> result)
        {
            if (sequence == null) return;
            var index = result.Count - 1;
            if (sequence.TransitionMode == TransitionMode.SlideShow)
            {
                var nextPov = smoothCameraManager.KnnManager.FindNearest(sequence.FramePosition).GetComponent<PovController>();
                var rotate = sequence.Quaternions.IsEmpty() ? smoothCameraManager.transform.rotation : sequence.Quaternions.First();
                var wrapFrame = new PovKeyFrame(++index, smoothCameraManager.povController, nextPov, rotate, rotate);
                wrapFrame.KeyFrameType = KeyFrameType.WrapWith8K;
                wrapFrame.DurationTimeAtPos = SmoothCameraManager.DefaultSecondAtPos;
                wrapFrame.DurationTimeAtRotate = SmoothCameraManager.DefaultSecondAtRotate;
                result.Add(wrapFrame);
            }
            else
            {
                if (sequence.Index == 0)
                {
                    var paths = await Request(sequence, smoothCameraManager);
                    KeyFrameGenerator.ConvertPathModel(paths, smoothCameraManager, result);
                }
                else
                {
                    var previous = sequences[sequence.Index - 1];
                    var paths = await Request(previous, sequence);
                    KeyFrameGenerator.ConvertPathModel(paths, smoothCameraManager, result, sequence);
                }
            }

            KeyFrameGenerator.ConvertSequenceProperty(sequence, smoothCameraManager, result);
        }

        private void OnReceivedPovPlay()
        {
            switch (SequenceStatus.Value)
            {
                case SeqStatus.Idle:
                case SeqStatus.Stop:
                    SequenceStatus.Value = SeqStatus.Play;
                    SequenceStart?.Invoke(CurrentSequence);
                    break;
                case SeqStatus.Pause:
                    SequenceStatus.Value = SeqStatus.Play;
                    SequenceResume?.Invoke(CurrentSequence);
                    break;
                default:
                    break;
            }
        }

        private void OnReceivedPovPause()
        {
            switch (SequenceStatus.Value)
            {
                case SeqStatus.Play:
                    SequenceStatus.Value = SeqStatus.Pause;
                    SequencePause?.Invoke(CurrentSequence);
                    break;
                default:
                    break;
            }
        }

        private void OnReceivedPovStop()
        {
            switch (SequenceStatus.Value)
            {
                case SeqStatus.Play:
                    if (cancellationTokenSoruce == null)
                    {
                        CurrentSequence = sequences.Last();
                        SequenceStatus.Value = SeqStatus.Stop;
                        SequenceStop?.Invoke(CurrentSequence);
                        return;
                    }

                    ProcessKeepGoing(CurrentSequence, out var isStop);
                    if (isStop)
                    {
                        SequenceStatus.Value = SeqStatus.Stop;
                        SequenceStop?.Invoke(CurrentSequence);
                    }
                    break;
                default:
                    break;
            }
        }

        private async UniTask<List<PathModel>> Request(ISequenceProperty property, SmoothCameraManager smoothCameraManager)
        {
            if (string.IsNullOrEmpty(CurrentMapKey))
            {
                Debug.Log("CurrentSpace is null");
            }

            var completionSource = new TaskCompletionSource<List<PathModel>>();
            SpaceNavigationController.FindPath(this,
                    XRTokenManager.Instance.GetHeaders(),
                    CurrentMapKey,
                    smoothCameraManager.povController.transform.position,
                    CurrentMapKey,
                    property.FramePosition, 2.0f,
                    (paths) =>
                    {
                        completionSource.SetResult(paths[paths.Keys.First()]);
                    },
                    () =>
                    {
                        Debug.LogError("No Path");
                        SequenceStatus.Value = SeqStatus.Stop;
                        completionSource.SetException(new Exception("No Path"));
                    },
                    CurrentUniqueNameKey);

            return await completionSource.Task;
        }

        private async UniTask<List<PathModel>> Request(ISequenceProperty previous, ISequenceProperty current)
        {
            if (string.IsNullOrEmpty(CurrentMapKey))
            {
                throw (new Exception($"space ID is null"));
            }

            var completionSource = new TaskCompletionSource<List<PathModel>>();
            SpaceNavigationController.FindPath(this,
                    XRTokenManager.Instance.GetHeaders(),
                    CurrentMapKey,
                    previous.FramePosition,
                    CurrentMapKey,
                    current.FramePosition, 2.0f,
                    (paths) =>
                    {
                        completionSource.SetResult(paths[paths.Keys.First()]);
                    },
                    () =>
                    {
                        Debug.LogError("No Path");
                        SequenceStatus.Value = SeqStatus.Stop;
                        completionSource.SetException(new Exception("No Path"));
                    },
                    CurrentUniqueNameKey);

            return await completionSource.Task;
        }

        private void ProcessKeepGoing(ISequenceProperty sequenceProperty, out bool isStoped)
        {
            if (IsAnimationStop())
            {
                isStoped = true;
                return;
            }

            var isNext = sequenceProperty.Index + 1 != sequences.Count;
            //Debug.LogWarning($"ProcessKeepGoing : => {sequenceProperty.Index}, {isNext}");
            if (isNext)
            {
                StepProcess(sequences[sequenceProperty.Index + 1]);
            }
            else
            {
                //all stop
                isStoped = true;
                //CurrentSequence = sequences.First();
                return;
            }
            isStoped = false;
            //processIndex
        }
    }
}
