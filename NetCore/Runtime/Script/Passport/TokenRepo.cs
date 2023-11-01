using Maxst.Settings;
using Maxst.Token;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using static Maxst.Token.JwtTokenParser;

namespace Maxst.Passport
{
    public class Token
    {
        public string idToken;
        public string accessToken;
        public string refreshToken;
        public int refreshExpiresIn;

        public TokenDictionary accessTokenDictionary;
        public TokenDictionary idTokenDictionary;
    }

    public enum TokenStatus
    {
        Validate,
        Expired,
        Renewing,
    }

    public class TokenRepo : MaxstUtils.Singleton<TokenRepo>
    {
        private const long DEFAULT_EFFECTIVE_TIME = 300;
        private const long ESTIMATED_EXPIRATION_TIME = 30;

        private const string ClientAccessTokenKey = "Passport_ClientAccessToken";

        private const string IdTokenKey = "Passport_IdToken";
        private const string AccessTokenKey = "Passport_AccessToken";
        private const string RefreshTokenKey = "Passport_RefreshToken";
        private const string RefreshExpiresIn = "Passport_RefreshExpiresIn";

        private const string GrantType = "refresh_token";

        private ClientToken clientToken;
        private TokenDictionary clientTokenDictionary;

        private Token token;

        private Coroutine refreshTokenCoroutine;

        private string IdToken => token?.idToken ?? string.Empty;
        private string BearerAccessToken => string.IsNullOrEmpty(token?.accessToken) ? "" : "Bearer " + token.accessToken;
        private string RefreshToken => token?.refreshToken ?? "";

        public ReactiveProperty<Token> tokenReactiveProperty = new(null);
        public ReactiveProperty<ClientToken> clientTokenReactiveProperty = new(null);

        public ReactiveProperty<TokenStatus> tokenStatus = new(TokenStatus.Expired);
        public ReactiveProperty<TokenStatus> clientTokenStatus = new(TokenStatus.Expired);
        public string ClientID { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public ClientType ClientType { get; set; } = ClientType.Public;
        public PassportConfig passportConfig { get; set; }

        [RuntimeInitializeOnLoadMethod]
        public static void TokenRepoOnLoad()
        {
            TokenRepo.Instance.RestoreToken();
        }

        public Token GetToken()
        {
            return token;
        }

        public ClientToken GetClientToken()
        {
            return clientToken;
        }

        public IEnumerator GetClientTokenCoroutine(Action<ClientToken> action) {
            if (ClientIsTokenExpired())
            {
                var applicationId = passportConfig.ApplicationId;
                var applicationKey = passportConfig.ApplicationKey;
                var grantType = passportConfig.GrantType;
                yield return FetchPassportClientToken(applicationId, applicationKey, grantType,
                    (exception, code) => {
                        Debug.Log($"[TokenRepo] GetClientTokenCoroutine exception : {exception}");
                        Debug.Log($"[TokenRepo] GetClientTokenCoroutine code : {code}");
                    }
                );
            }
            action?.Invoke(clientToken);
        }

        public TokenDictionary GetClinetTokenDictionary()
        {
            return clientTokenDictionary;
        }

        public void ClientTokenConfig(ClientToken token)
        {
            this.clientToken = token;
            
            StoreClientToken(token);
            
            if (token != null)
            {
                clientTokenDictionary = new TokenDictionary(BodyDecodeDictionary(token.access_token, DecodingType.BASE64_URL_SAFE));
                long exp = clientTokenDictionary.GetTypedValue<long>(JwtTokenConstants.exp);
                exp = exp > DEFAULT_EFFECTIVE_TIME ?
                    exp : CurrentTimeSeconds() + DEFAULT_EFFECTIVE_TIME;

                clientTokenStatus.Value = TokenStatus.Validate;
            }
            else
            {
                clientTokenStatus.Value = TokenStatus.Expired;
            }
        }

        public void Config(Token token)
        {
            this.token = token;
            
            StoreToken(token);

            if (token != null)
            {
                token.accessTokenDictionary = new TokenDictionary(BodyDecodeDictionary(token.accessToken, DecodingType.BASE64_URL_SAFE));
                token.idTokenDictionary = new TokenDictionary(BodyDecodeDictionary(token.idToken, DecodingType.BASE64_URL_SAFE));
                
                long exp = token.accessTokenDictionary.GetTypedValue<long>(JwtTokenConstants.exp);
                exp = exp > DEFAULT_EFFECTIVE_TIME ? exp : CurrentTimeSeconds() + DEFAULT_EFFECTIVE_TIME;

                //force test code
                //jwtTokenBody.exp = CurrentTimeSeconds() + ESTIMATED_EXPIRATION_TIME + 5;
                tokenStatus.Value = TokenStatus.Validate;
#if MAXST_TOKEN_AUTO_REFRESH
                StartRefreshTokenCoroutine();
#endif
            }
            else
            {
                tokenStatus.Value = TokenStatus.Expired;
                StopRefreshTokenCoroutine();
            }
        }

        private bool IsTokenNotRenewing()
        {
            return tokenStatus.Value != TokenStatus.Renewing;
        }

        private bool IsClientTokenNotRenewing()
        {
            return clientTokenStatus.Value != TokenStatus.Renewing;
        }

        public IEnumerator GetPassportClientToken(
            string applicationId, string applicationKey, string grantType,
            Action<TokenStatus, ClientToken> callback = null,
            Action<ErrorCode, Exception> LoginFailAction = null,
            bool isForcedRefresh = false)
        {
            if (isForcedRefresh || (clientToken == null || ClientIsTokenExpired()))
            {
                yield return new WaitUntil(() => IsClientTokenNotRenewing());
                yield return FetchPassportClientToken(applicationId, applicationKey, grantType, LoginFailAction);
            }

            callback?.Invoke(clientTokenStatus.Value, clientToken);
        }

        public IEnumerator GetPassportToken(
            OpenIDConnectArguments OpenIDConnectArguments, string code, string CodeVerifier,
            System.Action<TokenStatus, Token> callback,
            Action<ErrorCode, Exception> LoginFailAction,
            bool isForcedRefresh = true)
        {
            if (isForcedRefresh || (token == null || IsTokenExpired()))
            {
                yield return new WaitUntil(() => IsTokenNotRenewing());
                yield return FetchPassportToken(OpenIDConnectArguments, code, CodeVerifier, LoginFailAction);
            }

            callback?.Invoke(tokenStatus.Value, token);
        }

        internal IEnumerator GetConfidentialPassportToken(
            OpenIDConnectArguments OpenIDConnectArguments, string code,
            System.Action<TokenStatus, Token> callback,
            Action<ErrorCode, Exception> LoginFailAction
            )
        {
            yield return new WaitUntil(() => IsTokenNotRenewing());
            if (IsTokenExpired())
            {
                yield return FetchConfidentialPassportToken(OpenIDConnectArguments, code, LoginFailAction);
            }
            callback?.Invoke(tokenStatus.Value, token);
        }

        public IEnumerator GetPassportRefreshToken(System.Action<TokenStatus, Token> callback,
             Action<Exception> RefreshFailAction)
        {
            yield return new WaitUntil(() => IsTokenNotRenewing());
            if (IsTokenExpired())
            {
                //Debug.Log($"GetPassportRefreshToken : {RefreshToken}");
                yield return FetchPassportRefreshToken(ClientID, GrantType, RefreshToken, RefreshFailAction);
            }
            callback?.Invoke(tokenStatus.Value, token);
        }

        private void StartRefreshTokenCoroutine()
        {
            StopRefreshTokenCoroutine();
            refreshTokenCoroutine = StartCoroutine(RefreshTokenRoutine());
        }

        private void StopRefreshTokenCoroutine()
        {
            if (refreshTokenCoroutine != null)
            {
                StopCoroutine(refreshTokenCoroutine);
                refreshTokenCoroutine = null;
            }
        }

        private long MeasureRemainTimeSeconds()
        {
            var dict = token?.accessTokenDictionary;
            return dict != null && dict.GetTokenDictionary().ContainsKey(JwtTokenConstants.exp) ?
                dict.GetTypedValue<long>(JwtTokenConstants.exp) - CurrentTimeSeconds() : 0;
        }

        private long MeasureRefeshRemainTimeSeconds()
        {
            if (token == null) return 0;

            var dict = token?.accessTokenDictionary;
            int refreshExpiresIn = token?.refreshExpiresIn ?? 0;

            return dict != null && dict.GetTokenDictionary().ContainsKey(JwtTokenConstants.iat) ?
                dict.GetTypedValue<long>(JwtTokenConstants.iat) + refreshExpiresIn - CurrentTimeSeconds() : 0;
        }

        public bool IsTokenExpired()
        {
            return MeasureRemainTimeSeconds() < ESTIMATED_EXPIRATION_TIME;
        }

        public bool IsRefreshTokenExpired()
        {
            return MeasureRefeshRemainTimeSeconds() < ESTIMATED_EXPIRATION_TIME;
        }


        private long ClinetTokenMeasureRemainTimeSeconds()
        {
            var dict = clientTokenDictionary;

            return dict != null && dict.GetTokenDictionary().ContainsKey(JwtTokenConstants.exp) ?
                dict.GetTypedValue<long>(JwtTokenConstants.exp) - CurrentTimeSeconds() : 0;
        }

        public bool ClientIsTokenExpired()
        {
            return ClinetTokenMeasureRemainTimeSeconds() < ESTIMATED_EXPIRATION_TIME;
        }

        private long CurrentTimeSeconds()
        {
            return (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        private IEnumerator RefreshTokenRoutine()
        {
            while (true)
            {
                if (IsTokenExpired())
                {
                    if (IsRefreshTokenExpired())
                    {
                        Debug.LogWarning(" Refresh token has expired. Please log in again.");
                        yield break;
                    }

                    yield return FetchPassportRefreshToken(ClientID, GrantType, RefreshToken
                        , e =>
                        {

                        });
                    if (tokenStatus.Value != TokenStatus.Validate)
                    {
                        yield return new WaitForSeconds(5);
                    }
                }

                var time = MeasureRemainTimeSeconds();
                yield return new WaitForSeconds(System.Math.Max(time / 2, 5));
            }
        }

        public void PassportLogout(System.Action success = null, System.Action<System.Exception> fail = null)
        {
            StopRefreshTokenCoroutine();
            System.IObservable<System.Object> ob = null;

            ob = AuthService.Instance.PassportLogout(BearerAccessToken, ClientID, IdToken);
            LogoutSubscribeOn(ob, success, fail);
        }

        private void LogoutSubscribeOn(IObservable<System.Object> ob, Action success, Action<Exception> fail)
        {
            ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>   // on success
                {
                    Debug.Log($"[SessionLogout] : {data}");
                },
                error => // on error
                {
                    Config(null);
                    Debug.Log($"[SessionLogout] error {error}");
                    fail?.Invoke(error);
                },
                () =>
                {
                    Config(null);
                    Debug.Log("[SessionLogout] success");
                    success?.Invoke();
                });
        }

        private IEnumerator FetchPassportRefreshToken(string clientId, string grantType, string refreshToken,
            Action<Exception> RefreshFailAction)
        {
            IObservable<CredentialsToken> ob;

            if (ClientType == ClientType.Public)
            {
                ob = AuthService.Instance.PassportRefreshToken(clientId, grantType, refreshToken);
            }
            else {
                ob = AuthService.Instance.PassportConfidentialRefreshToken(clientId, grantType, refreshToken, ClientSecret);
            }

            tokenStatus.Value = TokenStatus.Renewing;

            var disposable = ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .OnErrorRetry((Exception ex) => Debug.Log(ex), retryCount: 3, TimeSpan.FromSeconds(1))
                .Subscribe(data =>   // on success
                {
                    Debug.Log($"[FetchPassportRefreshToken] {ClientType} data : " + data);
                    if (data != null)
                    {
                        Config(new Token
                        {
                            idToken = data.id_token,
                            accessToken = data.access_token,
                            refreshToken = data.refresh_token,
                            refreshExpiresIn = data.refresh_expires_in,
                        });
                    }
                    else
                    {
                        tokenStatus.Value = TokenStatus.Expired;
                        RefreshFailAction.Invoke(null);
                    }
                },
                error => // on error
                {
                    Debug.LogWarning($"[FetchPassportRefreshToken] error : {error}");
                    tokenStatus.Value = TokenStatus.Expired;
                    RefreshFailAction.Invoke(error);
                },
                () =>
                {
                    //Debug.Log("FetchRefreshToken complte : ");
                });

            yield return new WaitUntil(() => tokenStatus.Value != TokenStatus.Renewing);
            disposable.Dispose();
        }

        private IEnumerator FetchConfidentialPassportToken(
            OpenIDConnectArguments OpenIDConnectArguments, string Code,
            Action<ErrorCode, Exception> LoginFailAction)
        {
            tokenStatus.Value = TokenStatus.Renewing;

            IObservable<CredentialsToken> ob = null;

            var Setting = EnvAdmin.Instance.OpenIDConnectSetting;
            Setting.TryGetValue(OpenIDConnectSettingKey.GrantType, out var GrantType);

            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.ClientID, out var ClientID);

            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.ClientSecret, out var ClientSecret);

#if UNITY_ANDROID
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.AndroidRedirectUri, out var RedirectURI);
#elif UNITY_IOS
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.iOSRedirectUri, out var RedirectURI);
#else
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.WebRedirectUri, out var RedirectURI);
#endif

            Debug.Log($"[FetchToken] ConfidentialPassportToken ClientID : {ClientID}");
            Debug.Log($"[FetchToken] ConfidentialPassportToken ClientSecret : {ClientSecret}");
            Debug.Log($"[FetchToken] ConfidentialPassportToken GrantType : {GrantType}");
            Debug.Log($"[FetchToken] ConfidentialPassportToken RedirectURI : {RedirectURI}");
            Debug.Log($"[FetchToken] ConfidentialPassportToken code : {Code}");

            ob = AuthService.Instance.ConfidentialPassportToken(ClientID, ClientSecret, GrantType, RedirectURI, Code);

            var disposable = TokenSubscribeOn(ob, LoginFailAction);

            yield return new WaitUntil(() => tokenStatus.Value != TokenStatus.Renewing);
            disposable.Dispose();
        }

        private IEnumerator FetchPassportClientToken(string applicationId, string applicationKey, string grantType, Action<ErrorCode, Exception> FailAction)
        {
            clientTokenStatus.Value = TokenStatus.Renewing;

            IObservable<ClientToken> ob = null;

            ob = AuthService.Instance.PassportClientToken(applicationId, applicationKey, grantType);
            
            var disposable = ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                                .ObserveOn(Scheduler.MainThread)
                                .OnErrorRetry((Exception ex) => Debug.Log(ex), retryCount: 3, TimeSpan.FromSeconds(1))
                                .Subscribe(data =>   // on success
                                {
                                    Debug.Log("[FetchPassportAppToken] FetchPassportAppToken : " + data);
                                    if (data != null)
                                    {
                                        ClientTokenConfig(data);
                                        clientTokenReactiveProperty.Value = data;
                                    }
                                    else
                                    {
                                        clientTokenStatus.Value = TokenStatus.Expired;
                                        //LoginFailAction?.Invoke(null);
                                    }
                                },
                                error => // on error
                                {
                                    Debug.LogWarning($"[FetchPassportAppToken] FetchPassportAppToken error : {error}");
                                    clientTokenStatus.Value = TokenStatus.Expired;
                                    FailAction?.Invoke(ErrorCode.TOKEN_IS_EMPTY, error);
                                },
                                () =>
                                {
                                    Debug.Log("[FetchPassportAppToken] FetchPassportAppToken complte : ");
                                });

            yield return new WaitUntil(() => IsClientTokenNotRenewing());

            disposable.Dispose();
        }

        private IEnumerator FetchPassportToken(
            OpenIDConnectArguments OpenIDConnectArguments, string Code, string CodeVerifier,
            Action<ErrorCode, Exception> LoginFailAction
        )
        {
            tokenStatus.Value = TokenStatus.Renewing;

            IObservable<CredentialsToken> ob = null;

            var Setting = EnvAdmin.Instance.OpenIDConnectSetting;
            Setting.TryGetValue(OpenIDConnectSettingKey.GrantType, out var GrantType);

            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.ClientID, out var ClientID);

#if UNITY_ANDROID
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.AndroidRedirectUri, out var RedirectURI);
#elif UNITY_IOS
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.iOSRedirectUri, out var RedirectURI);
#else
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.WebRedirectUri, out var RedirectURI);
#endif

            Debug.Log($"[FetchToken] PassportToken ClientID : {ClientID}");
            Debug.Log($"[FetchToken] PassportToken CodeVerifier : {CodeVerifier}");
            Debug.Log($"[FetchToken] PassportToken GrantType : {GrantType}");
            Debug.Log($"[FetchToken] PassportToken RedirectURI : {RedirectURI}");
            Debug.Log($"[FetchToken] PassportToken code : {Code}");

            ob = AuthService.Instance.PassportToken(ClientID, CodeVerifier, GrantType, RedirectURI, Code);

            var disposable = TokenSubscribeOn(ob, LoginFailAction);

            yield return new WaitUntil(() => IsTokenNotRenewing());
            disposable.Dispose();
        }

        private IDisposable TokenSubscribeOn(IObservable<CredentialsToken> ob, Action<ErrorCode, Exception> LoginFailAction)
        {
            return ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                        .ObserveOn(Scheduler.MainThread)
                        .OnErrorRetry((Exception ex) => Debug.Log(ex), retryCount: 3, TimeSpan.FromSeconds(1))
                        .Subscribe(data =>   // on success
                        {
                            Debug.Log("[FetchToken] FetchToken : " + data);
                            if (data != null)
                            {
                                var idToken = data.id_token;
                                var accessToken = data.access_token;
                                var refreshToken = data.refresh_token;
                                
                                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                                {
                                    LoginFailAction?.Invoke(ErrorCode.TOKEN_IS_EMPTY, null);
                                    tokenStatus.Value = TokenStatus.Expired;
                                    Config(null);
                                }
                                else
                                {
                                    var t = new Token
                                    {
                                        idToken = data.id_token,
                                        accessToken = data.access_token,
                                        refreshToken = data.refresh_token,
                                        refreshExpiresIn = data.refresh_expires_in,
                                    };
                                    Config(t);
                                    tokenReactiveProperty.Value = t;
                                }
                            }
                            else
                            {
                                tokenStatus.Value = TokenStatus.Expired;
                            }
                        },
                        error => // on error
                        {
                            Debug.LogWarning($"[FetchToken] FetchToken error : {error}");
                            tokenStatus.Value = TokenStatus.Expired;
                            LoginFailAction?.Invoke(ErrorCode.TOKEN_IS_EMPTY, error);
                        },
                        () =>
                        {
                            Debug.Log("[FetchToken] FetchToken complte : ");
                        });
        }

        private void StoreToken(Token token = null)
        {
            PlayerPrefs.SetString(IdTokenKey, token?.idToken ?? "");
            PlayerPrefs.SetString(AccessTokenKey, token?.accessToken ?? "");
            PlayerPrefs.SetString(RefreshTokenKey, token?.refreshToken ?? "");
            PlayerPrefs.SetInt(RefreshExpiresIn, token?.refreshExpiresIn ?? 0);
        }

        private void StoreClientToken(ClientToken token = null)
        {
            PlayerPrefs.SetString(ClientAccessTokenKey, token?.access_token ?? "");
        }

        private void RestoreToken()
        {
            var idToken = PlayerPrefs.GetString(IdTokenKey, "");
            var accessToken = PlayerPrefs.GetString(AccessTokenKey, "");
            var refreshToken = PlayerPrefs.GetString(RefreshTokenKey, "");
            var refreshExpiresIn = PlayerPrefs.GetInt(RefreshExpiresIn);

            if (string.IsNullOrEmpty(accessToken)
                || string.IsNullOrEmpty(refreshToken))
            {
                Config(null);
            }
            else
            {
                Config(new Token
                {
                    idToken = idToken,
                    accessToken = accessToken,
                    refreshToken = refreshToken,
                    refreshExpiresIn = refreshExpiresIn
                });
            }

            var clientAccessToken = PlayerPrefs.GetString(ClientAccessTokenKey, "");
            if (string.IsNullOrEmpty(clientAccessToken))
            {
                ClientTokenConfig(null);
            }
            else
            {
                ClientTokenConfig(new ClientToken
                {
                    access_token = clientAccessToken,
                });
            }
        }
    }
}
