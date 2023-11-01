using System.Collections.Generic;
using UnityEngine;


namespace Maxst.Settings
{

    [CreateAssetMenu(fileName = "EnvUrlSetting", menuName = "Packages/Scriptable Dictionary/EnvUrlSetting", order = 1000)]
    public class EnvUrlSetting : ScriptableSingleton<EnvUrlSetting>
    {
        [SerializeField] public List<EnvSetting> EnvSettings;
    }
}
