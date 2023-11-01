using Retrofit.Methods;
using Retrofit.Parameters;
using System;

namespace MaxstXR.Place
{
    public interface IResourceAPIServiceability 
    {
        [Get("/mapspots")]
        IObservable<MapSpot> GetMapSpots(
            [Header(ApiConst.Authorization)] string accessToken,
            [Query("placeId")] long placeId);
    }
}