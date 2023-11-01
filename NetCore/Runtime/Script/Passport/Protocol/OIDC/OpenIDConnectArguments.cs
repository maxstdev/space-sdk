using UnityEngine;


namespace Maxst.Settings
{
    public enum OpenIDConnectArgument
    {
        ClientID = 0,
        ClientSecret,
        ResponseType,
        Scope,
        AndroidRedirectUri,
        iOSRedirectUri,
        WebRedirectUri,
    }

    [CreateAssetMenu(fileName = "OpenIDConnectArguments", menuName = "Packages/Passport/OpenIDConnectArguments", order = 1000)]
    public class OpenIDConnectArguments : ScriptableDictionary<OpenIDConnectArgument, string>
    {
        public void SetValue(OpenIDConnectArgument key, string value)
        {
            this[key] = value;
        }
    }
}