using Maxst.Passport;
using Retrofit.Methods;
using Retrofit.Parameters;
using System;
using System.Collections.Generic;

namespace MaxstXR.Place
{
    public static class ApiConst
	{
		public const string Authorization = "Authorization";
	}

    public interface ICustomerPoiApi
	{
		[Get("/v1/api/place/list")]
        IObservable<List<Place>> ReqPlaceList(
            [Header(ApiConst.Authorization)] string accessToken
            );

		[Get("/v1/api/place/list/all")]
		IObservable<List<Place>> ReqPlaceListAll(
			[Header(ApiConst.Authorization)] string accessToken);

		[Get("/v1/api/place/{place_id}")]
        IObservable<PlaceDetail> ReqPlaceDetail(
			[Header(ApiConst.Authorization)] string accessToken,
			[Path("place_id")] long placeId);

        //spot
        [Get("/v1/api/spot/list/{place_id}")]
        IObservable<List<Spot>> ReqSpotList(
			[Header(ApiConst.Authorization)] string accessToken,
			[Path("place_id")] long placeId);

        [Get("/api/spot/{spot_id}")]
        IObservable<SpotDetail> ReqSpotDetail (
			[Header(ApiConst.Authorization)] string accessToken,
			[Path("spot_id")] long spotId);

        //poi
        [Get("/v1/poi/place/{place_id}")]
        IObservable<List<Poi>> ReqPoiListFormPlace(
			[Header(ApiConst.Authorization)] string accessToken,
			[Path("place_id")] long placeId);

        /*
        [Get("/api/spot/{spot_id}")]
        IObservable<List<Poi>> ReqPoiListFormSpot(
            [Header("Authorization")] string accessToken,
            [Path("spot_id")] long spotId);
        */
        [Get("/v1/poi/{uuid}")]
        IObservable<PoiDetail> ReqPoiDetail(
			[Header(ApiConst.Authorization)] string accessToken,
			[Path("uuid")] string uuid);

        //category
        [Get("/v1/api/category/list")]
        IObservable<List<FirstCategory>> ReqCategoryList(
			[Header(ApiConst.Authorization)] string accessToken
			);

        [Get("/v1/api/category/{category_id}")]
        IObservable<FirstCategory> ReqCategoryDetail(
			[Header(ApiConst.Authorization)] string accessToken,
			[Path("category_id")] long categoryId);

        [Get("/v1/api/category/joint_type")]
        IObservable<List<JointType>> ReqCategoryJointType(
			[Header(ApiConst.Authorization)] string accessToken
			);

        [Get("/v1/api/category/augment_type")]
        IObservable<List<AugmentType>> ReqCategoryAugmentType(
			[Header(ApiConst.Authorization)] string accessToken
			);
    }
}

