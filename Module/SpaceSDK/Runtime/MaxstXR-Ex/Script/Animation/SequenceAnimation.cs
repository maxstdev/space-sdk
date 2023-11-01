using Cysharp.Threading.Tasks;
using MaxstXR.Place;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace MaxstXR.Extension
{
    public class SequenceAnimation : PovAnimation
    {
        [SerializeField] private List<ISequenceProperty> sequences = new List<ISequenceProperty>();
        [SerializeField] private int processIndex = 0;

        [field: SerializeField] public UnityEvent SequenceStart { get; private set; } = new UnityEvent();
        [field: SerializeField] public UnityEvent<ISequenceProperty> SequenceResume { get; private set; } = new UnityEvent<ISequenceProperty>();
        [field: SerializeField] public UnityEvent<ISequenceProperty> SequencePause { get; private set; } = new UnityEvent<ISequenceProperty>();
        [field: SerializeField] public UnityEvent<ISequenceProperty> SequenceStop { get; private set; } = new UnityEvent<ISequenceProperty>();
        [field: SerializeField] public UnityEvent<ISequenceProperty, PovKeyFrame> SequenceKeyFrame { get; private set; } = new UnityEvent<ISequenceProperty, PovKeyFrame>();

        protected override void Awake()
        {
            base.Awake();
            PovAniStart.AddListener(OnPovAniStart);
            PovAniEnd.AddListener(OnPovAniEnd);
        }

        public async void Config(List<ISequenceProperty> sp, UnityAction complete)
        {
            await UniTask.WaitUntil(() => smoothCameraManager);
            sequences.Clear();
            sequences.AddRange(sp);
            complete?.Invoke();
        }

        public async void StepProcess(ISequenceProperty property = null)
        {
            frames.Clear();
            if (property != null)
            {
                processIndex = property.Index;
                await ConvertSequenceProperty(property, smoothCameraManager, frames);
            }
            else
            {
                processIndex = 0;
                var sequence = sequences.IsNotEmpty() ? sequences.First() : null;
                await ConvertSequenceProperty(sequence, smoothCameraManager, frames);
            }
            //Debug.Log($"StepProcess : {property?.Index ?? 0}/{frames.Count}");
            Procedure();
        }

        public void StopCurrentProcess()
        {
            frames.Clear();
            SequenceStop.Invoke(sequences[processIndex]);
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
                    KeyFrameGenerator.ConvertPathModel(paths, smoothCameraManager, result);
                }
            }

            KeyFrameGenerator.ConvertSequenceProperty(sequence, smoothCameraManager, result);
        }


        private async UniTask<List<PathModel>> Request(ISequenceProperty property, SmoothCameraManager smoothCameraManager)
        {
            var completionSource = new TaskCompletionSource<List<PathModel>>();
            SpaceNavigationController.FindPath(this,
                    XRServiceManager.Instance(gameObject).GetHeaders(),
                    XRServiceManager.Instance(gameObject).spotData?.vps_spot_name,
                    smoothCameraManager.povController.transform.position,
                    XRServiceManager.Instance(gameObject).spotData?.vps_spot_name,
                    property.FramePosition, 2.0f,
                    (paths) =>
                    {
                        completionSource.SetResult(paths[paths.Keys.First()]);
                    },
                    () =>
                    {
                        Debug.LogError("No Path");
                        completionSource.SetException(new Exception("No Path"));
                    },
                    XRServiceManager.Instance(gameObject).placeData.place_unique_name);

            return await completionSource.Task;
        }

        private async UniTask<List<PathModel>> Request(ISequenceProperty previous, ISequenceProperty current)
        {
            var completionSource = new TaskCompletionSource<List<PathModel>>();
            SpaceNavigationController.FindPath(this,
                    XRServiceManager.Instance(gameObject).GetHeaders(),
                    XRServiceManager.Instance(gameObject).spotData?.vps_spot_name,
                    previous.FramePosition,
                    XRServiceManager.Instance(gameObject).spotData?.vps_spot_name,
                    current.FramePosition, 2.0f,
                    (paths) =>
                    {
                        completionSource.SetResult(paths[paths.Keys.First()]);
                    },
                    () =>
                    {
                        Debug.LogError("No Path");
                        completionSource.SetException(new Exception("No Path"));
                    },
                    XRServiceManager.Instance(gameObject).placeData.place_unique_name);

            return await completionSource.Task;
        }

        private void OnPovAniStart(PovKeyFrame frame)
        {
            if (frame.Index == 0)
            {
                SequenceStart.Invoke();
                SequenceKeyFrame.Invoke(sequences[processIndex], frame);
            }
        }

        private void OnPovAniEnd(PovKeyFrame frame)
        {
            //Debug.Log($"OnPovAniStop KeyFrameType : {processIndex}/{sequences.Count}/{frame.IsLastKeyFrame}");
            if (frame.IsLastKeyFrame)
            {
                // Debug.Log($"OnPovAniStop IsLastKeyFrame : {processIndex}/{sequences.Count}");
                sequences[processIndex].IsSelect.Value = false;
                if (++processIndex != sequences.Count)
                {
                    StepProcess(sequences[processIndex]);
                    sequences[processIndex].IsSelect.Value = true;
                }
                else
                {
                    //all stop
                    processIndex = sequences.Count - 1;
                    SequenceStop.Invoke(sequences.Last());
                }
            }
            //processIndex
        }
    }
}
