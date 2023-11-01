using TMPro;
using UniRx;
using UnityEngine;
using Maxst.Settings;

namespace Maxst.Passport
{
    public class TitleInfo : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI appNaming;
        [SerializeField] private TextMeshProUGUI title;

        private void OnEnable()
        {
            EnvAdmin.Instance.CurrentEnv
                .DistinctUntilChanged()
                .Subscribe(UpdateTitleInfo)
                .AddTo(this);
            UpdateTitleInfo(EnvAdmin.Instance.CurrentEnv.Value);
        }

        private void UpdateTitleInfo(EnvType envType)
        {
            appNaming.text = $"MaxChat v{Application.version}{envType.Meta()}";
        }
    }

}
