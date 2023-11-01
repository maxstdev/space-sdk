using Retrofit;
using Retrofit.Parameters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MaxstXR.Place
{
    public class TopologyAPIService : ITopologyAPIServiceability
    {
        private readonly RetrofitAdapter adapter;

        public TopologyAPIService(RetrofitAdapter adapter)
        {
            this.adapter = adapter;
        }

        public IObservable<string> GetTopologyData([Header(ApiConst.Authorization)] string authorization, [Path("spot_id")] long spotId)
        {
            var invocation = new NetworkInvocation(MethodBase.GetCurrentMethod() as MethodInfo, new object[] { authorization, spotId });
            adapter.Intercept(invocation);
            return invocation.ReturnValue as IObservable<string>;
        }
    }
}