using Cysharp.Threading.Tasks;
﻿using Castle.Core.Internal;
using maxstAR;
using MaxstUtils;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public Space CurrentSpace { private get; set; } = null;
        public Place CurrentPlace { private get; set; } = null;
        public Spot CurrentSpot { private get; set; } = null;

        public PlaceScriptableObjects PlaceScriptableObjects { get; set; } = null;

        protected SceneViewModel()
		{

		}

		~SceneViewModel()
		{

		}

        public async UniTask<string> GetSpaceAsync(IDynamicSceneView dsv, string id = "", bool isSelectFirst = false)
        {
            return VersionController.Instance.CurrentMode == VersionController.Mode.Modern
                ? await GetModernSpaceAsync(dsv, id, isSelectFirst)
                : await GetLegacyPlaceAsync(dsv, id, isSelectFirst);
        }

        private async UniTask SetCurrentSpace(IDynamicSceneView dsv, string id)
        {
            if (id.IsNullOrEmpty())
            {
                CurrentSpace = null;
            }
            else {
                var go = dsv.ShowProgress((int)ProgressType.Place);
                var space = await CustomerRepo.RequestSpace(id);
                CurrentSpace = space;
                GameObject.Destroy(go);
            }
        }

        private async UniTask<string> GetModernSpaceAsync(IDynamicSceneView dsv, string id, bool isSelectFirst)
        {
            if (false == string.IsNullOrEmpty(id))
            {
                await SetCurrentSpace(dsv, id);
                return CurrentSpace?.spaceId;
            }

            var spaces = VersionController.Instance.CurrentSpaceStep == SpaceStep.PUBLIC ?
                await CustomerRepo.FetchSpaceList() : await CustomerRepo.FetchSpaceListAll(VersionController.Instance.CurrentSpaceStep);
            if (spaces.Count == 1 || isSelectFirst)
            {
                await SetCurrentSpace(dsv, spaces[0].spaceId);
                return CurrentSpace.spaceId;
            }

            if (id != null && id.Length > 0)
            {
                CurrentSpace = spaces.Find(p => p.spaceId == id);
                await SetCurrentSpace(dsv, CurrentSpace.spaceId);
            }
            else
            {
                CurrentSpace = null;
            }

            if (CurrentSpace == null)
            {
                var listGo = dsv.ShowSpaceList(spaces);
                CurrentSpace = await dsv.SelectSpaceAsync(listGo.GetComponent<SpaceList>(), spaces);
                GameObject.Destroy(listGo);
                await SetCurrentSpace(dsv, CurrentSpace.spaceId);
            }
            return CurrentSpace?.spaceId;
        }

        private async UniTask<string> GetLegacyPlaceAsync(IDynamicSceneView dsv, string id, bool isSelectFirst)
        {
            var go = dsv.ShowProgress((int)ProgressType.Place);
            var places = await CustomerRepo.FetchPlaceList();
            GameObject.Destroy(go);

            if (places.Count == 1 || isSelectFirst)
            {
                CurrentPlace = places[0];
                await GetSpotAsync(dsv, places[0], null, true);
                return CurrentPlace.placeUniqueName;
            }

            if (id != null && id.Length > 0)
            {
                CurrentPlace = places.Find(p => p.placeUniqueName == id);
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
                await GetSpotAsync(dsv, CurrentPlace, null, true);
            }
            return CurrentPlace.placeUniqueName;
        }



        //      public async UniTask<Place> GetPlaceAsync(IDynamicSceneView dsv, long? placeId, bool isSelectFirst = false)
        //{
        //	//var go = dsv.ShowProgress((int)ProgressType.Place);
        //          var places = await CustomerRepo.FetchPlaceList();
        //         //GameObject.Destroy(go);

        //          if (places.Count == 1 || isSelectFirst)
        //          {
        //              CurrentPlace = places[0];
        //              return CurrentPlace;
        //          }

        //          if (placeId.HasValue)
        //	{
        //              CurrentPlace = places.Find(p => p.placeId == placeId.Value);
        //          }
        //          else
        //          {
        //              CurrentPlace = null;
        //          }

        //          if (CurrentPlace == null)
        //	{
        //		var listGo = dsv.ShowPlaceList(places);
        //              CurrentPlace = await dsv.SelectPlaceAsync(listGo.GetComponent<PlaceList>(), places);
        //              GameObject.Destroy(listGo);
        //          }
        //	return CurrentPlace;
        //}

        public async UniTask<string> GetSpotAsync(IDynamicSceneView dsv, Place place, long? spotId, bool isSelectFirst = false)
        {
            if (place == null) return null;
            var spots = await CustomerRepo.FetchSpotList(place.placeId);

            if (spots.Count == 1 || isSelectFirst)
            {
                CurrentSpot = spots[0];
                return CurrentSpot.vpsSpotName;
            }
            return null;
        }

        public async UniTask<List<FirstCategory>> GetCategoryAsync(IDynamicSceneView dsv)
        {
            //var go = dsv.ShowProgress((int)ProgressType.Poi);
            var categories = await CustomerRepo.FetchCategoryList();
            //GameObject.Destroy(go);
            return categories;
        }

        //public async UniTask<List<Poi>> GetPoIAsync(IDynamicSceneView dsv, Place place, List<Spot> spots)
        //{
        //    //var go = dsv.ShowProgress((int)ProgressType.Poi);
        //    var pois = await CustomerRepo.FetchPoiListFormPlace(place, spots);
        //    //GameObject.Destroy(go);
        //    return pois;
        //}

        public async UniTask<List<Poi>> GetSpacePoiAsync()
        {
            var pois = await CustomerRepo.FetchPoiListFormSpace(CurrentUniqueNameKey());

            return pois;
        }

        public string CurrentMapKey()
        {
            return VersionController.Instance.CurrentMode == VersionController.Mode.Modern
                    ? CurrentSpace?.SpaceId.ToString()
                    : CurrentSpot?.vpsSpotName;
        }

        public string CurrentBundleKey()
        {
            return VersionController.Instance.CurrentMode == VersionController.Mode.Modern
                    ? CurrentSpace?.SpaceId.ToString()
                    : CurrentPlace?.PlaceId.ToString();
        }

        public string CurrentTextureKey()
        {
            return VersionController.Instance.CurrentMode == VersionController.Mode.Modern
                    ? CurrentSpace?.SpaceId.ToString()
                    : CurrentSpot?.id.ToString();
        }

        public string CurrentNameKey()
        {
            return VersionController.Instance.CurrentMode == VersionController.Mode.Modern
                    ? CurrentSpace?.SpaceName
                    : CurrentPlace.placeName.ko;
        }

        public string CurrentUniqueNameKey()
        {
            return VersionController.Instance.CurrentMode == VersionController.Mode.Modern
                    ? CurrentSpace?.SpaceId
                    : CurrentPlace.placeUniqueName;
        }

        public List<float> CurrentInitialPoint()
        {
            return VersionController.Instance.CurrentMode == VersionController.Mode.Modern
                    ? CurrentSpace?.initialPoint?.coordinates?.Pos[0][0]
                    : CurrentPlace?.centralPoint?.coordinates?.Pos[0][0];
        }
    }
}
