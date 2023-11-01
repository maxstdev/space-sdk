using Retrofit.Methods;
using Retrofit.Parameters;
using System;
using System.Collections.Generic;

namespace MaxstXR.Place
{
    public interface ICustomerAuthApi
    {
        [Get("/auth/logout/{login_type}")]
        IObservable<List<string>> ReqLogout(
            [Path("login_type")] string loginType,
            [Body] RefreshTokenBody body);

        [Get("/auth/reissue")]
        IObservable<Token> ReqRefreshToken(
            [Body] RefreshTokenBody body);

		[Post("/sdk/token")]
		IObservable<AccessTokenData> SdkToken(
			[Header("Authorization")] string jwtToken);

		[Get("/sdk/time-sync")]
		IObservable<string> TimeSync(
			[Query("timestamp")] int unixTimestamp);
	}
}

