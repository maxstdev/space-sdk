#define API_DETAIL_DEBUG
using Cysharp.Threading.Tasks;
using Maxst.Passport;
using Maxst.Settings;
using maxstAR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace MaxstXR.Place
{
	public partial class CustomerRepo : Injector
	{
		public List<Place> CachePlaceList { get; private set; }
		public List<Spot> CacheSpotList { get; private set; }
		public List<FirstCategory> CacheCategoryList { get; private set; }
		public List<Poi> CachePoIList { get; private set; }

		private Place cacheCategoryPlace;
		private Spot cacheCategorySpot;
		private BaseCategory cacheCategory;


        protected CustomerRepo()
		{

		}

        private async Task<ClientToken> GetClientToken() {
            var auth = TokenRepo.Instance.passportConfig;

            await TokenRepo.Instance.GetPassportClientToken(
                auth.ApplicationId,
                auth.ApplicationKey,
                auth.GrantType,
                null,
                null,
                false
            );

            return TokenRepo.Instance.GetClientToken();
        }

		public async UniTask<List<Place>> FetchPlaceList(List<string> whiteList = null)
		{
			var completionSource = new TaskCompletionSource<List<Place>>();

            var token = await GetClientToken();

            var ob = CustomerService.Instance.ReqPlaceList(token.access_token);
            ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>   // on success
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"ReqPlaceList Received success data count : {data?.Count ?? -1}");
#endif
                    if (whiteList != null)
                    {
                        data = data.Where(place => whiteList.Contains(place.placeUniqueName)).ToList();
                    }

                    CachePlaceList = data;
                    completionSource.TrySetResult(data);
                    cacheCategoryPlace = null;
                },
                error => // on error
                {
                    Debug.LogWarning(error);
                    completionSource.TrySetException(error);

                },
                () =>
                {

                });

            return await completionSource.Task;
		}

		public async UniTask<List<Place>> FetchPlaceListAll()
		{
			var completionSource = new TaskCompletionSource<List<Place>>();

            var token = await GetClientToken();

            var ob = CustomerService.Instance.ReqPlaceListAll(token.access_token);
            ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>   // on success
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"ReqPlaceListAll Received success data count : {data?.Count ?? -1}");
#endif
                    CachePlaceList = data;
                    completionSource.TrySetResult(data);
                    cacheCategoryPlace = null;
                },
                error => // on error
                {
                    Debug.LogWarning(error);
                    completionSource.TrySetException(error);

                },
                () =>
                {

                });

			return await completionSource.Task;
		}

#if false
		public void FetchFilterdPlace(RxJob job)
		{
			var ob = ExternalResService.Instance.PlaceWhiteList();
			ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
				.ObserveOn(Scheduler.MainThread)
				.Subscribe(data =>
				{
					FetchPlaceList(job, data);
				},
				error =>
				{
					Debug.Log($"PlaceWhiteList error : {error}");
					if (job != null)
					{
						job.Exception = error;
						job.IsDone = true;
					}
				},
				() =>
				{
					Debug.Log("PlaceWhiteList complete");
					if (job != null) job.IsDone = true;
				});
		}
#endif

		public async UniTask<PlaceDetail> FetchPlaceDatail(long placeId)
		{
			var completionSource = new TaskCompletionSource<PlaceDetail>();

            var token = await GetClientToken();

            var ob = CustomerService.Instance.ReqPlaceDetail(token.access_token, placeId);
            ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>   // on success
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"ReqPlaceDetail Received success data : {data.createdAt}");
#endif
                    completionSource.TrySetResult(data);
                },
                error => // on error
                {
                    Debug.LogWarning(error);
                    completionSource.TrySetException(error);

                },
                () =>
                {

                });

			return await completionSource.Task;
		}

		public async UniTask<List<Spot>> FetchSpotList(long placeId)
		{
            var completionSource = new TaskCompletionSource<List<Spot>>();

            var token = await GetClientToken();

            var ob = CustomerService.Instance.ReqSpotList(token.access_token, placeId);
            ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>   // on success
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"ReqSpotList({placeId}) Received success data count : {data.Count}");
#endif
                    CacheSpotList = data;
                    completionSource.TrySetResult(data);
                    cacheCategorySpot = null;
                },
                error => // on error
                {
                    Debug.LogWarning(error);
                    completionSource.TrySetException(error);

                },
                () =>
                {

                });

            return await completionSource.Task;
		}

		public async UniTask<SpotDetail> FetchSpotDatail(long spotId)
		{
			var completionSource = new TaskCompletionSource<SpotDetail>();

            var token = await GetClientToken();

            var ob = CustomerService.Instance.ReqSpotDetail(token.access_token, spotId);
            ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>   // on success
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"ReqSpotDetail({spotId}) Received success data : {data.createdAt}");
#endif
                    completionSource.TrySetResult(data);
                },
                error => // on error
                {
                    Debug.LogWarning(error);
                    completionSource.TrySetException(error);

                },
                () =>
                {

                });

			return await completionSource.Task;
		}

		public async UniTask<List<Poi>> FetchPoiListFormPlace(Place place, List<Spot> spots)
		{
			Debug.Log($"FetchPoiListFormPlace {place.placeId}");
			var completionSource = new TaskCompletionSource<List<Poi>>();

            var token = await GetClientToken();

            var ob = CustomerService.Instance.ReqPoiListFormPlace(token.access_token, place.placeId);
            ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>   // on success
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"ReqPoiListFormPlace({place.placeId}) Received success data count : {data.Count}");
#endif
                    var categoryFilterList = new HashSet<BaseCategory>();
                    foreach (var d in data)
                    {
                        d.refPlace = place;
                        d.refSpot = FindSpot(d.spotId, spots);
                        d.refCategory = FindCategory(d.category?.categoryId ?? 0L);
                        if (d.refCategory != null) categoryFilterList.Add(d.refCategory);
                    }
                    CachePoIList = data;
                    UpdateCategoryFilterList(categoryFilterList);
                    completionSource.TrySetResult(data);

                },
                error => // on error
                {
                    Debug.LogWarning(error);
                    completionSource.TrySetException(error);

                },
                () =>
                {

                });

            return await completionSource.Task;
		}

		public async UniTask<PoiDetail> FetchPoiDetail(string uuid)
		{
			var completionSource = new TaskCompletionSource<PoiDetail>();

            var token = await GetClientToken();

            var ob = CustomerService.Instance.ReqPoiDetail(token.access_token, uuid);
            ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>   // on success
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"ReqPoiDetail Received success data : {data.poiUuid}");
#endif
                    completionSource.TrySetResult(data);
                },
                error => // on error
                {
                    Debug.LogWarning(error);
                    completionSource.TrySetException(error);
                },
                () =>
                {

                });
            
			return await completionSource.Task;
		}
	}
}
