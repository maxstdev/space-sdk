using Newtonsoft.Json;
using System;

namespace MaxstXR.Place
{
    [Serializable]
    public class Name
    {
        [JsonProperty("en")] public string en;
        [JsonProperty("ko")] public string ko;
    }
}
