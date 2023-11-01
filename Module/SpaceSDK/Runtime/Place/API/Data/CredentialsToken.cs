using Newtonsoft.Json;
using System;

namespace MaxstXR.Place
{
	[Serializable]
	public class CredentialsToken
	{
		[JsonProperty("access_token")] public string accessToken;
		[JsonProperty("expires_in")] public long expiresIn;
		[JsonProperty("refresh_expires_in")] public long refreshExpiresIn;
		[JsonProperty("token_type")] public string tokenType;
		[JsonProperty("not-before-policy")] public int notBeforePolicy;
		[JsonProperty("scope")] public string scope;

		public string Authorization => $"{tokenType} {accessToken}";
	}
}
