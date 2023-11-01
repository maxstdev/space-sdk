using Maxst.Passport;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

namespace Maxst.Settings
{
    public class SettingMenu : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private TMP_Dropdown urlDropdown;
        [SerializeField] private TMP_Dropdown lngDropdown;
        [SerializeField] private Button completeBtn;

        public Action OnCompleteAction { get; set; } = null;

        private void OnEnable()
        {
            var options = new List<OptionData>();
            foreach (var env in Enum.GetValues(typeof(EnvType)))
            {
                options.Add(new OptionData(env.ToString()));
            }

            var urlOptions = new List<OptionData>();
            foreach (var env in Enum.GetValues(typeof(DomainType)))
            {
                urlOptions.Add(new OptionData(env.ToString()));
            }

            var lngOptions = new List<OptionData>();
            foreach (var env in Enum.GetValues(typeof(LngType)))
            {
                lngOptions.Add(new OptionData(env.ToString()));
            }

            dropdown.options = options;
            dropdown.value = (int)EnvAdmin.Instance.CurrentEnv.Value;
            dropdown.onValueChanged.AddListener(OnEnvTypeChange);

            urlDropdown.options = urlOptions;
            urlDropdown.value = (int)EnvAdmin.Instance.CurrentEnvUrl.Value;
            urlDropdown.onValueChanged.AddListener(OnEnvUrlTypeChange);

            lngDropdown.options = lngOptions;
            lngDropdown.value = (int)EnvAdmin.Instance.CurrentEnvLng.Value;
            lngDropdown.onValueChanged.AddListener(OnEnvLngTypeChange);

            completeBtn.onClick.AddListener(OnClickCompleteBtn);
        }

        private void OnDisable()
        {
            dropdown.onValueChanged.RemoveListener(OnEnvTypeChange);
            urlDropdown.onValueChanged.RemoveListener(OnEnvUrlTypeChange);
            lngDropdown.onValueChanged.RemoveListener(OnEnvLngTypeChange);
            completeBtn.onClick.RemoveListener(OnClickCompleteBtn);
        }

        private void OnEnvTypeChange(int type)
        {
            EnvAdmin.Instance.UpdateEnvType((EnvType)type);
        }
        private void OnEnvUrlTypeChange(int type)
        {
            EnvAdmin.Instance.UpdateEnvUrlType((DomainType)type);
        }

        private void OnEnvLngTypeChange(int type)
        {
            EnvAdmin.Instance.UpdateEnvLngType((LngType)type);
        }

        private void OnClickCompleteBtn()
        {
            OnCompleteAction?.Invoke();
        }
    }
}
