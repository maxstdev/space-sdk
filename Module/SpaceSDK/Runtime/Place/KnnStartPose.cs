using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MaxstXR.Place
{
    [Serializable]
    public class KnnStartPose
    {
        [field: SerializeField] public Vector3 Position { get; set; } = Vector3.zero;
        [field: SerializeField] public Quaternion Rotation { get; set; } = Quaternion.identity;
    }
}
