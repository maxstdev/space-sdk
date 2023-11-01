using Cysharp.Threading.Tasks;
using Maxst;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MaxstXR.Place
{
#if false
	public class CustomerProcedure : InjectorBehaviour
	{
		[DI(DIScope.singleton)] private CustomerRepo CustomerRepo { get; }
		[DI(DIScope.singleton)] private MinimapPoiEvent MinimapPoiEvent { get; }
		[DI(DIScope.singleton)] private MinimapUserEvent MinimapUserEvent { get; }
		[DI(DIScope.singleton)] private MinimapViewModel MinimapViewModel { get; }
		[DI(DIScope.singleton)] private DynamicSceneViewModel DynamicSceneViewModel { get; }
		[DI(DIScope.singleton)] private PoiEvent PoiEvent { get; }
		[DI(DIScope.singleton)] private XrConfig XrConfig { get; }
		[DI(DIScope.singleton)] private PovEvent PovEvent { get; }

		//[SerializeField] private bool usePlaceConfig = true;
		//[SerializeField] private bool useSpotConfig = true;
		//[SerializeField] private bool useXRDataConfig = true;
		[SerializeField] private bool useUpdateXRPoi = true;
		[SerializeField] private bool useUpdateMinimapPoi = true;
		[SerializeField] private bool useUpdatePlace = false;

		[SerializeField] private MinimapScriptableObjects minimapScriptableObjects;
		[SerializeField] private PlacePrefabScriptableObjects placePrefabScriptableObjects;
		[SerializeField] private SceneScriptableObjects sceneScriptableObjects;
		[SerializeField] private CommonResourceSO commonResourceSO;
		[SerializeField] private ModelObjectRegistry modelObjectRegistry;

		private GameObject minimapContent;
		private GameObject xrContent;
		private Canvas rootCanvas;

		private bool isOrbisScene;
		private bool isReceivedFetchPoi = false;
		private GameObject progress;

		private void OnMultiPlayServerPath(string serverPath)
		{
			var currentSceneIndex = SceneManager.GetSceneAt(SceneManager.sceneCount - 1).buildIndex;
			var sceneType = sceneScriptableObjects.FindSceneType(currentSceneIndex);
			switch (sceneType)
			{
				case SceneType.ARScene:
					//case SceneType.ar_maxst_2f_orbis_demo:
					//case SceneType.ar_coex_b1_orbis_demo:
					isOrbisScene = true;
					PlayerInfo.Playmode = PlayerInfo.ModeEnum.AR;
					break;
				case SceneType.VRScene:
					//case SceneType.vr_maxst_2f_orbis_demo:
					//case SceneType.vr_coex_b1_orbis_demo:
					isOrbisScene = true;
					PlayerInfo.Playmode = PlayerInfo.ModeEnum.VR;
					break;
				case SceneType.WebVRScene:
				case SceneType.WebViewerScene:
					isOrbisScene = true;
					PlayerInfo.Playmode = PlayerInfo.ModeEnum.PC_VR;
					break;
				default:
					isOrbisScene = false;
					break;
			}
			//Debug.Log($"JJUN OnMultiPlayServerPath {isOrbisScene}/{serverPath}");
			XrConfig.IsMultiplay.Post(isOrbisScene, serverPath);
		}

		private void OnEnable()
		{
			CustomerRepo.placeListEvent.AddObserver(this, OnPlaceListEvent);
			CustomerRepo.spotListEvent.AddObserver(this, OnSpotListEvent);
			CustomerRepo.poiListEvent.AddObserver(this, OnPoiListEvent);
			CustomerRepo.categoryListEvent.AddObserver(this, OnCategoryListEvent);
			MinimapViewModel.ChangeNavigationLocation.AddObserver(this, OnChangeNavigationLocation);
			DynamicSceneViewModel.MultiPlayServerPath.AddObserver(this, OnMultiPlayServerPath);
			DynamicSceneViewModel.ReceiveSpotController.AddObserver(this, OnReceiveSpotController);
			InitalizePlaceData().Forget();
		}

		private async UniTask InitalizePlaceData()
		{
			//progress = commonResourceSO.Progress(FindObjectOfType<Canvas>());
			await UniTask.Delay(TimeSpan.FromSeconds(1));

			InitVarient();
			FetchData();
			//if (progress != null) GameObject.Destroy(progress);
		}

		private void OnDisable()
		{
			CustomerRepo.placeListEvent.RemoveAllObserver(this);
			CustomerRepo.spotListEvent.RemoveAllObserver(this);
			CustomerRepo.poiListEvent.RemoveAllObserver(this);
			CustomerRepo.categoryListEvent.RemoveAllObserver(this);
			MinimapViewModel.ChangeNavigationLocation.RemoveAllObserver(this);
			DynamicSceneViewModel.MultiPlayServerPath.RemoveAllObserver(this);
			DynamicSceneViewModel.ReceiveSpotController.RemoveAllObserver(this);
		}

		private void OnDestroy()
		{
			XrConfig.IsMultiplay.DirectInvoke(false, "");
			CustomerRepo.currentPlace.Post(null);
			CustomerRepo.currentSpot.Post(null);
		}

		private void InitVarient()
		{
			if (rootCanvas != null) return;

			var ds = FindObjectOfType<DynamicSceneManager>();
			if (ds != null)
			{
				minimapContent = ds.MinimapContent;
				xrContent = ds.XRContent;
				rootCanvas = ds.RootCanvas;
			}
			else
			{
				rootCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>() ?? null;
			}
		}

		private void FetchData()
		{
			if (string.IsNullOrEmpty(PlayerInfo.PlaceName)) return;

			var place = CustomerRepo.FindPlaceFromName(PlayerInfo.PlaceName);
			if (place == null)
			{
				UpdatePlaceList();
				CustomerRepo.currentPlace.Post(null);
				CustomerRepo.currentSpot.Post(null);
				return;
			}
			else
			{
				CustomerRepo.currentPlace.Post(place);
				var placeUniqueName = (PlaceUniqueName)System.Enum.Parse(typeof(PlaceUniqueName), place.placeUniqueName);
				DynamicSceneViewModel.UpdatePlace.Post(placeUniqueName);
			}

			if (string.IsNullOrEmpty(PlayerInfo.SpotName)) return;
			var spot = CustomerRepo.FindSpotFromName(PlayerInfo.SpotName);
			if (spot == null)
			{
				UpdateSpotList();
				CustomerRepo.currentSpot.Post(null);
				return;
			}
			else
			{
				CustomerRepo.currentSpot.Post(spot);
			}
			UpdateCategoryList();
		}

		private void UpdatePlaceList()
		{
			Debug.Log("UpdatePlaceList FindPlaceFromName start ");
			InitVarient();
			var progress = commonResourceSO.LoadingStatus(rootCanvas, LoadingStatus.UPDATE_PLACE);
			RxJob.Build()
				.ToObservable()
				.Subscribe(
				job =>
				{
					CustomerRepo.FetchPlaceListAll(job);
				},
				error =>
				{
					Debug.Log($"UpdatePlaceList FindPlaceFromName error : {error}");
					var networkPopup = commonResourceSO.CustomPopup(transform);
					networkPopup.SetupMessage(string.Empty, Res.network_fail.Localize());
					networkPopup.AddAction(() =>
					{
						Destroy(networkPopup.gameObject);
						UpdatePlaceList();
					});
					if (progress != null) GameObject.Destroy(progress);
				},
				() =>
				{
					/*progress hide*/
					Debug.Log("UpdatePlaceList FindPlaceFromName complte ");
					if (progress != null) GameObject.Destroy(progress);
				});
		}

		private void UpdateSpotList()
		{
			if (CustomerRepo.currentPlace.Value == null) return;
			Debug.Log("UpdateSpotList FetchSpotList start ");
			InitVarient();
			var progress = commonResourceSO.LoadingStatus(rootCanvas, LoadingStatus.UPDATE_PLACE);
			RxJob.Build()
				.ToObservable()
				.Subscribe(
				job =>
				{
					CustomerRepo.FetchSpotList(CustomerRepo.currentPlace.Value.placeId, job);
				},
				error =>
				{
					Debug.Log($"UpdateSpotList FetchSpotList error : {error}");
					var networkPopup = commonResourceSO.CustomPopup(transform);
					networkPopup.SetupMessage(string.Empty, Res.network_fail.Localize());
					networkPopup.AddAction(() =>
					{
						Destroy(networkPopup.gameObject);
						UpdateSpotList();
					});

					if (progress != null) GameObject.Destroy(progress);
				},
				() =>
				{
					Debug.Log("UpdateSpotList FetchSpotList complte ");
					if (progress != null) GameObject.Destroy(progress);
				});
		}

		private void UpdateCategoryList()
		{
			InitVarient();
			var progress = commonResourceSO.LoadingStatus(rootCanvas, LoadingStatus.UPDATE_CATEGORY);
			RxJob.Build()
				.ToObservable()
				.Subscribe(
				job =>
				{
					CustomerRepo.FetchCategoryList(job);
				},
				error =>
				{
					Debug.Log($"UpdateCategoryList FetchCategoryList error : {error}");
					var networkPopup = commonResourceSO.CustomPopup(transform);
					networkPopup.SetupMessage(string.Empty, Res.network_fail.Localize());
					networkPopup.AddAction(() =>
					{
						Destroy(networkPopup.gameObject);
						UpdateCategoryList();
					});
					if (progress != null) GameObject.Destroy(progress);
				},
				() =>
				{
					Debug.Log("UpdateCategoryList FetchCategoryList complte ");
					if (progress != null) GameObject.Destroy(progress);
				});
		}

		private void UpdatePoiListFormPlace()
		{
			progress = commonResourceSO.LoadingStatus(rootCanvas, LoadingStatus.UPDATE_POILIST);
			InitVarient();

			RxJob.Build()
				.ToObservable()
				.Subscribe(
				job =>
				{
					CustomerRepo.FetchPoiListFormPlace(CustomerRepo.currentPlace.Value.placeId, job);
					isReceivedFetchPoi = true;
				},
				error =>
				{
					Debug.Log($"UpdatePoiListFormPlace FetchPoiListFormPlace error : {error}");
					var networkPopup = commonResourceSO.CustomPopup(transform);
					networkPopup.SetupMessage(string.Empty, Res.network_fail.Localize());
					networkPopup.AddAction(() =>
					{
						Destroy(networkPopup.gameObject);
						UpdateCategoryList();
					});
				},
				() =>
				{
					Debug.Log($"UpdatePoiListFormPlace FetchPoiListFormPlace complete");
				});
		}

		private void OnPlaceListEvent(List<Place> data)
		{
			if (!CustomerRepo.placeListEvent.IsNew) return;
			if (data != null)
			{

				CustomerRepo.currentPlace.DirectInvoke(data.Find((place) => { return PlayerInfo.PlaceName == place.placeUniqueName; }));
				if (CustomerRepo.currentPlace.Value != null)
				{
					UpdateSpotList();
					if (useUpdatePlace)
					{
						var placeUniqueName = (PlaceUniqueName)System.Enum.Parse(typeof(PlaceUniqueName),
							CustomerRepo.currentPlace.Value.placeUniqueName);
						DynamicSceneViewModel.UpdatePlace.Post(placeUniqueName);
					}
				}
				else
				{
					Debug.LogWarning("place data not found !!!");
				}
			}
		}

		private void OnSpotListEvent(List<Spot> data)
		{
			if (!CustomerRepo.spotListEvent.IsNew) return;
			if (data != null)
			{
				CustomerRepo.currentSpot.Post(data.Find((spot) => { return PlayerInfo.SpotName == spot.vpsSpotName; }));
				if (CustomerRepo.currentSpot.Value != null
					&& CustomerRepo.currentPlace.Value != null)
				{
					UpdateCategoryList();
				}
				else
				{
					Debug.LogWarning("spot data not found !!!");
				}
			}
		}

		private void OnPoiListEvent(List<Poi> data)
		{
			if (!CustomerRepo.poiListEvent.IsNew) return;
			if (data != null)
			{
				if (useUpdateXRPoi)
				{
					LoadPoi3DSign(complete: () =>
					{
						if (progress != null) GameObject.Destroy(progress);
					},
					data, MinimapViewModel.ChangeNavigationLocation.Second,
						MinimapViewModel.ChangeNavigationLocation.First,
						MinimapViewModel.ChangeNavigationLocation.Second);
				}

				if (useUpdateMinimapPoi)
				{
					//In order to prevent duplicate creation, the current type is also all removed.
					LoadMinimapPoi(data, MinimapViewModel.ChangeNavigationLocation.Second,
						MinimapViewModel.ChangeNavigationLocation.First,
						MinimapViewModel.ChangeNavigationLocation.Second);

					LoadLocalUser(minimapScriptableObjects, MinimapViewModel.ChangeNavigationLocation.Second,
						MinimapViewModel.ChangeNavigationLocation.First,
						MinimapViewModel.ChangeNavigationLocation.Second);
				}
			}
		}

		private void OnCategoryListEvent(List<FirstCategory> data)
		{
			if (!CustomerRepo.categoryListEvent.IsNew) return;
			if (data != null)
			{
				if (CustomerRepo.currentSpot.Value != null
					&& CustomerRepo.currentPlace.Value != null)
				{
					UpdatePoiListFormPlace();
				}
			}
		}

		private void LoadMinimapPoi(List<Poi> pois, string currentLocation, params string[] removeLocations)
		{
			//Debug.Log($"CustomerProcedure LoadMinimapPoi {currentLocation}");
			foreach (var removeLocation in removeLocations ?? new string[0])
			{
				if (!string.IsNullOrEmpty(removeLocation))
				{
					MinimapPoiEvent.removeAllPointType.Post(removeLocation, PointType.MINIMAP_POI_TYPE);
				}
			}

			if (string.IsNullOrEmpty(currentLocation)) return;

			var list = new List<IPoint>();
			foreach (var poi in pois)
			{
				if (poi.NavigationLocation() == currentLocation)
				{
					list.Add(new MinimapPoi(poi, minimapContent, minimapScriptableObjects));
				}
			}
			MinimapPoiEvent.receivePoints.Post(currentLocation, list);
		}

		public void LoadLocalUser(MinimapScriptableObjects minimapSO, string navigationLocation, params string[] removeLocations)
		{
			if (!isOrbisScene)
			{
				return;
			}

			foreach (var location in removeLocations ?? new string[0])
			{
				if (!string.IsNullOrEmpty(location))
				{
					MinimapUserEvent.RemoveUsers.Post(location, UserType.LocalUser);
				}
			}

			if (string.IsNullOrEmpty(navigationLocation)) return;

			var localUser = new LocalUser(navigationLocation, minimapContent, minimapSO);
			MinimapUserEvent.OnCreateLocalUser.Post(navigationLocation, localUser);
		}

		public void OnReceiveSpotController(SpotController povController)
		{
			PovEvent.removeAllPointType.Post(PointType.POV_TYPE);

			var list = new List<IPoint>();
			foreach (Transform childTransform in povController.transform)
			{
				list.Add(new PovHandler(childTransform.gameObject));
			}
			PovEvent.receivePoints.Post(list);
		}


		private void OnChangeNavigationLocation(string prev, string current)
		{
			Debug.Log($"OnChangeNavigationLocation prev : {prev}, current : {current} / {MinimapViewModel.ChangeNavigationLocation.IsNew}");
			if (!MinimapViewModel.ChangeNavigationLocation.IsNew) return;

			if (minimapContent == null || xrContent == null) return;

			if (!string.IsNullOrEmpty(current))
			{
				var spot = CustomerRepo.FindSpotFromName(current);
				PlayerInfo.SpotName = current;
				if (spot == null)
				{
					UpdateSpotList();
					CustomerRepo.currentSpot.Post(null);
					return;
				}
				else
				{
					CustomerRepo.currentSpot.Post(spot);
				}
			}

			if (CustomerRepo.poiListEvent.Value != null && CustomerRepo.poiListEvent.Value.Count > 0)
			{
				if (useUpdateXRPoi)
				{
					LoadPoi3DSign(null, CustomerRepo.poiListEvent.Value, current, prev);
				}
				if (useUpdateMinimapPoi)
				{
					LoadMinimapPoi(CustomerRepo.poiListEvent.Value, current, prev);
					LoadLocalUser(minimapScriptableObjects, current, prev);
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(current) && !isReceivedFetchPoi)
				{
					UpdateCategoryList();
				}
			}
		}

		private void LoadPoi3DSign(Action complete, List<Poi> pois, string currentLocation, params string[] removeLocations)
		{
			foreach (var removeLocation in removeLocations ?? new string[0])
			{
				if (!string.IsNullOrEmpty(removeLocation))
				{
					PoiEvent.OnRemoveAllPointType.Post(removeLocation, PointType.SIGN_3D_TYPE);
				}
			}

			if (string.IsNullOrEmpty(currentLocation)) return;

			var list = new List<IPoint>();
			var poiGroupDic = new Dictionary<Vector3, List<PoiPromise>>();
			foreach (var poi in pois)
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
			foreach (var entry in poiGroupDic)
			{
				list.Add(new Poi3DSign(entry.Value, xrContent, placePrefabScriptableObjects, modelObjectRegistry));
			}

			PoiEvent.OnReceivePoints.Post(currentLocation, list, complete);
		}
	}
#endif
}
