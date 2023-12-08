using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MaxstXR.Place
{
    public class SpaceList : MonoBehaviour
    {
        [SerializeField] protected GameObject contentObject;
        [SerializeField] protected GameObject itemPrefeb;

        public virtual void Config(List<Space> spaces, UnityAction<Space> spaceClickAction)
        {
            contentObject.transform.DestroyAllChildren();

            foreach (var space in spaces)
            {
                var pi = Instantiate(itemPrefeb, contentObject.transform).GetComponent<SpaceItem>();
                pi.Config(space, spaceClickAction, true);
            }
        }
    }
}
