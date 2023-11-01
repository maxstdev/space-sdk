#if UNITY_EDITOR

using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using Maxst.Settings;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Maxst.Passport
{
    [InitializeOnLoad]
    public class PassportLoginEditor : EditorWindow, IOpenIDConnectListener, MaxstIOpenIDConnectProvider
    {
        [SerializeField]
        public OpenIDConnectArguments openIDConnectArguments;

        public ReactiveProperty<TokenStatus> tokenStatus = new(TokenStatus.Expired);

        public OpenIDConnectAdapter openIDConnectAdapter;

        private Button loginButton;
        private Button clientLoginButton;

        private bool isError = false;
        private string errorText = string.Empty;

        private Scene editorScene;

        [MenuItem("Passport/LoginEditor", priority = 99)]
        static void Init()
        {
            PassportLoginEditor window = (PassportLoginEditor)EditorWindow.GetWindow(typeof(PassportLoginEditor));
            window.titleContent = new GUIContent("LoginEditor");
            window.Show();
        }

        [MenuItem("Passport/LoginEditor", isValidateFunction: true)]
        static bool ValidateEditorLogin()
        {
#if EDITOR_LOGIN
            return true;
#else
            return false;
#endif
        }

        void CreateGUI()
        {
            VisualElement root = rootVisualElement;

#if EDITOR_USER_LOGIN
            root.Add(new PropertyField() { bindingPath = nameof(openIDConnectArguments) });

            loginButton = new Button(OnClickLoginButton)
            {
                text = "Login"
            };
            loginButton.style.height = new Length(100);
            root.Add(loginButton);
#endif

#if EDITOR_LOGIN
            clientLoginButton = new Button(OnClickClientLoginButton)
            {
                text = "Client Login"
            };
            clientLoginButton.style.height = new StyleLength(100);

            root.Add(clientLoginButton);
#endif
            if (isError)
            {
                Label lable = new Label();
                lable.style.color = Color.yellow;
                lable.text = errorText;

                root.Add(lable);
            }

            root.Bind(new SerializedObject(this));
        }

        private void OnClickLoginButton()
        {
            UserLogin().Forget();
        }

        private void OnClickClientLoginButton()
        {
            ClientLogin().Forget();
        }

        private async UniTask UserLogin()
        {
            CreateLoginReferenceObject();

            openIDConnectAdapter = OpenIDConnectAdapter.Instance;
            openIDConnectAdapter.InitOpenIDConnectAdapter(openIDConnectArguments);
#if !UNITY_ANDROID && !UNITY_IOS
            openIDConnectAdapter.SetWindowLoginServiceManger(this);
#endif

            UnityEditor.EditorApplication.update += LoginEditorUpdate;

            var PKCEManagerInstance = PKCEManager.GetInstance();
            var CodeVerifier = PKCEManagerInstance.GetCodeVerifier();
            var CodeChallenge = PKCEManagerInstance.GetCodeChallenge(CodeVerifier);
            openIDConnectAdapter.ShowOIDCProtocolLoginPage(CodeVerifier, CodeChallenge);
        }

        private async UniTask ClientLogin()
        {
            CreateLoginReferenceObject();

            var passportConfig = SampleConfig.Instance;

            try
            {
                await TokenRepo.Instance.GetPassportClientToken(
                     passportConfig.ApplicationId,
                     passportConfig.ApplicationKey,
                     passportConfig.GrantType,
                     OnSuccess,
                     OnFail
                 );
            }
            catch (Exception e)
            {
                isError = true;
                errorText = "Fail, TokenRepo GetPassprtClinetToken. \n" +
                    "Click the ClientLogin button again";
                DestoryEditorLoginScene();
            }

        }

        private void CreateLoginReferenceObject()
        {
            isError = false;

            List<GameObject> objs = new();

            var tokenObj = new GameObject("TokenRepo");
            tokenObj.AddComponent<TokenRepo>();

            var envObj = new GameObject("Envadmin");
            var envadmin = envObj.AddComponent<EnvAdmin>();
            envadmin.ConfigEnvType();

            var serviceObj = new GameObject("Service");
            var servicemanager = serviceObj.AddComponent<ServiceManager>();
            servicemanager.CreateRunner();

            var authinstance = AuthService.Instance;

            objs.Add(tokenObj);
            objs.Add(envObj);
            objs.Add(serviceObj);
            objs.Add(authinstance.gameObject);

            MoveEditorLoginObjects(objs);
        }

        private Scene GetEditorScene()
        {
            if (editorScene == null || !editorScene.isLoaded)
            {
                editorScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                editorScene.name = "EditorLogin";
            }
            return editorScene;
        }

        private void MoveEditorLoginObjects(List<GameObject> objs)
        {
            Scene targetScene = GetEditorScene();
            foreach (var obj in objs)
            {
                SceneManager.MoveGameObjectToScene(obj, targetScene);
            }
        }

        private void LoginEditorUpdate()
        {
            ServiceManager.Instance.Update();
        }

        public string GetLoginPageURL(string redirectUri)
        {
            var PKCEManagerInstance = PKCEManager.GetInstance();
            var CodeVerifier = PKCEManagerInstance.GetCodeVerifier();
            var CodeChallenge = PKCEManagerInstance.GetCodeChallenge(CodeVerifier);
            return openIDConnectAdapter.GetURL(redirectUri, CodeVerifier, CodeChallenge);
        }

        public void OnAuthorazationCode(string code)
        {
            Debug.Log($"OnAuthorazationCode : {code}");

            openIDConnectAdapter.EditorAccessToken(code);
        }

        public void OnSuccess(Token Token, RequestType type)
        {
            Debug.Log($"[PassportLoginEditor] OnSuccess RequestType : {type}");
            Debug.Log($"[PassportLoginEditor] OnSuccess idToken : {Token.idToken}");
            Debug.Log($"[PassportLoginEditor] OnSuccess accessToken : {Token.accessToken}");
            Debug.Log($"[PassportLoginEditor] OnSuccess refreshToken : {Token.refreshToken}");

            DestoryEditorLoginScene();
        }

        private void OnSuccess(TokenStatus status, ClientToken clientToken)
        {
            DestoryEditorLoginScene();

            Debug.Log($"[PassportLoginEditor] OnSuccess accessToken : {clientToken.access_token}");
        }

        public void OnFail(ErrorCode ErrorCode, Exception e)
        {
            Debug.Log($"[LoginMenu] OnFail : {ErrorCode}");
            Debug.Log($"[LoginMenu] Exception : {e}");

            isError = true;
            errorText = $"OnFail : {ErrorCode}";

            DestoryEditorLoginScene();
        }

        private void DestoryEditorLoginScene(bool autoDelete = true)
        {
            if (autoDelete)
            {
                SceneManager.UnloadSceneAsync(GetEditorScene());
            }
            EditorUtility.RequestScriptReload();
        }


        public void OnLogout()
        {
            Debug.Log($"[LoginMenu] OnLogout");
        }
    }
}
#endif

