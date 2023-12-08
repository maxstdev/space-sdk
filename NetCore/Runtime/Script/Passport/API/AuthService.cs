using Retrofit;
using Retrofit.HttpImpl;
using Retrofit.Parameters;
using System;
using System.Reflection;
using UnityEngine;
using Maxst.Settings;

namespace Maxst.Passport
{

    public class AuthService : RestAdapter, IAuthApi
    
    {
        private static AuthService _instance;
        public static AuthService Instance
        {
            get
            {
                if (_instance == null) 
                {
                    var authService = new GameObject(typeof(AuthService).FullName);
                    _instance = authService.AddComponent<AuthService>();
                    if (Application.isPlaying) 
                        DontDestroyOnLoad(authService);
                }
                return _instance;
            }
        }

        protected override void SetRestAPI()
        {
            baseUrl = GetUrl();
            iRestInterface = typeof(IAuthApi);
        }

        protected override RequestInterceptor SetIntercepter()
        {
            return null;
        }

        protected override HttpImplement SetHttpImpl()
        {
            var httpImpl = new UnityWebRequestImpl();
            httpImpl.EnableDebug = true;
            return httpImpl;
        }

        private string GetUrl()
        {
            //var prefix = EnvAdmin.Instance.CurrentEnv.Value == EnvType.Alpha ? "/api" : "";
            return $"{EnvAdmin.Instance.AuthUrlSetting.Urls[URLType.API]}";
        }

        public IObservable<CredentialsToken> ConfidentialPassportToken(
            [Field("client_id")] string client_id, 
            [Field("client_secret")] string client_secret, 
            [Field("grant_type")] string grant_type, 
            [Field("redirect_uri")] string redirect_uri, 
            [Field("code")] string code
            )
        {
            return SendRequest<CredentialsToken>(MethodBase.GetCurrentMethod(),
                client_id, client_secret, grant_type, redirect_uri, code) as IObservable<CredentialsToken>;
        }

        public IObservable<CredentialsToken> PassportToken(
            [Field("client_id")] string client_id, 
            [Field("code_verifier")] string code_verifier, 
            [Field("grant_type")] string grant_type, 
            [Field("redirect_uri")] string redirect_uri, 
            [Field("code")] string code
            )
        {
            return SendRequest<CredentialsToken>(MethodBase.GetCurrentMethod(),
                client_id, code_verifier, grant_type, redirect_uri, code) as IObservable<CredentialsToken>;
        }

        public IObservable<CredentialsToken> PassportRefreshToken(
            [Field("client_id")] string client_id,
            [Field("grant_type")] string grant_type,
            [Field("refresh_token")] string refresh_token
            )
        {
            return SendRequest<CredentialsToken>(MethodBase.GetCurrentMethod(),
                client_id, grant_type, refresh_token) as IObservable<CredentialsToken>;
        }

        public IObservable<CredentialsToken> PassportConfidentialRefreshToken(
            [Field("client_id")] string client_id, 
            [Field("grant_type")] string grant_type, 
            [Field("refresh_token")] string refresh_token, 
            [Field("client_secret")] string client_secret
        )
        {
            return SendRequest<CredentialsToken>(MethodBase.GetCurrentMethod(),
                client_id, grant_type, refresh_token, client_secret) as IObservable<CredentialsToken>;
        }

        public IObservable<string> PassportLogout(
            [Retrofit.Parameters.Header("Authorization")] string accessToken,
            [Field("client_id")] string client_id, 
            [Field("id_token_hint")] string id_token_hint
            )
        {
            return SendRequest<string>(MethodBase.GetCurrentMethod(),
            accessToken, client_id, id_token_hint) as IObservable<string>;
        }

        public IObservable<ClientToken> PassportClientToken([Field("client_id")] string applicationId, [Field("client_secret")] string applicationKey, [Field("grant_type")] string grantType)
        {
            return SendRequest<ClientToken>(MethodBase.GetCurrentMethod(),
            applicationId, applicationKey, grantType) as IObservable<ClientToken>;
        }
    }
}
