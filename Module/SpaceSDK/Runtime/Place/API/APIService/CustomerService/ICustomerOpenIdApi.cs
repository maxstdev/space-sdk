using Retrofit.Methods;
using Retrofit.Parameters;
using System;

namespace MaxstXR.Place
{
	public interface ICustomerOpenIdApi
	{
        [Post("/realms/maxst/protocol/openid-connect/token")]
        IObservable<CredentialsToken> Token(
			[Field("grant_type")] string grantType,
			[Field("client_id")] string clientId,
			[Field("client_secret")] string clientSecret);
	}
}

