using Maxst.Settings;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Maxst.Passport
{
    public partial class LoginMenu : MonoBehaviour, IOpenIDConnectListener
    {
        [SerializeField] private GameObject loginPopupPrefeb;
        [SerializeField] private Button loginFromEmailBtn;
        [SerializeField] private OpenTab openTab;
        [SerializeField] private Button LogoutBtn;
        [SerializeField] private Button ReFreshBtn;
        [SerializeField] private OpenIDConnectArguments openidConnectArguments;

        [SerializeField] private UnityEvent tokenEvent;
        [SerializeField] private UnityEvent failEvent;
        [SerializeField] private UnityEvent LogoutEvent;

        private OpenIDConnectAdapter OpenIdConnectAdapter;

        private void Awake()
        {
            OnInitialize();
        }

        private void OnClickLogoutBtn()
        {
            OpenIdConnectAdapter.OnLogout();
        }

        private void OnClickloginFromEmailBtn()
        {
            OpenIdConnectAdapter.OpenUrlLoginPage(SampleConfig.Instance);
        }

        private void OnClickRefreshBtn()
        {
            OpenIdConnectAdapter.OnRefreshToken();
        }

        private void GetClientToken()
        {
            var passportConfig = SampleConfig.Instance;
            OpenIdConnectAdapter.OnClientToken(passportConfig,
                (status, token) => {
                    Debug.Log($"[LoginMenu] OnClientToken status : {status}");
                    Debug.Log($"[LoginMenu] OnClientToken token.access_token : {token.access_token}");
                },
                (code, exception) => {
                    Debug.Log($"[LoginMenu] OnClientToken code : {code}");
                    Debug.Log($"[LoginMenu] OnClientToken exception : {exception}");
                }
            );
        }

        void IOpenIDConnectListener.OnSuccess(Token Token, RequestType Type)
        {
            Debug.Log($"[LoginMenu] OnSuccess RequestType : {Type}");
            Debug.Log($"[LoginMenu] OnSuccess idToken : {Token.idToken}");
            Debug.Log($"[LoginMenu] OnSuccess accessToken : {Token.accessToken}");
            Debug.Log($"[LoginMenu] OnSuccess refreshToken : {Token.refreshToken}");
            tokenEvent?.Invoke();
        }

        void IOpenIDConnectListener.OnFail(ErrorCode ErrorCode, Exception e)
        {
            Debug.Log($"[LoginMenu] OnFail : {ErrorCode}");
            Debug.Log($"[LoginMenu] Exception : {e}");
            failEvent?.Invoke();
        }

        void IOpenIDConnectListener.OnLogout()
        {
            Debug.Log($"[LoginMenu] OnLogout" + Application.platform);
            LogoutEvent?.Invoke(); 
        }
    }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
    public partial class LoginMenu
    {
        private void OnInitialize()
        {
            Debug.Log("LoginMenu UNITY");
            loginFromEmailBtn.onClick.AddListener(OnClickloginFromEmailBtn);
            LogoutBtn.onClick.AddListener(OnClickLogoutBtn);
            ReFreshBtn.onClick.AddListener(OnClickRefreshBtn);

            OpenIdConnectAdapter = OpenIDConnectAdapter.Instance;
            OpenIdConnectAdapter.InitOpenIDConnectAdapter(openidConnectArguments, SampleConfig.Instance);
            OpenIdConnectAdapter.SetLoginListener(this);
        }
    }
#elif !UNITY_EDITOR && UNITY_WEBGL
    public partial class LoginMenu
    {
        private void OnInitialize()
        {
            Debug.Log("LoginMenu UNITY_WEBGL");
       }
    }
#else
    public partial class LoginMenu
    {
        private void OnInitialize()
        {
            Debug.Log("LoginMenu not imeplement" : + Application.platform);
        }
    }
#endif
}