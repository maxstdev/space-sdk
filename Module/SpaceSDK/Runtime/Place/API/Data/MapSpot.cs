using Newtonsoft.Json;

namespace MaxstXR.Place
{
    public class MapSpot
    {
        [JsonProperty("spaceId")]
        public string spaceId;

        [JsonProperty("placeId")] 
        public string placeId;
        
        [JsonProperty("spotId")] 
        public string spotId;
        
        [JsonProperty("name")] 
        public string name;

        [JsonProperty("resource")] 
        public MapResource resource;
    }

    public class MapResource {
        [JsonProperty("StandaloneWindows64")]
        public string StandaloneWindows64;

        [JsonProperty("WebGL")]
        public string WebGL;

        public string GetResourcePath()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return WebGL;
#else
            return StandaloneWindows64;
#endif
        }
    }
}
