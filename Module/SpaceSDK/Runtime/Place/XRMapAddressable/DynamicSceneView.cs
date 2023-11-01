using Cysharp.Threading.Tasks;
using MaxstUtils;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MaxstXR.Place
{
    public enum ProgressType
    {
        Place = 0,
        Spot,
        Poi,
    }

    public interface IDynamicSceneView
    {
        GameObject ShowProgress(int progressType);
        GameObject ShowBackgoundProgress(int progressType);
        GameObject ShowPlaceList(List<Place> places);
        UniTask<Place> SelectPlaceAsync(PlaceList placeList, List<Place> places);
        GameObject ShowSpotList(List<Spot> spots);
        UniTask<Spot> SelectSpotAsync(SpotList spotList, List<Spot> places);
    }

    public class DynamicSceneView : InSceneUniqueBehaviour, IDynamicSceneView
    {
        public static DynamicSceneView Instance(GameObject go) => Instance<DynamicSceneView>(go);

        [SerializeField] private GameObject progressPrefeb;
        [SerializeField] private GameObject placesPrefeb;
        [SerializeField] private GameObject spotsPrefeb;

        public virtual GameObject ShowProgress(int progressType)
        {
            var go = Instantiate(progressPrefeb, GetComponentInParent<Canvas>().transform);
            //go.GetComponentInChildren<Text>().text = string.Empty;
            return go;
        }

        public virtual GameObject ShowBackgoundProgress(int progressType)
        {
            var go = Instantiate(progressPrefeb, GetComponentInParent<Canvas>().transform);
            //go.GetComponentInChildren<Text>().text = string.Empty;
            return go;
        }

        public virtual GameObject ShowPlaceList(List<Place> places)
        {
            var go = Instantiate(placesPrefeb, GetComponentInParent<Canvas>().transform);
            return go;
        }

        public virtual async UniTask<Place> SelectPlaceAsync(PlaceList placeList, List<Place> places)
        {
            var completionSource = new TaskCompletionSource<Place>();

            placeList.Config(places, p =>
            {
                completionSource.TrySetResult(p);
            });

            return await completionSource.Task;
        }

        public virtual GameObject ShowSpotList(List<Spot> spots)
        {
            var go = Instantiate(spotsPrefeb, GetComponentInParent<Canvas>().transform);
            return go;
        }

        public virtual async UniTask<Spot> SelectSpotAsync(SpotList spotList, List<Spot> spots)
        {
            var completionSource = new TaskCompletionSource<Spot>();

            spotList.Config(spots, p =>
            {
                completionSource.TrySetResult(p);
            });

            return await completionSource.Task;
        }
    }
}
