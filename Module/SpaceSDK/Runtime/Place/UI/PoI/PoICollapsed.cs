using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MaxstXR.Place
{
	public class PoICollapsed : InjectorBehaviour
	{
		[SerializeField] private TextMeshProUGUI poiCount;
		[SerializeField] private Button button;

		public void Config(List<PoiPromise> poiList, UnityAction<List<PoiPromise>> clickAction, bool isRemoveAll = false)
		{
			poiCount.SetText(poiList.Count.ToString());
			if (isRemoveAll) button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => clickAction?.Invoke(poiList));
        }
	}
}
