using System.Collections.Generic;
using UnityEngine;


namespace Maxst.Settings
{
    public class EnvUrlSetting
    {
        public static Dictionary<DomainType, EnvSetting> Domains { get; } = new Dictionary<DomainType, EnvSetting>()
        {
            { DomainType.maxst,  new EnvSettingFromMaxst() }
        };
    }
}
