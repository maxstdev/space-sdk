using Newtonsoft.Json;
using System;

namespace Maxst.Passport
{
    [Serializable]
    public class CredentialsToken
    {
        [JsonProperty("access_token")]
        public string access_token;
        [JsonProperty("refresh_token")]
        public string refresh_token;
        [JsonProperty("id_token")]
        public string id_token;
        
        [JsonProperty("refresh_expires_in")]
        public int refresh_expires_in;
        [JsonProperty("scope")]
        public string scope;
        [JsonProperty("expires_in")]
        public int expires_in;
        [JsonProperty("token_type")]
        public string token_type;
    }
}
