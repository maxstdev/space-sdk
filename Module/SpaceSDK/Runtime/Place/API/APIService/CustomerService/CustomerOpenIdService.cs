using Retrofit;
using Retrofit.HttpImpl;
using Retrofit.Parameters;
using System;
using System.Reflection;
using UnityEngine;

namespace MaxstXR.Place
{
    [Obsolete("Not supported due to use of passport", true)]
    public class CustomerOpenIdService : RestAdapter, ICustomerOpenIdApi
    {
		private static CustomerOpenIdService _instance;
        public static CustomerOpenIdService Instance
        {
            get
            {
                if (_instance == null)
                {
                    var authService = new GameObject(typeof(CustomerOpenIdService).FullName);
                    _instance = authService.AddComponent<CustomerOpenIdService>();
                    DontDestroyOnLoad(authService);
                }
                return _instance;
            }
        }

        protected override void SetRestAPI()
        {
            baseUrl = GetUrl();
            iRestInterface = typeof(ICustomerOpenIdApi);
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
				? "https://api.maxst.com/auth"
				: "https://api.maxst.com/auth";

			//https://alpha-api.maxst.com/auth
		}

        /*public IObservable<CredentialsToken> AuthoringToken()
        {
            return Token(ApiConst.GRANT_TYPE, ApiConst.AUTHORING_CLIENT_ID, ApiConst.AUTHORING_CLIENT_SECRET);
        }*/

        public IObservable<CredentialsToken> Token( 
			[Field("grant_type")] string grantType, 
			[Field("client_id")] string clientId, 
			[Field("client_secret")] string clientSecret)
		{
			return SendRequest<CredentialsToken>(MethodBase.GetCurrentMethod(), 
				grantType, clientId, clientSecret) as IObservable<CredentialsToken>;
		}
	}
}
