using UnityEngine;

namespace Maxst.Settings
{
    public enum OpenIDConnectSettingKey
    {
        CodeChallengeMethod,
        LoginAPI,
        PublicLoginUrl,
        ConfidentialLoginUrl,
        GrantType
    }

    [CreateAssetMenu(fileName = "OpenIDConnectSetting", menuName = "Packages/Passport/OpenIDConnectSetting", order = 1000)]
    public class OpenIDConnectSetting : ScriptableDictionary<OpenIDConnectSettingKey, string>
    {

    }
}
