#define API_DETAIL_DEBUG
#if false
using Cysharp.Threading.Tasks;
using Maxst;
using maxstAR;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;


namespace MaxstXR.Place
{
    public partial class CustomerRepo
    {
        public const string ACCESS_TOKEN = "access_token";
        public const string EXPIRES_IN = "expires_in";
        public const string REFRESH_EXPIRES_IN = "refresh_expires_in";
        public const string TOKEN_TYPE = "token_type";
        public const string NOT_BEFORE_POLICY = "not_before_policy";
        public const string SCOPE = "scope";

        const long SAFETY_TIME_MARGIN = 10;  // 10 sec
        private ReactiveProperty<CredentialsToken> credentialsToken = new(null);
        public ReadOnlyReactiveProperty<CredentialsToken> CredentialsToken => credentialsToken.ToReadOnlyReactiveProperty();
        private bool isWaitingTokenUpdate = false;

        public void FetchToken(RxJob job)
        {
            var ob = CustomerOpenIdService.Instance.Token();
            ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>   // on success
                {
                    Upsert(data);
                    Debug.Log($"OpenId Token success : {data?.accessToken}");
                },
                error => // on error
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"OpenId Token fail : {error}");
#endif
                    if (job != null)
                    {
                        job.Exception = error;
                        job.IsDone = true;
                    }
                },
                () =>
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"OpenId Token complete");
#endif
                    if (job != null) job.IsDone = true;
                });
        }

        private void RefreshToken()
        {
            if (false == IsTokenExpired()) return;

            if (GetTokenExpireTime() - SAFETY_TIME_MARGIN < TimeUtil.CurrentTimeMillis() / 1000)
            {
                var ob = CustomerOpenIdService.Instance.Token();
                ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                    .ObserveOn(Scheduler.MainThread)
                    .Subscribe(data =>   // on success
                    {
                        Upsert(data);
                        Debug.Log($"Refresh Token success : {data?.accessToken}");
                    },
                    error => // on error
                    {
#if API_DETAIL_DEBUG
                        Debug.Log($"OpenId Token fail : {error}");
#endif
                    });
            }
        }

        public IEnumerator GetToken(System.Action<CredentialsToken> result)
        {
            yield return new WaitUntil(() => isWaitingTokenUpdate == false);

            isWaitingTokenUpdate = false;

            if (GetTokenExpireTime() - SAFETY_TIME_MARGIN < TimeUtil.CurrentTimeMillis() / 1000)
            {
                isWaitingTokenUpdate = true;

                var ob = CustomerOpenIdService.Instance.Token();
                ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                    .ObserveOn(Scheduler.MainThread)
                    .Subscribe(data =>   // on success
                    {
                        Upsert(data);
                        isWaitingTokenUpdate = false;
                        Debug.Log($"OpenId Token success : {data?.accessToken}");
                    },
                    error => // on error
                    {
                        isWaitingTokenUpdate = false;
#if API_DETAIL_DEBUG
                        Debug.Log($"OpenId Token fail : {error}");
#endif
                    });
            }

            yield return new WaitUntil(() => isWaitingTokenUpdate == false);
            result?.Invoke(credentialsToken.Value);
        }

        public async UniTask<CredentialsToken> GetAuthoringToken()
        {
            var completionSource = new TaskCompletionSource<CredentialsToken>();
            var ob = CustomerOpenIdService.Instance.AuthoringToken();
            ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>   // on success
                    {
                        Debug.Log($"OpenId Token success : {data?.accessToken}");
                        completionSource.TrySetResult(data);
                    },
                    error => // on error
                    {
                        Debug.Log($"OpenId Token fail : {error}");
                        completionSource.TrySetException(error);
                    });

            return await completionSource.Task;
        }

        public Dictionary<string, string> GetHeaders()
        {
            if (IsTokenExpired())
            {
                RefreshToken();
            }

            return new Dictionary<string, string>()
            {
                { "Authorization",  "Bearer " + credentialsToken.Value.accessToken},
                { "X-App-Key", ApiConst.APP_KEY }
            };
        }

        private bool IsTokenExpired()
        {
            return GetTokenExpireTime() - 30 < CurrentTimeSeconds();
        }

        private bool IsTokenExpired(long expiresIn)
        {
            return expiresIn - 30 < CurrentTimeSeconds();
        }

        private long CurrentTimeSeconds()
        {
            return (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        private void Upsert(CredentialsToken data)
        {
            credentialsToken.Value = data;
            PlayerPrefs.SetString(ACCESS_TOKEN, data.accessToken);
            PlayerPrefs.SetString(EXPIRES_IN, ExpireSecToUnixTime(data.expiresIn));
            PlayerPrefs.SetString(REFRESH_EXPIRES_IN, data.refreshExpiresIn.ToString());
            PlayerPrefs.SetString(TOKEN_TYPE, data.tokenType);
            PlayerPrefs.SetString(NOT_BEFORE_POLICY, data.notBeforePolicy.ToString());
            PlayerPrefs.SetString(SCOPE, data.scope);
        }

        public long GetTokenExpireTime()
        {
            return credentialsToken.Value != null ? System.Convert.ToInt64(PlayerPrefs.GetString(EXPIRES_IN, "0")) : 0L;
        }

        private string ExpireSecToUnixTime(long expireSec)
        {
            var now = TimeUtil.CurrentTimeMillis() / 1000;
            return (now + expireSec).ToString();
        }
    }
}
#endif