using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MaxstXR.Place
{
    public class PlaceItem : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI placeName;
        [SerializeField] protected Button itemButton;

        public virtual void Config(Place place, UnityAction<Place> placeClickAction = null, bool isRemoveAll = false)
        {
            placeName.text = place?.PlaceName ?? string.Empty;
            placeName.text += $"({place.PlaceId})";
            if (isRemoveAll) itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(() =>
            {
                placeClickAction?.Invoke(place);
            });
        }
    }
}
