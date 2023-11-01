using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace MaxstXR.Extension
{
    public class PovAnimation : MonoBehaviour, ITransitionDelegate
    {
        [SerializeField] protected List<PovKeyFrame> frames = new List<PovKeyFrame>();

        [field: SerializeField] public UnityEvent PovStart { get; private set; } = new UnityEvent();
        [field: SerializeField] public UnityEvent<PovKeyFrame> PovResume { get; private set; } = new UnityEvent<PovKeyFrame>();
        [field: SerializeField] public UnityEvent<PovKeyFrame> PovPause { get; private set; } = new UnityEvent<PovKeyFrame>();
        [field: SerializeField] public UnityEvent<PovKeyFrame> PovStop { get; private set; } = new UnityEvent<PovKeyFrame>();
        [field: SerializeField] public UnityEvent<PovKeyFrame> PovAniStart { get; private set; } = new UnityEvent<PovKeyFrame>();
        [field: SerializeField] public UnityEvent<PovKeyFrame> PovAniEnd { get; private set; } = new UnityEvent<PovKeyFrame>();
        [field: SerializeField] public UnityEvent Finalize { get; private set; } = new UnityEvent();

        [SerializeField] protected SmoothCameraManager smoothCameraManager;

        protected PovKeyFrame lastProcessKeyFrame;

        protected virtual void Awake()
        {
            smoothCameraManager = GetComponent<SmoothCameraManager>();
        }

        private void OnDestroy()
        {
            Finalize?.Invoke();
        }

        public async void Generate(List<PathModel> paths, UnityAction complete)
        {
            await UniTask.WaitUntil(() => smoothCameraManager);
            GenerateFrame(paths);
            complete?.Invoke();
        }

        public void Procedure(PovKeyFrame frame = null)
        {
            StartCoroutine(FramesProcess(frame));
        }

        protected virtual void GenerateFrame(List<PathModel> paths)
        {
            frames.Clear();
            KeyFrameGenerator.ConvertPathModel(paths, smoothCameraManager, frames);
        }

        private IEnumerator FramesProcess(PovKeyFrame lastFrame = null)
        {
            //Debug.Log($"FramesProcess : {frames.Count}");
            int startIndex = 0;
            if (lastFrame == null || lastFrame.IsLastKeyFrame)
            {
                PovStart?.Invoke();
            }
            else
            {
                startIndex = lastFrame.Index + 1;
                PovResume.Invoke(frames[startIndex]); 
            }

            for (var i = startIndex; i < frames.Count; ++i)
            {
                smoothCameraManager.HandlePovKeyFrame(frames[i], this);
                yield return new WaitUntil(() => smoothCameraManager.IsAddableTransitions());
            }
            yield break;
        }

        void ITransitionDelegate.DownloadStart()
        {
            
        }

        void ITransitionDelegate.DownloadProgess(float f)
        {
            
        }

        void ITransitionDelegate.DownloadComplete()
        {
            
        }

        void ITransitionDelegate.DownloadException(Exception e)
        {
            
        }

        void ITransitionDelegate.DownloadException(UnityWebRequest www)
        {
            
        }

        void ITransitionDelegate.AnimationStarted(PovController current, object sourceObject)
        {
            if (sourceObject is PovKeyFrame frame)
            {
                PovAniStart.Invoke(frame);
            }
        }

        void ITransitionDelegate.AnimationFinished(PovController next, object sourceObject) 
        {
            if (sourceObject is PovKeyFrame frame)
            {
                //Debug.Log($"AnimationFinished {frame.Index}/{frame.KeyFrameType}/{frame.IsLastKeyFrame}");
                PovAniEnd.Invoke(frame);
                lastProcessKeyFrame = frame;
                if (frame.IsLastKeyFrame)
                {
                    PovStop?.Invoke(frame);
                }
            }
            else
            {

            }
        }
    }
}
