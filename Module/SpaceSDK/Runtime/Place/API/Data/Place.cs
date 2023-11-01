using Newtonsoft.Json;
using System;


namespace MaxstXR.Place
{
    public abstract class PlaceProperty
    {
        public abstract long PlaceId { get; }
        public abstract string PlaceUniqueName { get; }
    }

    [Serializable]
    public class Place : PlaceProperty
    {
        [JsonProperty("place_id")] public long placeId;
        [JsonProperty("place_unique_name")] public string placeUniqueName;
        [JsonProperty("place_name")] public Name placeName;
        [JsonProperty("place_image_url")] public string placeImageUrl;
        [JsonProperty("place_map_url")] public string placeMapUrl;
        [JsonProperty("central_point")]
        [JsonConverter(typeof(GeometryConverter))] public Geometry centralPoint;
        [JsonProperty("display_point")]
        [JsonConverter(typeof(GeometryConverter))] public Geometry displayPoint;
        [JsonProperty("address")] public Address address;
        [JsonProperty("floor")] public Floor floor;

        [JsonIgnore] public string PlaceName => placeName?.ko ?? string.Empty;
        [JsonIgnore] public override long PlaceId => placeId;
        [JsonIgnore] public override string PlaceUniqueName => placeUniqueName;
    }

    public class Floor
    {
        [JsonProperty("upper")] public int? upper;
        [JsonProperty("lower")] public int? lower;
    }
}
