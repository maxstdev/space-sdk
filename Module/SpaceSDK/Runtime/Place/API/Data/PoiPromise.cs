using MaxstUtils;
using Newtonsoft.Json;
using UnityEngine;

namespace MaxstXR.Place
{
    public abstract class PoiPromise
    {
        [JsonIgnore] public readonly Event<bool> OnDestination = new (false);
        [JsonIgnore] public abstract string[] Keyward { get; }
        [JsonIgnore] public abstract int PoiId { get; }
        [JsonIgnore] public abstract int PoiRefId { get; }
        [JsonIgnore] public abstract string PoiUuid { get; }
        [JsonIgnore] public abstract bool Contained { get; set; }
        [JsonIgnore] public abstract string PoiName { get; }
        [JsonIgnore] public abstract string PoiSubType { get; }
        [JsonIgnore] public virtual string CategoryIcon { get { return null; } }
        [JsonIgnore] public virtual string CategoryType { get { return null; } }
        [JsonIgnore] public abstract string Floor { get; set; }
        [JsonIgnore] public abstract string VpsMap { get; }
        [JsonIgnore] public abstract string StoreName { get; }
        [JsonIgnore] public abstract string UniqueNameFromPlace { get; }
        //[JsonIgnore] public abstract ExtensionObject ExtensionObject { get; }

        public abstract Vector3 GetPosition();
        public abstract Vector3 GetVpsPosition();
		public abstract Vector3 GetTruncateVpsPosition();
		public abstract void SetPosition(float x, float y, float z);
        public abstract string NavigationLocation();
        public abstract long CategotyId();
        public abstract long ViewLevel();
    }
}
