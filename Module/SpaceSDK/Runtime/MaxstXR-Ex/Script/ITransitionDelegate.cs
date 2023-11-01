using System;
using UnityEngine.Networking;

namespace MaxstXR.Extension
{
    public interface ITransitionDelegate
    {
        void DownloadStart();
        void DownloadProgess(float f);
        void DownloadComplete();
        void DownloadException(Exception e);
        void DownloadException(UnityWebRequest www);

        //animation
        void AnimationStarted(PovController current, object sourceObject) { }
        void AnimationFinished(PovController next, object sourceObject) { }
    }
}
