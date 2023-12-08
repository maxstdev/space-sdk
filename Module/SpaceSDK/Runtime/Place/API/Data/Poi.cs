using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaxstXR.Place
{

    [Serializable]
    public class Poi : PoiPromise
    {
        [JsonProperty("id")] public long id;
        [JsonProperty("poi_uuid")] public string poiUuid;
        [JsonProperty("spot_id")] public string spotId;
        [JsonProperty("spot_name_ko")] public string spotNameKo;
        [JsonProperty("spot_name_en")] public string spotNameEn;
        [JsonProperty("poi_name_ko")] public string poiNameKo;
        [JsonProperty("poi_name_en")] public string poiNameEn;
        [JsonProperty("category_info")] public PoiCategory category;
        [JsonProperty("poi_name")] public PoiName poiname;

        [JsonProperty("view_level")] public int viewLevel;
        [JsonProperty("floor")] public int? floor;
        [JsonProperty("tags")] public List<string> tags;
        [JsonProperty("latitude")] public float? latitude;
        [JsonProperty("longitude")] public float? longitude;
        [JsonProperty("vps_x")] public float? vpsX;
        [JsonProperty("vps_y")] public float? vpsY;
        [JsonProperty("vps_z")] public float? vpsZ;
        [JsonProperty("connect_poi_uuid")] public string connectPoiUuid;
        [JsonProperty("custom")]
        //[JsonConverter(typeof(PoiExtensionConverter))] public ExtensionObject extensionObject;


        [JsonIgnore] public bool contained = false;

        [JsonIgnore] public Place refPlace;
        [JsonIgnore] public Spot refSpot;
        [JsonIgnore] public BaseCategory refCategory;


        public override string[] Keyward => tags?.ToArray() ?? new string[0];

        public override int PoiId => (int)id;

        public override int PoiRefId => (int)id;

        public override string PoiUuid => poiUuid;

        public override bool Contained { get => contained; set => contained = value; }

        //public override string Name => AppResources.IsKorean() ? poiNameKo : poiNameEn;

        //public override string Description => AppResources.IsKorean() ? detailInformation.description.ko : detailInformation.description.en;

        public override string PoiName => poiNameKo; //AppResources.IsKorean() ? poiNameKo : poiNameEn;

        public override string PoiSubType 
        {
            get 
            {
#if false
                if (PlaceUniqueName.maxst.ToString().Equals(refPlace?.placeUniqueName, 
                    StringComparison.OrdinalIgnoreCase)) 
                { 
                    return "DEVELOPER";
                }
                else
                {
                    return "RESTAURANT";
                }
#else
                return "RESTAURANT";
#endif
            }
        }

        public override string CategoryIcon 
        { 
            get
            {
                return category?.categoryIcon ?? null;
            }
        }

        public override string CategoryType 
        {
            get
            {
                return category?.categoryNameKo ?? string.Empty;// AppResources.IsKorean() ? category?.categoryNameKo : category?.categoryNameEn ;
            }
        }

        public override string Floor
        {
            get => refSpot?.Floor;
            set { if (refSpot != null) { refSpot.Floor = value; } }
        }

        public override string VpsMap => refSpot?.vpsSpotName;

        //public override string VpsMap => refSpot?.vpsSpotName == "outdoor_jongno" ? "jongro" : refSpot?.vpsSpotName;

        //public override string RepresentativeImageUrl => detailInformation?.image_url;

        //public override string LogoImageUrl => detailInformation?.image_url;

        /*
        public override List<string> DetailImages 
        {
            get 
            {
                var ret = new List<string>();
                if (detailInformation?.expansion != null) ret.Add(detailInformation?.expansion.websiteUrl);
                return ret;
            }
        }
        */

        public override string StoreName => poiNameKo;//AppResources.IsKorean() ? poiNameKo : poiNameEn;
        /*
        public override string AddressRoad => detailInformation?.address?.roadName;

        public override string WebsiteUrl => detailInformation?.expansion?.websiteUrl;

        public override string InstagramPath => detailInformation?.expansion?.websiteUrl;

        public override string Phone => detailInformation?.expansion?.phone;
        */

        public override string UniqueNameFromPlace => refPlace?.placeUniqueName ?? "";

        //public override ExtensionObject ExtensionObject => extensionObject;

        public override Vector3 GetVpsPosition()
        {
            return new Vector3(vpsX ?? 0, vpsZ ?? 0, vpsY ?? 0);
        }

        public override Vector3 GetPosition()
        {
            return new Vector3(vpsX ?? 0, vpsY ?? 0, vpsZ ?? 0);
        }

		public override Vector3 GetTruncateVpsPosition()
		{
			float truncateVpsX = (float) Math.Truncate((vpsX ?? 0));
			float truncateVpsY = (float) Math.Truncate((vpsY ?? 0));
			float truncateVpsZ = (float) Math.Truncate((vpsZ ?? 0));
			return new Vector3(truncateVpsX, truncateVpsY, truncateVpsZ);
		}


		public override void SetPosition(float x, float y, float z)
        {
            vpsX = x;
            vpsY = y;
            vpsZ = z;
        }

        public override string NavigationLocation() 
        {
            return refSpot?.vpsSpotName ?? "";
        }

        public override long CategotyId()
        {
            return refCategory?.categoryId ?? PoiCategory.CLASSIFY_LANDMARK_ID;
        }

        public override long ViewLevel()
        {
            return viewLevel;
        }
        /*
        public override string Department() 
        {
            return detailInformation?.expansion?.department ?? ""; 
        }
        */
    }

    [Serializable]
    public class PoiName
    {
        [JsonProperty("ko")] public string ko;
        [JsonProperty("en")] public string en;
    }

    [Serializable]
    public class Expansion
    {
        [JsonProperty("custom_category")] public string customCategory;
        [JsonProperty("room_number")] public string roomNumber;
        [JsonProperty("phone")] public string phone;
        [JsonProperty("website_url")] public string websiteUrl;
        [JsonProperty("menu")] public string menu;
        [JsonProperty("menu_list")] public Menu menu_list;
        [JsonProperty("department")] public string department;
        [JsonProperty("position")] public ExpansionPosition position;
        [JsonProperty("email_address")] public string emailAddress;
        [JsonProperty("business_hours")] public string businesshours;
    }

    [Serializable]
    public class DetailInformation
    {
        [JsonProperty("address")] public Address address;
        [JsonProperty("image_url")] public string image_url;
        [JsonProperty("tags")] public List<string> tags;
        [JsonProperty("description")] public DetailDescription description;
        [JsonProperty("memo")] public string memo;
        [JsonProperty("expansion")] public Expansion expansion;
    }

    public class DetailDescription
    {
        [JsonProperty("ko")]
        public string ko;
        [JsonProperty("en")]
        public string en;
    }

    public class ExpansionPosition
    {
        [JsonProperty("ko")]
        public string ko;
        [JsonProperty("en")]
        public string en;
    }
}
