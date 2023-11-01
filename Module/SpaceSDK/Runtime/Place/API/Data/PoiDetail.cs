using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MaxstXR.Place
{
    [Serializable]
    public class PoiDetail : Poi
    {
        [JsonProperty("image_url")] public string imageUrl;
        [JsonProperty("detail_images")] public List<string> detailImages;

        [JsonProperty("description_ko")] public string descriptionKo;
        [JsonProperty("description_en")] public string descriptionEn;
        [JsonProperty("menu_list_ko")] public List<string> menuListKo;
        [JsonProperty("menu_list_en")] public List<string> menuListEn;

        [JsonProperty("business_hours")] public string businessHours;
        [JsonProperty("phone")] public string phone;
        [JsonProperty("website_url")] public string websiteUrl;
        [JsonProperty("address_roadname")] public string addressRoadname;
        [JsonProperty("address_jibeon")] public string addressJibeon;
        [JsonProperty("address_detail")] public string addressDetail;

        [JsonProperty("position_ko")] public string positionKo;
        [JsonProperty("position_en")] public string positionEn;
        [JsonProperty("email_address")] public string emailAddress;


        [JsonIgnore] public string Description => descriptionKo;// AppResources.IsKorean() ? descriptionKo : descriptionEn;
        [JsonIgnore] public List<string> MenuList => menuListKo;// AppResources.IsKorean() ? menuListKo : menuListEn;
        [JsonIgnore] public string Position => positionKo;// AppResources.IsKorean() ? positionKo : positionEn;
    }
}
