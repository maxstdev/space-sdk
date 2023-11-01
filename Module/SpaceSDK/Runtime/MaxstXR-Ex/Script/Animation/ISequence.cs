using System;
using UnityEngine;

namespace MaxstXR.Extension
{
    public interface ISequenceAni
    {
        abstract IPovAni PovInfo { get; }
        abstract string ThumbnailAddress { get; }
    }

    public interface IPovAni
    {
        abstract Vector3 StartPov { get; }
        abstract Vector3 EndPov { get; }
        abstract Quaternion StartQuaternion { get; }
        abstract Quaternion EndQuaternion { get; }
        abstract float EndDelay { get; }
        abstract bool IsRotateLeft { get; }
        abstract float MoveSpeed { get; }
        abstract float RotationSpeed { get; }
        abstract bool IsWalk { get; }
        abstract string Title { get; }
        abstract string Description { get; }
    }
}
