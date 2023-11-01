using Newtonsoft.Json;
using System;


namespace MaxstXR.Place
{
    [Serializable]
    public class PlaceDetail : Place
    {
        [JsonProperty("is_deleted")] public bool isDeleted;
        [JsonProperty("created_at")]
        [JsonConverter(typeof(DateTimeConverter))] public DateTime createdAt;
        [JsonProperty("updated_at")]
        [JsonConverter(typeof(DateTimeConverter))] public DateTime updatedAt;
    }
}
