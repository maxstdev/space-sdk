using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace MaxstXR.Extension
{
    public class SequenceProperty : ISequenceProperty
    {
        public virtual int Index { get; set; }

        public virtual string FrameName { get; set; }

        public virtual string FrameDescription { get; set; }

        public virtual string ThumbNailPath { get; set; }

        public virtual Vector3 FramePosition { get; set; }

        public virtual float RotationDuration { get; set; }

        public virtual List<Quaternion> Quaternions { get; set; }

        public virtual TransitionMode TransitionMode { get; set; }

        public virtual float Delay { get; set; }

        //status
        public virtual ReactiveProperty<bool> IsSelect { get; set; } = new ReactiveProperty<bool>(false);
    }
}
