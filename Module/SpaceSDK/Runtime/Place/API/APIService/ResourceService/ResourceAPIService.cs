using Retrofit.Parameters;
using Retrofit;
using System;
using System.Reflection;

namespace MaxstXR.Place
{
    public class ResourceAPIService : IResourceAPIServiceability
    {
        private readonly RetrofitAdapter adapter;

        public ResourceAPIService(RetrofitAdapter adapter)
        {
            this.adapter = adapter;
        }

        public IObservable<MapSpot> GetMapSpots([Header("Authorization")] string accessToken, [Query("spaceId")] string spaceId)
        {
            var invocation = new NetworkInvocation(MethodBase.GetCurrentMethod() as MethodInfo, new object[] { accessToken, spaceId });
            adapter.Intercept(invocation);
            return invocation.ReturnValue as IObservable<MapSpot>;
        }

        public IObservable<MapSpot> LegacyGetMapSpots([Header("Authorization")] string accessToken, [Query("placeId")] long placeId)
        {
            var invocation = new NetworkInvocation(MethodBase.GetCurrentMethod() as MethodInfo, new object[] { accessToken, placeId });
            adapter.Intercept(invocation);
            return invocation.ReturnValue as IObservable<MapSpot>;
        }
    }
}
