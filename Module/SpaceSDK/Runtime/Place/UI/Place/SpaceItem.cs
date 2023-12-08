using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MaxstXR.Place
{
    public class SpaceItem : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI spaceName;
        [SerializeField] protected Button itemButton;

        public virtual void Config(Space space, UnityAction<Space> spaceClickAction = null, bool isRemoveAll = false)
        {
            spaceName.text = space?.SpaceName ?? string.Empty;
            spaceName.text += $"({space.SpaceId})";
            if (isRemoveAll) itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(() =>
            {
                spaceClickAction?.Invoke(space);
            });
        }
    }
}
