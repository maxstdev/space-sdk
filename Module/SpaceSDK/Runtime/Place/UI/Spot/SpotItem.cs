using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MaxstXR.Place
{
    public class SpotItem : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI spotName;
        [SerializeField] protected Button itemButton;

        public virtual void Config(Spot spot, UnityAction<Spot> itemClickAction = null, bool isRemoveAll = false)
        {
            spotName.text = spot?.SpotName ?? string.Empty;
            if (isRemoveAll) itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(() =>
            {
                itemClickAction?.Invoke(spot);
            });
        }
    }
}
