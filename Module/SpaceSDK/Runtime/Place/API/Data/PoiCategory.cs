using Newtonsoft.Json;
using System;

namespace MaxstXR.Place
{
    [Serializable]
    public class PoiCategory
    {
        public const long CLASSIFY_LANDMARK_ID = 11000000;
        public const long CLASSIFY_OFFICE_ID = 12000000;

        public const long UNIT = (long)1e6;

        [JsonProperty("category_id")] public long categoryId;
        [JsonProperty("category_name_ko")] public string categoryNameKo;
        [JsonProperty("category_name_en")] public string categoryNameEn;
        [JsonProperty("category_icon")] public string categoryIcon;
        [JsonProperty("description")] public string description;
        [JsonProperty("is_joint_type")] public bool? isJointType;
        [JsonProperty("map_poi_category_joint_type_id")] public string jointTypeId;
        [JsonProperty("map_poi_category_augment_type_id")] public string augmentTypeId;

        public static long ConvertClassifyId(long categoryId)
        {
            return categoryId / UNIT * UNIT;
        }
    }
}
