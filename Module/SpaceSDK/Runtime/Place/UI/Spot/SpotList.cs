using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MaxstXR.Place
{
    public class SpotList : MonoBehaviour
    {
        [SerializeField] protected GameObject contentObject;
        [SerializeField] protected GameObject itemPrefeb;

        public virtual void Config(List<Spot> spots, UnityAction<Spot> placeClickAction)
        {
            contentObject.transform.DestroyAllChildren();

            foreach (var spot in spots)
            {
                var si = Instantiate(itemPrefeb, contentObject.transform).GetComponent<SpotItem>();
                si.Config(spot, placeClickAction, true);
            }
        }
    }
}
