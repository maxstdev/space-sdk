using UnityEngine;

namespace Maxst.Passport
{
    public enum URLType
    {
        API,
    }

    [CreateAssetMenu(fileName = "AuthUrlSetting", menuName = "Packages/Scriptable Dictionary/AuthUrlSetting", order = 1000)]
    public class AuthUrlSetting : ScriptableDictionary<URLType, string>
    {

    }
}