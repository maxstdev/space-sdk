using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace MaxstXR.Place
{
    public class NavigationLocationObserver : InjectorBehaviour
    {
        [DI(DIScope.component, DIComponent.place)] private XrSettings XrSettings { get; }
        [DI(DIScope.component, DIComponent.place)] private SceneViewModel SceneViewModel { get; }
        [DI(DIScope.component, DIComponent.place)] private CustomerRepo CustomerRepo { get; }
        [DI(DIScope.component, DIComponent.place)] private PoIEvent PoIEvent { get; }
        [DI(DIScope.component, DIComponent.place)] private PovEvent PovEvent { get; }
        [DI(DIScope.component, DIComponent.minimap)] private MinimapPoiEvent MinimapPoiEvent { get; }

        [DisplayWithoutEdit, SerializeField] private DynamicSceneManager dynamicSceneManager;

        [field: SerializeField, DisplayWithoutEdit] public bool IsWorldContent { get; set; } = false;
        [field: SerializeField, DisplayWithoutEdit] public bool IsMinimapContent { get; set; } = false;

        private void Start()
        {
            //XrSettings.NavigationLocation.ObserveOnMainThread().Subscribe(OnChangedNavigationLocation).AddTo(this);
            XrSettings.SpotController.ObserveOnMainThread().Subscribe(OnChangedSpotController).AddTo(this);
            dynamicSceneManager = FindObjectOfType<DynamicSceneManager>();
        }

        private void OnChangedSpotController(SpotController povController)
        {
            if (povController == null) return;

            PovEvent.removeAllPointType.Post(PointType.POV_TYPE);

            var list = new List<IPoint>();
            foreach (Transform childTransform in povController.transform)
            {
                list.Add(new PovHandler(childTransform.gameObject));
            }
            PovEvent.receivePoints.Post(list);
        }

        private void OnChangedNavigationLocation(string navigationLocation)
        {
            //Debug.Log($"OnChangedNavigationLocation : {navigationLocation}");
            if (string.IsNullOrEmpty(navigationLocation)) { return; }
            Fetch(XrSettings.NavigationLocation.Value, XrSettings.OldNavigationLocation.Value);
        }

        private async void Fetch(string currentLocation, params string[] removeLocations)
        {
            if (CustomerRepo.CachePoIList.IsEmpty())
            {
                //await SceneViewModel.GetCategoryAsync(DynamicSceneView.Instance(gameObject));

                //await SceneViewModel.GetPoIAsync(DynamicSceneView.Instance(gameObject),
                //    SceneViewModel.CurrentPlace, CustomerRepo.CacheSpotList);
                await SceneViewModel.GetSpacePoiAsync();
            }
            Load(CustomerRepo.CachePoIList, currentLocation, removeLocations);
        }

        private void Load(List<Poi> pois, string currentLocation, params string[] removeLocations)
        {
            foreach (var removeLocation in removeLocations ?? new string[0])
            {
                if (!string.IsNullOrEmpty(removeLocation))
                {
                    if (IsWorldContent) PoIEvent.OnRemoveAllPointType.Post(removeLocation, PointType.SIGN_3D_TYPE);
                    if (IsMinimapContent) MinimapPoiEvent.OnRemoveAllPointType.Post(removeLocation, PointType.MINIMAP_POI_TYPE);
                }
            }

            if (string.IsNullOrEmpty(currentLocation)) return;

            if (IsWorldContent) { }
            var list = new List<IPoint>();
            var minimapList = new List<IPoint>();
            var poiGroupDic = new Dictionary<Vector3, List<PoiPromise>>();
            foreach (var poi in pois)
            {
                if (IsWorldContent) 
                {
                    if (poi.NavigationLocation() == currentLocation)
                    {
                        if (poiGroupDic.TryGetValue(poi.GetTruncateVpsPosition(), out var poiList))
                        {
                            poiList.Add(poi);
                        }
                        else
                        {
                            poiList = new List<PoiPromise>
                        {
                            poi
                        };
                            poiGroupDic.Add(poi.GetTruncateVpsPosition(), poiList);
                        }
                    }
                }

                if (IsMinimapContent)
                {
                    // don't add minimap poi list
                    //minimapList.Add(new MinimapPoi(poi, dynamicSceneManager.MinimapContent));
                }
            }
            if (IsWorldContent)
            {
                foreach (var entry in poiGroupDic)
                {
                    // don't add World map POI list
                    //list.Add(new PoIContent(entry.Value, dynamicSceneManager.WorldContent));
                }
            }

            if (IsWorldContent)  PoIEvent.OnReceivePoints.Post(currentLocation, list);
            if (IsMinimapContent) MinimapPoiEvent.OnReceivePoints.Post(currentLocation, minimapList);
        }
    }
}
