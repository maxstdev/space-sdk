using UnityEngine;
using UnityEngine.EventSystems;

namespace Maxst.Settings
{
    public class SettingMenuTrigger : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] GameObject hiddenMenuPrefeb;
        [SerializeField] int limitClickCount = 0;
		private int clickCount = 0;
		private float lastTimeClick = 0;

        private SettingMenu settingMenu = null;

        public void OnPointerClick(PointerEventData eventData)
        {
			float currentTimeClick = eventData.clickTime;
			if (Mathf.Abs(currentTimeClick - lastTimeClick) < 0.75f)
			{
				clickCount++;
			}
			else
			{
				clickCount = 1;
			}

			if (clickCount > limitClickCount)
            {
                ReleaseSettingMenu();
                var t = GetComponentInParent<Canvas>()?.transform;
                settingMenu = Instantiate(hiddenMenuPrefeb, t).GetComponent<SettingMenu>();
                settingMenu.OnCompleteAction = ReleaseSettingMenu;
				clickCount = 0;
			}
			lastTimeClick = currentTimeClick;
		}

        private void ReleaseSettingMenu()
        {
            if (settingMenu != null)
            {
                GameObject.Destroy(settingMenu.gameObject);
                settingMenu = null;
            }
        }
    }
}
