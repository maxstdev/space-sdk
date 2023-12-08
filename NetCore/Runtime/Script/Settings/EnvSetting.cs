using UnityEngine;
using Maxst.Passport;
using System.Collections.Generic;

namespace Maxst.Settings
{
    public enum EnvType
    {
        Alpha = 0,
        Beta,
        Prod
    }
    public enum DomainType
    {
        maxst = 0
    }

    public enum LngType
    {
        ko = 0,
        en,
        ja,
    }

    public static class EnvUrlTypeExtensions
    {
        public static EnvSetting EnvSetting(this DomainType type)
        {
            return EnvUrlSetting.Domains[type];
        }
    }

    public static class EnvTypeExtensions
    {

        public static string Meta(this EnvType env)
        {
            return env switch
            {
                EnvType.Beta => "-" + env.ToString().ToLower(),
                EnvType.Alpha => "-" + env.ToString().ToLower(),
                _ => "",
            };
        }

        public static string Prefix(this EnvType env)
        {
            return env.ToString().ToUpper() + "_";
        }
    }

    [System.Serializable]
    public class EnvData
    {
        public AuthUrlSetting authUrlSetting;

        public OpenIDConnectSetting OpenIDConnectSetting;
    }

    
    public abstract class EnvSetting
    {
        public abstract Dictionary<EnvType, EnvData> Envs { get; }
    }

    public class EnvSettingFromMaxst : EnvSetting
    {
        public override Dictionary<EnvType, EnvData> Envs { get; } = new Dictionary<EnvType, EnvData>()
        {
            {
                EnvType.Alpha,
                new EnvData {
                    authUrlSetting = AuthUrlSetting.Datas[EnvType.Alpha],
                    OpenIDConnectSetting = OpenIDConnectSetting.Datas[EnvType.Alpha],
                }
            },
            {
                EnvType.Beta,
                new EnvData {
                    authUrlSetting = AuthUrlSetting.Datas[EnvType.Beta],
                    OpenIDConnectSetting = OpenIDConnectSetting.Datas[EnvType.Beta],
                }
            },
            {
                EnvType.Prod,
                new EnvData {
                    authUrlSetting = AuthUrlSetting.Datas[EnvType.Prod],
                    OpenIDConnectSetting = OpenIDConnectSetting.Datas[EnvType.Prod],
                }
            },
        };
    }
}
