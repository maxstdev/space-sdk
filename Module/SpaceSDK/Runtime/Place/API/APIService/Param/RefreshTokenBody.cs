using Newtonsoft.Json;
using System;

namespace MaxstXR.Place
{
    [Serializable]
    public class RefreshTokenBody
    {
        [JsonProperty("refresh_token")] public string refreshToken;
    }
}
