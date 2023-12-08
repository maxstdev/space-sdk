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

    public enum SpaceStep
    {
        CREATED_SPACE,
        CONFIRMED_MP4,
        UPLOADED_IMAGE360,
        GENERATING_RAW_PLY,
        RAW_PLY_FAIL,
        SET_SCALE,
        GENERATING_ALIGNED_PLY,
        ALIGNED_PLY_FAIL,
        COMPLETE_ALIGNED_PLY,
        UPLOADED_MESH,
        UPLOADED_SPATIAL_MAP,
        UPLOADED_BUNDLE_SET,
        FINISHED, 
        PRIVATE,
        PUBLIC
    }

    public interface ISpaceConsoleApi
	{
        [Get("/v1/spaces")]
        IObservable<SpaceDatas> ReqSpaceListAll(
            [Header(ApiConst.Authorization)] string accessToken,
            [Query("step")] SpaceStep step,
            [Query("page")] int page,            // def : 1
            [Query("size")] int size,            // def : 10
            [Query("sort_by")] string sortBy,    //do not test
            [Query("sort_direction")] string sortDirection);     // ASC, DESC

        //sort_by={sort_by}&
        [Get("/v1/spaces/public")]
        IObservable<SpaceDatas> ReqSpaceList(
            [Header(ApiConst.Authorization)] string accessToken,
            [Query("page")] int page,            // def : 1
            [Query("size")] int size,            // def : 10
            [Query("sort_by")] string sortBy,    //do not test
            [Query("sort_direction")] string sortDirection);     // ASC, DESC

        [Get("/v1/space/{space_id}")]
        IObservable<Space> ReqSpace(
            [Header(ApiConst.Authorization)] string accessToken,
            [Path("space_id")] string spaceId);

        [Get("/v1/space/{space_id}/texture")]
        IObservable<SpaceTextureUrl> GetImageUrl(
            [Header(ApiConst.Authorization)] string accessToken,
            [Path("space_id")] string spaceId);

        [Get("/v1/space/{space_id}/pois")]
        IObservable<List<Poi>> ReqPoiListFromSpace(
            [Header(ApiConst.Authorization)] string accessToken,
            [Path("space_id")] string spaceId);
    }
}

