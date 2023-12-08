using Maxst.Passport;
using System;
using UniRx;
using UnityEngine;

namespace Maxst.Settings
{
    public class EnvAdmin : MaxstUtils.Singleton<EnvAdmin>
    {
        public ReactiveProperty<EnvType> CurrentEnv = new ReactiveProperty<EnvType>(EnvType.Prod);
        public ReactiveProperty<DomainType> CurrentEnvUrl = new ReactiveProperty<DomainType>(DomainType.maxst);
        public ReactiveProperty<LngType> CurrentEnvLng = new ReactiveProperty<LngType>(LngType.ko);

        public const string EnvTypeKey = "EnvType";
        public const string EnvUrlTypeKey = "EnvUrlType";
        public const string EnvLngTypeKey = "EnvLngType";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void NetworkManagerOnLoad()
        {
            EnvAdmin.Instance.ConfigEnvType();
        }

        public AuthUrlSetting AuthUrlSetting
        {
            get
            {
                return CurrentEnvUrl.Value.EnvSetting().Envs[CurrentEnv.Value].authUrlSetting;
            }
        }

        public OpenIDConnectSetting OpenIDConnectSetting
        {
            get
            {
                return CurrentEnvUrl.Value.EnvSetting().Envs[CurrentEnv.Value].OpenIDConnectSetting;
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
            CurrentEnv.Value = (EnvType)PlayerPrefs.GetInt(EnvTypeKey, (int)EnvType.Prod);
            CurrentEnvUrl.Value = (DomainType)PlayerPrefs.GetInt(EnvUrlTypeKey, (int)DomainType.maxst);
            CurrentEnvLng.Value = (LngType)PlayerPrefs.GetInt(EnvLngTypeKey, (int)LngType.ko);
        }

        public void SetConfiguration(
            EnvType envType,
            DomainType domainType,
            LngType lngType,
            Action OnInitialized = null
        )
        {
            var savedEnvType = PlayerPrefs.GetInt(EnvTypeKey);

            if ((EnvType)savedEnvType != envType)
            {
                PlayerPrefs.DeleteKey(EnvTypeKey);
                PlayerPrefs.DeleteKey(EnvUrlTypeKey);
                PlayerPrefs.DeleteKey(EnvLngTypeKey);

                OnInitialized?.Invoke();
            }

            UpdateEnvType(envType);
            UpdateEnvUrlType(domainType);
            UpdateEnvLngType(lngType);
        }

        public void UpdateEnvType(EnvType value)
        {
            PlayerPrefs.SetInt(EnvTypeKey, (int)value);
            CurrentEnv.Value = value;
        }

        public void UpdateEnvUrlType(DomainType value)
        {
            PlayerPrefs.SetInt(EnvUrlTypeKey, (int)value);
            CurrentEnvUrl.Value = value;
        }

        public void UpdateEnvLngType(LngType value)
        {
            PlayerPrefs.SetInt(EnvLngTypeKey, (int)value);
            CurrentEnvLng.Value = value;
        }
    }
}