using Retrofit.Methods;
using Retrofit.Parameters;
using System;
using System.Collections.Generic;

namespace MaxstXR.Place
{
    public interface ITopologyAPIServiceability
    {
        //[Get("/v1/spot-project/work-process/download-topology/{spot_id}")]
        [Get("/v1/space/{spot_id}/topology")]
        IObservable<string> GetTopologyData(
            [Header(ApiConst.Authorization)] string accessToken,
            [Path("spot_id")] long spotId);
    }
}