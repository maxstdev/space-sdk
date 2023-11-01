using Retrofit;
using Retrofit.Parameters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MaxstXR.Place
{
	public class PublicAPIService : IPublicAPIServiceability
	{
		private readonly RetrofitAdapter adapter;

		public PublicAPIService(RetrofitAdapter adapter)
		{
			this.adapter = adapter;
		}

		public IObservable<Place> GetPlaceFromId([Path("place_id")] long placeId)
		{
			var invocation = new NetworkInvocation(MethodBase.GetCurrentMethod() as MethodInfo, new object[] { placeId });
			adapter.Intercept(invocation);
			return invocation.ReturnValue as IObservable<Place>;
		}

		public IObservable<List<Spot>> ReqSpotList([Path("place_id")] long placeId)
		{
			var invocation = new NetworkInvocation(MethodBase.GetCurrentMethod() as MethodInfo, new object[] { placeId });
			adapter.Intercept(invocation);
			return invocation.ReturnValue as IObservable<List<Spot>>;
		}
	}
}