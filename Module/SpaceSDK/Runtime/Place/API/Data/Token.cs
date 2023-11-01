using Newtonsoft.Json;
using System;

namespace MaxstXR.Place
{
    [Serializable]
    public class Token
    {
        [JsonProperty("access_token")] public string accessToken; 
        [JsonProperty("refresh_token")] public string refreshToken;
    }
}
