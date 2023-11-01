using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MaxstXR.Place
{
    [Serializable]
    public class Spot
    {
        [JsonProperty("id")] public long id;
        [JsonProperty("spot_name")] public Name spotName;
        [JsonProperty("vps_spot_name")] public string vpsSpotName;
        [JsonProperty("vps_x")] public float? vpsX;
        [JsonProperty("vps_y")] public float? vpsY;
        [JsonProperty("vps_z")] public float? vpsZ;
        [JsonProperty("floor")] public int? floor;
        [JsonProperty("floor_name")] public Name floorName;
        [JsonProperty("is_outdoor")] public bool isOutdoor;
        [JsonProperty("can_support_gps")] public bool canSupportGps;
        [JsonProperty("is_multi_section")] public bool isMultiSection;
        [JsonProperty("spot_origin")] public SpotOrigin spotOrigin;
		[JsonProperty("ktx2_texture_file_path")] public string ktx2TextureFilePath;
		[JsonProperty("spot_directory")] public string spotDirectory;
        [JsonProperty("raw_ply_path")] public string rawPlyPath;
        [JsonProperty("aligned_ply_path")] public string alignedPlyPath;

        [JsonIgnore] public string SpotName => spotName?.ko ?? string.Empty;

        [JsonIgnore] public string Floor
        {
            get => floorName?.ko ?? string.Empty; //AppResources.IsKorean() ? floorName?.ko : floorName?.en;
            set { if (floorName != null) { floorName.ko = value; floorName.en = value; } }
        }
    }

    [Serializable]
    public class SpotOrigin
    {
        [JsonProperty("UTM-K")] List<float> utmK;
        [JsonProperty("GPS")] public List<float> gps;
        [JsonProperty("Section")] List<string> section;
    }
}
