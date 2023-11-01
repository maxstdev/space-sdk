using Newtonsoft.Json;
using System;

namespace MaxstXR.Place
{
    [Serializable]
    public class AugmentType
    {
        [JsonProperty("id")] public long id;
        [JsonProperty("category_augment_name")] public string categoryAugmentName;
    }
}
