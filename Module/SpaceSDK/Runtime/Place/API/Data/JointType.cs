using Newtonsoft.Json;
using System;

namespace MaxstXR.Place
{
    [Serializable]
    public class JointType
    {
        [JsonProperty("id")] public long id;
        [JsonProperty("category_joint_name")] public string categoryJointName;
    }
}
