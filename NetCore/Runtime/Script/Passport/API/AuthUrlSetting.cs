using Maxst.Settings;
using System.Collections.Generic;

namespace Maxst.Passport
{
    public enum URLType
    {
        API, Location
    }

    public abstract class AuthUrlSetting
    {
        public abstract Dictionary<URLType, string> Urls { get; }

        public static Dictionary<EnvType, AuthUrlSetting> Datas { get; } = new Dictionary<EnvType, AuthUrlSetting>()
        {
            { EnvType.Alpha,  new AuthUrlSettingAlpha() },
            { EnvType.Beta,  new AuthUrlSettingBeta() },
            { EnvType.Prod,  new AuthUrlSettingProd() },
        };

    }

    public class AuthUrlSettingAlpha : AuthUrlSetting
    {
        public override Dictionary<URLType, string> Urls { get; } = new Dictionary<URLType, string>()
        {
            { URLType.API, "https://alpha-api.maxst.com" },
            { URLType.Location, "https://alpha-passport.maxst.com/return-to-app" },
        };
    }

    public class AuthUrlSettingBeta : AuthUrlSetting
    {
        public override Dictionary<URLType, string> Urls { get; } = new Dictionary<URLType, string>()
        {
            { URLType.API, "https://api.maxst.com" },
            { URLType.Location, "https://passport.maxst.com/return-to-app" },
        };
    }

    public class AuthUrlSettingProd : AuthUrlSetting
    {
        public override Dictionary<URLType, string> Urls { get; } = new Dictionary<URLType, string>()
        {
            { URLType.API, "https://api.maxst.com" },
            { URLType.Location, "https://passport.maxst.com/return-to-app" },
        };
    }
}