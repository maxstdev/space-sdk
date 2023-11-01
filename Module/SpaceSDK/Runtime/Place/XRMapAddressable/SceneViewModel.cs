using Cysharp.Threading.Tasks;
using maxstAR;
using MaxstUtils;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace MaxstXR.Place
{
    public class SceneViewModel : Injector
	{
		[DI(DIScope.component, DIComponent.place)] private CustomerRepo CustomerRepo { get; }

		public readonly LiveEvent PlaceLoadComplete = new();
		public readonly LiveEvent<Dictionary<string, Bounds>> TrackableBounds = new();

        public readonly ReactiveProperty<ARLocationRecognitionState> CurrentRecognitionState 
            = new(ARLocationRecognitionState.ARLocationRecognitionStateNotAvailable);

        public Place CurrentPlace { get; set; } = null;
        public Spot CurrentSpot { get; set; } = null;
        public PlaceScriptableObjects PlaceScriptableObjects { get; set; } = null;

        protected SceneViewModel()
		{

		}

		~SceneViewModel()
		{

		}

		public async UniTask<Place> GetPlaceAsync(IDynamicSceneView dsv, long? placeId, bool isSelectFirst = false)
		{
			//var go = dsv.ShowProgress((int)ProgressType.Place);
            var places = await CustomerRepo.FetchPlaceList();
           //GameObject.Destroy(go);

            if (places.Count == 1 || isSelectFirst)
            {
                CurrentPlace = places[0];
                return CurrentPlace;
            }

            if (placeId.HasValue)
			{
                CurrentPlace = places.Find(p => p.placeId == placeId.Value);
            }
            else
            {
                CurrentPlace = null;
            }
			
            if (CurrentPlace == null)
			{
				var listGo = dsv.ShowPlaceList(places);
                CurrentPlace = await dsv.SelectPlaceAsync(listGo.GetComponent<PlaceList>(), places);
                GameObject.Destroy(listGo);
            }
			return CurrentPlace;
		}

        public async UniTask<Spot> GetSpotAsync(IDynamicSceneView dsv, Place place, long? spotId, bool isSelectFirst = false)
        {
            //var go = dsv.ShowProgress((int)ProgressType.Spot);
            var spots = await CustomerRepo.FetchSpotList(place.placeId);
            //GameObject.Destroy(go);

            if (spots.Count == 1 || isSelectFirst)
            {
                CurrentSpot = spots[0];
                return CurrentSpot;
            }

            if (spotId.HasValue)
            {
                CurrentSpot = spots.Find(s => s.id == spotId.Value);
            }
            else
            {
                CurrentSpot = null;
            }

            if (CurrentSpot == null)
            {
                var listGo = dsv.ShowSpotList(spots);
                CurrentSpot = await dsv.SelectSpotAsync(listGo.GetComponent<SpotList>(), spots);
                GameObject.Destroy(listGo);
            }
            return CurrentSpot;
        }

        public async UniTask<List<FirstCategory>> GetCategoryAsync(IDynamicSceneView dsv)
        {
            //var go = dsv.ShowProgress((int)ProgressType.Poi);
            var categories = await CustomerRepo.FetchCategoryList();
            //GameObject.Destroy(go);
            return categories;
        }

        public async UniTask<List<Poi>> GetPoIAsync(IDynamicSceneView dsv, Place place, List<Spot> spots)
        {
            //var go = dsv.ShowProgress((int)ProgressType.Poi);
            var pois = await CustomerRepo.FetchPoiListFormPlace(place, spots);
            //GameObject.Destroy(go);
            return pois;
        }
    }
}
