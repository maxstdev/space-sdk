using System.Collections.Generic;

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
    
    public abstract class OpenIDConnectSetting
    {
        public abstract Dictionary<OpenIDConnectSettingKey, string> Urls { get; }

        public static Dictionary<EnvType, OpenIDConnectSetting> Datas { get; } = new Dictionary<EnvType, OpenIDConnectSetting>()
        {
            { EnvType.Alpha,  new OpenIDConnectSettingAlpha() },
            { EnvType.Beta,  new OpenIDConnectSettingBeta() },
            { EnvType.Prod,  new OpenIDConnectSettingProd() },
        };
    }

    public class OpenIDConnectSettingAlpha : OpenIDConnectSetting
    {
        public override Dictionary<OpenIDConnectSettingKey, string> Urls { get; } = new Dictionary<OpenIDConnectSettingKey, string>()
        {
            { OpenIDConnectSettingKey.CodeChallengeMethod, "S256" },
            { OpenIDConnectSettingKey.LoginAPI, "/passport/authorize" },
            { OpenIDConnectSettingKey.PublicLoginUrl, "{0}{1}?client_id={2}&response_type={3}&scope={4}&redirect_uri={5}&code_challenge={6}&code_challenge_method={7}" },
            { OpenIDConnectSettingKey.ConfidentialLoginUrl, "{0}{1}?client_id={2}&response_type={3}&scope={4}&redirect_uri={5}" },
            { OpenIDConnectSettingKey.GrantType, "authorization_code" },
        };
    }

    public class OpenIDConnectSettingBeta : OpenIDConnectSetting
    {
        public override Dictionary<OpenIDConnectSettingKey, string> Urls { get; } = new Dictionary<OpenIDConnectSettingKey, string>()
        {
            { OpenIDConnectSettingKey.CodeChallengeMethod, "S256" },
            { OpenIDConnectSettingKey.LoginAPI, "/passport/authorize" },
            { OpenIDConnectSettingKey.PublicLoginUrl, "{0}{1}?client_id={2}&response_type={3}&scope={4}&redirect_uri={5}&code_challenge={6}&code_challenge_method={7}" },
            { OpenIDConnectSettingKey.ConfidentialLoginUrl, "{0}{1}?client_id={2}&response_type={3}&scope={4}&redirect_uri={5}" },
            { OpenIDConnectSettingKey.GrantType, "authorization_code" },
        };
    }

    public class OpenIDConnectSettingProd : OpenIDConnectSetting
    {
        public override Dictionary<OpenIDConnectSettingKey, string> Urls { get; } = new Dictionary<OpenIDConnectSettingKey, string>()
        {
            { OpenIDConnectSettingKey.CodeChallengeMethod, "S256" },
            { OpenIDConnectSettingKey.LoginAPI, "/passport/authorize" },
            { OpenIDConnectSettingKey.PublicLoginUrl, "{0}{1}?client_id={2}&response_type={3}&scope={4}&redirect_uri={5}&code_challenge={6}&code_challenge_method={7}" },
            { OpenIDConnectSettingKey.ConfidentialLoginUrl, "{0}{1}?client_id={2}&response_type={3}&scope={4}&redirect_uri={5}" },
            { OpenIDConnectSettingKey.GrantType, "authorization_code" },
        };
    }
}
