using Newtonsoft.Json;
using System;

namespace MaxstXR.Place
{
    [Serializable]
    public class Custom
    {
        [JsonProperty("type")] public string type;
        [JsonProperty("url")] public string url;
        [JsonProperty("local_pose")] public LocalPose localPose;
    }

    [Serializable]
    public class LocalPose
    {
        [JsonProperty("rotation")] public Rotation rotation;
        [JsonProperty("scale")] public Scale scale;
        [JsonProperty("collider_scale")] public ColliderScale collider;
    }

    [Serializable]
    public class ColliderScale
    {
        [JsonProperty("center")] public Center center;
        [JsonProperty("size")] public Size size;
    }

    [Serializable]
    public class Size
    {
        [JsonProperty("size_x")] public float sizeX;
        [JsonProperty("size_y")] public float sizeY;
        [JsonProperty("size_z")] public float sizeZ;
    }

    [Serializable]
    public class Center
    {
        [JsonProperty("center_x")] public float centerX;
        [JsonProperty("center_y")] public float centerY;
        [JsonProperty("center_z")] public float centerZ;
    }

    [Serializable]
    public class Scale
    {
        [JsonProperty("scale_x")] public float scaleX;
        [JsonProperty("scale_y")] public float scaleY;
        [JsonProperty("scale_z")] public float scaleZ;
    }

    [Serializable]
    public class Rotation
    {
        [JsonProperty("rotation_x")] public float rotationX;
        [JsonProperty("rotation_y")] public float rotationY;
        [JsonProperty("rotation_z")] public float rotationZ;
    }
}