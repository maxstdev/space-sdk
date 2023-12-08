using System;
using UnityEngine.Networking;

namespace MaxstXR.Extension
{
    public enum TransitionStatus
    {
        Idle = 0,
        DownloadStarted,
        DownloadEnded,
        DownloadError,
        AnimateStarted,
        AnimateEnded,
    }

    public interface ITransitionDelegate
    {
        void DownloadStart(PovKeyFrame keyFrame);
        void DownloadProgess(PovKeyFrame keyFrame, float f);
        void DownloadComplete(PovKeyFrame keyFrame);
        void DownloadException(PovKeyFrame keyFrame, Exception e);
        void DownloadException(PovKeyFrame keyFrame, UnityWebRequest www);

        //animation
        void AnimationStarted(PovKeyFrame keyFrame) { }
        void AnimationFinished(PovKeyFrame keyFrame) { }
    }
}
