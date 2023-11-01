using Maxst.Passport;
using UniRx;
using UnityEngine;

namespace Maxst.Settings
{
    public class EnvAdmin : MaxstUtils.Singleton<EnvAdmin>
    {
        public ReactiveProperty<EnvType> CurrentEnv = new ReactiveProperty<EnvType>(EnvType.Prod);
        public ReactiveProperty<DomainType> CurrentEnvUrl = new ReactiveProperty<DomainType>(DomainType.maxst);
        public ReactiveProperty<LngType> CurrentEnvLng = new ReactiveProperty<LngType>(LngType.ko);

        [RuntimeInitializeOnLoadMethod]
        static void NetworkManagerOnLoad()
        {
            EnvAdmin.Instance.ConfigEnvType();
        }

        public AuthUrlSetting AuthUrlSetting
        {
            get
            {
                return CurrentEnvUrl.Value.EnvSetting()[CurrentEnv.Value].authUrlSetting;
            }
        }
        
        public OpenIDConnectSetting OpenIDConnectSetting
        {
            get
            {
                return CurrentEnvUrl.Value.EnvSetting()[CurrentEnv.Value].OpenIDConnectSetting;
            }
        }

        public LngType LocaleSetting()
        {
            return CurrentEnvLng.Value;
        }

        public bool IsAuthUrl()
        {
            return CurrentEnvUrl.Value == DomainType.maxst;
        }

        public void ConfigEnvType()
        {
            CurrentEnv.Value = (EnvType)PlayerPrefs.GetInt("EnvType", (int)EnvType.Prod);
            CurrentEnvUrl.Value = (DomainType)PlayerPrefs.GetInt("EnvUrlType", (int)DomainType.maxst);
            CurrentEnvLng.Value = (LngType)PlayerPrefs.GetInt("EnvLngType", (int)LngType.ko);
        }

        public void UpdateEnvType(EnvType value)
        {
            PlayerPrefs.SetInt("EnvType", (int)value);
            CurrentEnv.Value = value;
        }

        public void UpdateEnvUrlType(DomainType value)
        {
            PlayerPrefs.SetInt("EnvUrlType", (int)value);
            CurrentEnvUrl.Value = value;
        }

        public void UpdateEnvLngType(LngType value)
        {
            PlayerPrefs.SetInt("EnvLngType", (int)value);
            CurrentEnvLng.Value = value;
        }
    }
}