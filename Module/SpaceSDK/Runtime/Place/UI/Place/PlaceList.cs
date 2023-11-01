using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MaxstXR.Place
{
    public class PlaceList : MonoBehaviour
    {
        [SerializeField] protected GameObject contentObject;
        [SerializeField] protected GameObject itemPrefeb;

        public virtual void Config(List<Place> places, UnityAction<Place> placeClickAction)
        {
            contentObject.transform.DestroyAllChildren();

            foreach (var place in places)
            {
                var pi = Instantiate(itemPrefeb, contentObject.transform).GetComponent<PlaceItem>();
                pi.Config(place, placeClickAction, true);
            }
        }
    }
}
