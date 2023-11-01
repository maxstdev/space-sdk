using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MaxstXR.Place
{
    public class MinimapRenderMeasure : InjectorBehaviour
    {
        [SerializeField] private GameObject rawImage;
        [SerializeField] private bool useRatio = false;
        [SerializeField] private Vector2 scale = Vector2.one;
        [SerializeField] private Vector2 maxSize = Vector2.one;

        private void OnEnable()
        {
            StartCoroutine(MeasureRawImageAspect());
        }


        IEnumerator MeasureRawImageAspect()
        {
            yield return new WaitForEndOfFrame();
            if (useRatio)
            {
                var rect = gameObject.GetComponent<RectTransform>().rect;
                UpdateRawImageHeight(rect.width, rect.height, rawImage, scale.x, scale.y);
            }
            else
            {
                UpdateRawImageHeight(maxSize.x, maxSize.y, rawImage);
            }
        }

        private void UpdateRawImageHeight(float minWidth, float currentHeight, GameObject rawImage,
            float scaleX = 1F, float scaleY = 1F)
        {
            var rectTransform = rawImage.transform.GetComponent<RectTransform>();
            var t = rawImage.GetComponent<RawImage>().texture;
            var tw = (float)t.width;
            var th = (float)t.height;
            var ratio = tw / th;

            var expectWidth = currentHeight * ratio;
            var expectHeight = currentHeight;
            //Debug.Log($"UpdateRawImageHeight {minWidth}/{currentHeight}");
            if (expectWidth < minWidth)
            {
                expectHeight = minWidth * th / tw;
                expectWidth = expectHeight * ratio;
            }
            //Debug.Log($"UpdateRawImageHeight expect {expectWidth}/{ expectHeight}");

            rectTransform.sizeDelta = new Vector2(expectWidth * scaleX, expectHeight * scaleY);
        }
    }
}
