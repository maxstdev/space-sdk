using TMPro;
using UnityEngine;

namespace MaxstXR.Place
{
    public class MinimapPoiBehaviour : MonoBehaviour
    {
        [SerializeField] private TextMeshPro poiName;
        [SerializeField] private SpriteRenderer categorySpriteRenderer;
        [SerializeField] private GameObject icon;
        [SerializeField] private GameObject dest;

        public void UpdateContent(bool isDest, PoiPromise poi)
        {

            poiName.SetText(poi.PoiName);
            if (isDest)
            {
                poiName.color = PlaceResources.Instance(gameObject).DestColor;
                icon.SetActive(false);
                dest.SetActive(true);
                gameObject.SetActive(true);
            }
            else
            {
                poiName.color = PlaceResources.Instance(gameObject).NormalColor;
                icon.SetActive(true);
                dest.SetActive(false);

                if (string.IsNullOrEmpty(poi.CategoryIcon))
                {
                    categorySpriteRenderer.material = PlaceResources.Instance(gameObject).MinimapBillboardMaterial;
                }
                else
                {
                    Davinci.get().load(poi.CategoryIcon)
                        .into(categorySpriteRenderer)
                        .withLoadedAction(() => categorySpriteRenderer.material = PlaceResources.Instance(gameObject).MinimapBillboardMaterial)
                        .start();
                }
            }
        }

        private void UpdateSprite(Sprite sprite)
        {
            if (categorySpriteRenderer)
            {
                categorySpriteRenderer.sprite = sprite;
            }
        }
    }
}