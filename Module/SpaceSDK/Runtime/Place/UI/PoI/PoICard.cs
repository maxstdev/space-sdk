using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MaxstXR.Place
{
    public class PoICard : InjectorBehaviour
    {
        [DI(DIScope.component, DIComponent.place)] XrSettings XrSettings { get; }

        [SerializeField] private RawImage categoryIcon;
        [SerializeField] private TextMeshProUGUI categoryText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI floorText;
        [SerializeField] private Button button;
        
        public void Config(PoiPromise poi, UnityAction<PoiPromise> clickAction, bool isRemoveAll = false)
        {
            if (TryGetComponent<Canvas>(out var canvas))
            {
                canvas.worldCamera = XrSettings.XrCamera.GetComponent<Camera>();
            }

            if (!string.IsNullOrEmpty(poi.CategoryIcon))
            {
                Davinci.get().load(poi.CategoryIcon).into(categoryIcon).start();
            }

            categoryText.SetText(poi.CategoryType);
            nameText.SetText(poi.PoiName);
            floorText.SetText(poi.Floor);

            if (isRemoveAll) button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => clickAction?.Invoke(poi));
        }
    }
}

