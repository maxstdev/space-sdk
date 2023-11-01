using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace MaxstXR.Place
{
	public class PoICardList : InjectorBehaviour
	{
		public enum Status
		{
			None, Collapsed, Expanded,
		}

		private const int MAX_VISIBLE_LIST_ITEM_CNT = 5;

		[DI(DIScope.component, DIComponent.place)] XrSettings XrSettings { get; }

		[SerializeField] private ScrollRect scrollRect;
		[SerializeField] private GameObject mainObject;
        [SerializeField] private GameObject contentObject;
        [SerializeField] private GameObject itemPrefeb;
        [SerializeField] private PoICollapsed poICollapsed;
		[SerializeField] private float itemHeight;

        [SerializeField, DisplayWithoutEdit] private Status status = Status.None;

		private List<PoiPromise> poiList;

		public void Config(List<PoiPromise> poiList)
		{
			ConfigEventCamera();
			this.poiList = poiList;
			ResizeByData();
            poICollapsed.Config(this.poiList, pois => { }, true);

			contentObject.transform.DestroyAllChildren();
            foreach (var p in poiList)
			{
				var go = GameObject.Instantiate(itemPrefeb, contentObject.transform);
				go.name = p.PoiName;
				go.GetComponent<PoICard>().Config(p, poi => { }, true);
			}
		}

		private void ConfigEventCamera()
		{
			if (TryGetComponent<Canvas>(out var canvas))
			{
				canvas.worldCamera = XrSettings.XrCamera.GetComponent<Camera>();
			}
		}

		private void ResizeByData()
		{
			var rectTransform = mainObject.GetComponent<RectTransform>();
			scrollRect.enabled = poiList.Count > MAX_VISIBLE_LIST_ITEM_CNT;
			rectTransform.sizeDelta = new(rectTransform.rect.width, VisibleDataHeight());
		}

		public void ExpendPoi()
		{
			if (status == Status.Expanded)
			{
				return;
			}
            poICollapsed.gameObject.SetActive(false);
			mainObject.SetActive(true);
			status = Status.Expanded;
		}

		public void CollapsePoi()
		{
			if (status == Status.Collapsed)
			{
				return;
			}

            poICollapsed.gameObject.SetActive(true);
            mainObject.SetActive(false);
			status = Status.Collapsed;
		}

		private float VisibleDataHeight()
		{
			return Mathf.Min(poiList.Count, MAX_VISIBLE_LIST_ITEM_CNT) * itemHeight;
		}
	}
}