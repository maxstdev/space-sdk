using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.Utilities;
using Maxst.Settings;
using System;
using System.Collections;
using System.Linq;
using UniRx;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEngine;

namespace Maxst.Passport
{
#if UNITY_EDITOR || UNITY_STANDALONE
    public partial class OpenIDConnectAdapter : MaxstIOpenIDConnectProvider
    {

        void MaxstIOpenIDConnectProvider.OnAuthorazationCode(string code)
        {
            Debug.Log($"OnAuthorazationCode : {code}");
            AcceceToken(code);
        }

        string MaxstIOpenIDConnectProvider.GetLoginPageURL(string redirectUri)
        {
            switch (clientType)
            {
                case ClientType.Public:
                    var PKCEManagerInstance = PKCEManager.GetInstance();
                    var CodeVerifier = PKCEManagerInstance.GetCodeVerifier();
                    var CodeChallenge = PKCEManagerInstance.GetCodeChallenge(CodeVerifier);
                    return GetURL(redirectUri, CodeVerifier, CodeChallenge);

                case ClientType.Confidential:
                    return GetConfidentialLoginURL(redirectUri);

                default:
                    Debug.Log("GetLoginPageURL error!");
                    return null;
            }
        }
    }
#endif

    public partial class OpenIDConnectAdapter
    {
        private ClientType clientType;

        public OpenIDConnectArguments OpenIDConnectArguments;
        public IOpenIDConnectListener IOpenIDConnectListener = null;

        private string CodeVerifier;
        
        private static OpenIDConnectAdapter instance;


#if UNITY_EDITOR || UNITY_STANDALONE
        private MaxstOpenIDConnectService OpenIDConnectService;

        private string locationUrl = null;

        public void SetLocationUrl(string url) {
            locationUrl = url;
        }
#endif
        private OpenIDConnectAdapter() { }

        public static OpenIDConnectAdapter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OpenIDConnectAdapter();

                }
                return instance;
            }
        }

        public void SetLoginListener(IOpenIDConnectListener listener)
        {
            IOpenIDConnectListener = listener;
        }

        public void InitOpenIDConnectAdapter(OpenIDConnectArguments openidArguments, PassportConfig passportConfig = null)
        {
            clientType = passportConfig == null ? ClientType.Public : passportConfig.clientType;
            OpenIDConnectArguments = openidArguments;

            TokenRepo.Instance.ClientID = openidArguments.TryGetValue(OpenIDConnectArgument.ClientID, out var clientID) ? clientID : string.Empty;
            TokenRepo.Instance.ClientSecret = openidArguments.TryGetValue(OpenIDConnectArgument.ClientSecret, out var ClientSecret) ? ClientSecret : string.Empty;
            TokenRepo.Instance.ClientType = clientType;
            TokenRepo.Instance.passportConfig = passportConfig;
            
            SetWindowLoginServiceManger(this);
        }

        public void SetWindowLoginServiceManger(object provider)
        {
            MaxstIOpenIDConnectProvider IOpenIDConnectProvider = provider as MaxstIOpenIDConnectProvider;
#if UNITY_EDITOR || UNITY_STANDALONE
            if (OpenIDConnectService != null)
            {
                ServiceManager.Instance.InstRemoveService<MaxstOpenIDConnectService>();
                OpenIDConnectService = null;
            }

            ServiceManager.Instance.CreateRunner();

            OpenIDConnectService = new MaxstOpenIDConnectService(IOpenIDConnectProvider)
            {
                OidcProvider = new MaxstOpenIDConnectProvider(IOpenIDConnectProvider)
            };
            ServiceManager.Instance.InstRegisterService(OpenIDConnectService);
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SetDeeplink()
        {
            Application.deepLinkActivated += Instance.OnSuccessAuthorization;
        }

        public void ShowPassportLoginPage()
        {
            OnConfidentialLogin();
        }

        public void ShowOIDCProtocolLoginPage()
        {
            OnConfidentialLogin();
        }

        private void OnConfidentialLogin()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.AndroidRedirectUri, out var RedirectURI);
            Application.OpenURL(GetConfidentialLoginURL(RedirectURI));
#elif UNITY_IOS && !UNITY_EDITOR
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.iOSRedirectUri, out var RedirectURI);
            Application.OpenURL(GetConfidentialLoginURL(RedirectURI));
#elif UNITY_EDITOR || UNITY_STANDALONE
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.WebRedirectUri, out var RedirectURI);
            OpenLoginPageAsync();
#else 
            Debug.Log("This platform is not supported OnConfidentialLogin");
#endif     
        }

        public void ShowOIDCProtocolLoginPage(string CodeVerifier, string CodeChallenge)
        {
            OnPublicLogin(CodeVerifier, CodeChallenge);
        }

        private void OnPublicLogin(string CodeVerifier, string CodeChallenge)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.AndroidRedirectUri, out var RedirectURI);
            Application.OpenURL(GetURL(RedirectURI, CodeVerifier, CodeChallenge));
#elif UNITY_IOS && !UNITY_EDITOR
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.iOSRedirectUri, out var RedirectURI);
            Application.OpenURL(GetURL(RedirectURI, CodeVerifier, CodeChallenge));
#elif UNITY_EDITOR || UNITY_STANDALONE
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.WebRedirectUri, out var RedirectURI);
            OpenLoginPageAsync();
#else 
            Debug.Log("This platform is not supported OnPublicLogin");
#endif
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        private async void OpenLoginPageAsync() {
            var location = locationUrl == null ? EnvAdmin.Instance.AuthUrlSetting.Urls[URLType.Location] : locationUrl;
            await ServiceManager.Instance.InstGetService<MaxstOpenIDConnectService>().OpenLoginPageAsync(location);
        }
#endif
        public void OpenUrlLoginPage(PassportConfig config)
        {
            switch (config.clientType)
            {
                case ClientType.Public:
                    var PKCEManagerInstance = PKCEManager.GetInstance();
                    var CodeVerifier = PKCEManagerInstance.GetCodeVerifier();
                    var CodeChallenge = PKCEManagerInstance.GetCodeChallenge(CodeVerifier);
                    
                    ShowOIDCProtocolLoginPage(CodeVerifier, CodeChallenge);
                    
                    break;
                case ClientType.Confidential:
                    ShowOIDCProtocolLoginPage();
                    
                    break;
                default:
                    Debug.Log("OpenUrlLoginPage Client type is not set!");
                    break;
            }
        }

        public string GetConfidentialLoginURL(string RedirectURI)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            OpenIDConnectArguments.SetValue(OpenIDConnectArgument.WebRedirectUri, RedirectURI);
# endif
            var Setting = EnvAdmin.Instance.OpenIDConnectSetting;
            Setting.Urls.TryGetValue(OpenIDConnectSettingKey.ConfidentialLoginUrl, out var LoginUrl);

            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.ClientID, out var ClientID);
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.ClientSecret, out var ClientSecret);

            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.ResponseType, out var ResponseType);
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.Scope, out var Scope);

            Setting.Urls.TryGetValue(OpenIDConnectSettingKey.LoginAPI, out var LoginAPI);

            var Host = EnvAdmin.Instance.AuthUrlSetting.Urls[URLType.API];

            var URL = string.Format(LoginUrl, Host, LoginAPI, ClientID, ResponseType, Scope, RedirectURI, ClientSecret);

            Debug.Log($"[OpenIDConnectAdapter] GetConfidentialLoginURL : {URL}");

            return URL;
        }

        public string GetURL(string RedirectURI, string CodeVerifier, string CodeChallenge)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            OpenIDConnectArguments.SetValue(OpenIDConnectArgument.WebRedirectUri, RedirectURI);
# endif
            var Setting = EnvAdmin.Instance.OpenIDConnectSetting;
            Setting.Urls.TryGetValue(OpenIDConnectSettingKey.PublicLoginUrl, out var LoginUrl);

            Setting.Urls.TryGetValue(OpenIDConnectSettingKey.LoginAPI, out var LoginAPI);
            
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.ClientID, out var ClientID);
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.ResponseType, out var ResponseType);
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.Scope, out var Scope);

            this.CodeVerifier = CodeVerifier;

            Setting.Urls.TryGetValue(OpenIDConnectSettingKey.CodeChallengeMethod, out var CodeChallengeMethod);

            var Host = EnvAdmin.Instance.AuthUrlSetting.Urls[URLType.API];

            var URL = string.Format(LoginUrl, Host, LoginAPI, ClientID, ResponseType, Scope, RedirectURI, CodeChallenge, CodeChallengeMethod);

            Debug.Log($"[OpenIDConnectAdapter] GetURL : {URL}");

            return URL;
        }

        public void OnClientToken(PassportConfig config, Action<TokenStatus, ClientToken> success = null,
            Action<ErrorCode, Exception> fail = null)
        {
            MainThreadDispatcher.StartCoroutine(
                    TokenRepo.Instance.GetPassportClientToken(
                config.ApplicationId,
                config.ApplicationKey,
                config.GrantType,
                success,
                fail
            ));
        }

        public void OnRefreshToken(Action completeAction = null)
        {
            Debug.Log("[OpenIDConnectAdapter] OnRefresh");
            RefreshToken(completeAction, OpenIDConnectArguments, clientType);
        }

        public void OnLogout(Action completeAction = null)
        {
            Debug.Log("[OpenIDConnectAdapter] OnLogout");
            SessionLogout(completeAction);
        }

        public void ShowSAMLProtocolLoginPage()
        {
            Debug.Log("[OpenIDConnectAdapter] : SAML Protocol is not supported.");
        }
        private void OnSuccessAuthorization(string url)
        {
            string Query = url.Split("?"[0])[1];

            var AuthorizationDictionary = Query.Replace("?", "").Split('&').ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1]);
            var code = AuthorizationDictionary["code"];

            foreach (var each in AuthorizationDictionary)
            {
                Debug.Log($"[OpenIDConnectAdapter] OnSuccessAuthorization Key: {each.Key}, Value: {each.Value}");
            }

            AcceceToken(code);
        }

        private Action<TokenStatus, Token> GetTokenSuccessCallback()
        {
            return (status, token) =>
            {
                if (status != TokenStatus.Validate)
                {
                    Debug.LogWarning("[OpenIDConnectAdapter] auth fail.. need retry login");
                    TokenRepo.Instance.Config(null);
                    return;
                }
                Debug.Log($"[OpenIDConnectAdapter] token.idToken : {token.idToken}");
                Debug.Log($"[OpenIDConnectAdapter] token.accessToken : {token.accessToken}");
                Debug.Log($"[OpenIDConnectAdapter] token.refreshToken : {token.refreshToken}");
                IOpenIDConnectListener?.OnSuccess(token, RequestType.ACCECE_TOKEN);
            };
        }

        private Action<ErrorCode, Exception> GetTokenFaliCallback()
        {
            return (LoginErrorCode, Exception) =>
            {

                IOpenIDConnectListener?.OnFail(LoginErrorCode, Exception);

            };
        }

        public void AcceceToken(string code)
        {
            IEnumerator ie;
            if (clientType == ClientType.Public)
            {
                ie = TokenRepo.Instance.GetPassportToken(
                    OpenIDConnectArguments, code, CodeVerifier, GetTokenSuccessCallback(), GetTokenFaliCallback()
                );
            }
            else
            {
                ie = TokenRepo.Instance.GetConfidentialPassportToken(
                        OpenIDConnectArguments, code, GetTokenSuccessCallback(), GetTokenFaliCallback()
                );
            }

            MainThreadDispatcher.StartCoroutine(ie);
        }
#if UNITY_EDITOR
        public void EditorAccessToken(string code)
        {
            EditorCoroutineUtility.StartCoroutine(
                clientType == ClientType.Public ?
                TokenRepo.Instance.GetPassportToken(
                        OpenIDConnectArguments, code, CodeVerifier, GetTokenSuccessCallback(), GetTokenFaliCallback()
                    )
                : TokenRepo.Instance.GetConfidentialPassportToken(
                        OpenIDConnectArguments, code, GetTokenSuccessCallback(), GetTokenFaliCallback()
                    )
                , this
                );
        }
#endif
        private void RefreshToken(Action complete, OpenIDConnectArguments OpenIDConnectArguments , ClientType clientType)
        {
            MainThreadDispatcher.StartCoroutine(
                    TokenRepo.Instance.GetPassportRefreshToken(
                        (status, token) =>
                        {
                            if (status != TokenStatus.Validate)
                            {
                                Debug.LogWarning("[OpenIDConnectAdapter] auth fail.. need retry login");
                                TokenRepo.Instance.Config(null);
                                complete?.Invoke();
                                return;
                            }

                            if (token == null)
                            {
                                Debug.Log($"[OpenIDConnectAdapter] Token value does not exist");
                            }
                            else
                            {
                                Debug.Log($"[OpenIDConnectAdapter] OnSuccess RefreshToken idToken : {token.idToken}");
                                Debug.Log($"[OpenIDConnectAdapter] OnSuccess RefreshToken accessToken : {token.accessToken}");
                                Debug.Log($"[OpenIDConnectAdapter] OnSuccess RefreshToken refreshToken : {token.refreshToken}");

                                IOpenIDConnectListener?.OnSuccess(token, RequestType.REFRESH_TOKEN);
                            }
                            complete?.Invoke();
                        },
                        (Exception) =>
                        {
                            IOpenIDConnectListener?.OnFail(ErrorCode.REFRESH, Exception);
                            complete?.Invoke();
                        }
                    )
                );
        }

        //private Action success = null, System.Action<System.Exception> fail = null
        private Action GetLogoutSuccessCallback(Action complete)
        {
            return () =>
            {
                Debug.Log($"[OpenIDConnectAdapter] SessionLogout success");
                complete?.Invoke();
                IOpenIDConnectListener?.OnLogout();
            };
        }

        private Action<Exception> GetLogoutExceptionCallback(Action complete)
        {
            return (Exception) =>
            {
                Debug.Log($"[OpenIDConnectAdapter] SessionLogout fail : {Exception}");
                complete?.Invoke();
                IOpenIDConnectListener?.OnFail(ErrorCode.LOGOUT, Exception);
            };
        }

        private void SessionLogout(Action complete)
        {
            MainThreadDispatcher.StartCoroutine(
                TokenRepo.Instance.GetPassportRefreshToken(
                    (status, token) =>
                    {
                        if (status != TokenStatus.Validate)
                        {
                            Debug.LogWarning("[OpenIDConnectAdapter] auth fail.. need retry login");
                            TokenRepo.Instance.Config(null);
                            complete?.Invoke();
                            return;
                        }

                        TokenRepo.Instance.PassportLogout(
                            GetLogoutSuccessCallback(complete),
                            GetLogoutExceptionCallback(complete)
                        );
                    },
                    (Exception) =>
                    {
                        IOpenIDConnectListener?.OnFail(ErrorCode.LOGOUT_REFRESH, Exception);
                        Debug.Log($"[OpenIDConnectAdapter] Exception : {Exception}");
                        complete?.Invoke();
                    }
                )
            );
        }
    }
}
