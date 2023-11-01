using Newtonsoft.Json;
using System;


namespace MaxstXR.Place
{
    [Serializable]
    public class SpotDetail : Spot
    {
        [JsonProperty("is_deleted")] public bool isDeleted;
        //ex : 2022-01-12T14:34:09
        [JsonProperty("created_at")]
        [JsonConverter(typeof(DateTimeConverter))] public DateTime createdAt;
        //ex : 2022-01-19T03:44:58
        [JsonProperty("updated_at")]
        [JsonConverter(typeof(DateTimeConverter))] public DateTime updatedAt;
    }
}
