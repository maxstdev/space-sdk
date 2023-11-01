using Newtonsoft.Json;
using System;

namespace MaxstXR.Place
{

    [Serializable]
    public class Address
    {
        [JsonProperty("road_name")] public string roadName;
        [JsonProperty("jibeon")] public string jibeon;
        [JsonProperty("detail")] public string detail;
    }
}
