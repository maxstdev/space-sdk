using maxstAR;
using Retrofit;
using Retrofit.HttpImpl;
using Retrofit.Parameters;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MaxstXR.Place
{
    [Obsolete("Not supported due to use of passport", true)]
    public class CustomerAuthService : RestAdapter, ICustomerAuthApi
    {
        private static CustomerAuthService _instance;
        public static CustomerAuthService Instance
        {
            get
            {
                if (_instance == null)
                {
                    var authService = new GameObject("CustomerAuthService");
                    _instance = authService.AddComponent<CustomerAuthService>();
                    DontDestroyOnLoad(authService);
                }
                return _instance;
            }
        }

        protected override void SetRestAPI()
        {
            baseUrl = GetUrl();
            iRestInterface = typeof(ICustomerAuthApi);
        }

        protected override RequestInterceptor SetIntercepter()
        {
            return null;
        }

        protected override HttpImplement SetHttpImpl()
        {
			var httpImpl = new UnityWebRequestImpl
			{
				EnableDebug = true
			};
			return httpImpl;
        }

        private string GetUrl()
        {
            return Debug.isDebugBuild
				? "https://beta-api-auth-developer.maxverse.io"
				: "https://beta-api-auth-developer.maxverse.io";
		}

        public IObservable<List<string>> ReqLogout([Path("login_type")] string loginType, [Body] RefreshTokenBody body)
        {
            return SendRequest<List<string>>(MethodBase.GetCurrentMethod(), loginType, body) as IObservable<List<string>>;
        }

        public IObservable<Token> ReqRefreshToken([Body] RefreshTokenBody body)
        {
            return SendRequest<Token>(MethodBase.GetCurrentMethod(), body) as IObservable<Token>;
        }

		public IObservable<AccessTokenData> SdkToken(
			[Retrofit.Parameters.Header("Authorization")] string jwtToken)
		{
			return SendRequest<AccessTokenData>(MethodBase.GetCurrentMethod(), 
				jwtToken) as IObservable<AccessTokenData>;
		}

		public IObservable<string> TimeSync(int unixTimestamp)
		{
			return SendRequest<string>(MethodBase.GetCurrentMethod(),
				unixTimestamp) as IObservable<string>;
		}
	}
}
