using Newtonsoft.Json;
using System;

namespace Maxst.Passport
{
    [Serializable]
    public class ClientToken
    {
        [JsonProperty("access_token")]
        public string access_token;
        [JsonProperty("token_type")]
        public string token_type;
        [JsonProperty("expires_in")]
        public long expires_in;
    }
}
