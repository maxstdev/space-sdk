using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace MaxstXR.Extension
{
    public enum TransitionMode : int
    {
        WalkThrough = 0, 
        SlideShow
    }

    public interface ISequenceProperty
    {
        abstract int Index { get; }
        abstract string FrameName { get; }
        abstract string FrameDescription { get; }
        abstract string ThumbNailPath { get; }

        abstract Vector3 FramePosition { get; }
        abstract float RotationDuration { get; }
        abstract List<Quaternion> Quaternions { get; }

        abstract TransitionMode TransitionMode { get; }
        abstract float Delay { get; }

        /*status*/
        abstract ReactiveProperty<bool> IsSelect { get; }
    }
}
