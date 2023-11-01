using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Maxst.Passport;
using Maxst.Settings;
using System;


namespace Maxst.Passport
{
    public class OpenTab : MonoBehaviour
    {
#if !UNITY_EDITOR && UNITY_WEBGL
    //[DllImport("__Internal")]
    //private static extern void OpenNewTab(string url);
    //[DllImport("__Internal")]
    //private static extern string GetCookie(string name);
#endif

        public Action<string, string> LoginSuccessAction { get; set; } = null;
        public Action<ErrorCode> LoginFailAction { get; set; } = null;

        private Coroutine CheckTokenCoroutine;


        private void OnApplicationPause(bool pause)
        {
            Debug.Log($"OnApplicationPause {pause}");
        }

        private void OpenIt(string url)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
        //OpenNewTab(url);
        //StartCheckToken();
#endif
        }

        private void StartCheckToken()
        {
            StopCheckToken();
            CheckTokenCoroutine = StartCoroutine(ProcessCheckToken());
        }

        private void StopCheckToken()
        {
            if (CheckTokenCoroutine != null)
            {
                StopCoroutine(CheckTokenCoroutine);
                CheckTokenCoroutine = null;
            }
        }

        private IEnumerator ProcessCheckToken()
        {
            string accessToken = "";
            string refreshToken = "";
#if !UNITY_EDITOR && UNITY_WEBGL
        //while (true)
        //{
        //    accessToken = GetCookie(CanvasWebViewForSSO.COOKIE_ID_TOKEN);
        //    if (string.IsNullOrEmpty(accessToken))
        //    {
        //        yield return new WaitForSeconds(1);
        //        continue;
        //    }
        //    Debug.Log($"ProcessCheckToken COOKIE_ID_TOKEN : {accessToken}");

        //    refreshToken = GetCookie(CanvasWebViewForSSO.COOKIE_REFRESH_TOKEN);
        //    if (string.IsNullOrEmpty(refreshToken))
        //    {
        //        yield return new WaitForSeconds(1);
        //        continue;
        //    }
        //    Debug.Log($"ProcessCheckToken COOKIE_REFRESH_TOKEN : {refreshToken}");
        //    break;
        //}
#endif

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                LoginFailAction?.Invoke(ErrorCode.TOKEN_IS_EMPTY);
                TokenRepo.Instance.Config(null);
            }
            else
            {
                LoginSuccessAction?.Invoke(accessToken, refreshToken);
                TokenRepo.Instance.Config(new Token
                {
                    accessToken = accessToken,
                    refreshToken = refreshToken,
                });
            }

            Debug.Log($"ProcessCheckToken end");
            yield break;
        }
    }
}