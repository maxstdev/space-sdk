using Retrofit.Methods;
using Retrofit.Parameters;
using System;
using System.Collections.Generic;

namespace MaxstXR.Place
{
    public interface IXRApi
    {
        [Get("/poi-customer/v1/api/spot/download?textured_file_path={texturePath}")]
        IObservable<string> ImageDownloadUrl(
        [Header("Authorization")] string authorization,
        [Header("Content-Type")] string contentType,
        [Path("texturePath")] string texturePath
        );


        [Get("/poi-customer/v1/spot/textured-file")]
        IObservable<string> GetImageUrl(
        [Query("spot_id")] string spot_id,
        [Header("Authorization")] string authorization,
        [Header("Content-Type")] string contentType
        );

        [Get("/poi-customer/v1/api/place/{placeid}")]
        IObservable<string> GetPlaceData(
        [Header("Authorization")] string authorization,
        [Header("Content-Type")] string contentType,
        [Path("placeid")] string placeid
        );

        [Get("/poi-customer/v1/api/spot/{spotid}")]
        IObservable<string> GetSpotData(
        [Header("Authorization")] string authorization,
        [Header("Content-Type")] string contentType,
        [Path("spotid")] string spotid
        );

        [Get("/poi-customer/v1/api/logs")]
        IObservable<string> GetLog(
        [Header("Authorization")] string authorization,
        [Query("aid")] string aid,
        [Query("aname")] string aname,
        [Query("env")] string env,
        [Query("mcc")] string mcc,
        [Query("mpn")] string mpn,
        [Query("msn")] string msn,
        [Query("oid")] string oid,
        [Query("on")] string on,
        [Query("pn")] string pn,
        [Query("sid")] string sid,
        [Query("success")] string success,
        [Query("sv")] string sv,
        [Query("uid")] string uid
        );

        [Post("/vps/v1/path")]
        IObservable<string> PostNaviData(
        [Header("Authorization")] string authorization,
        [Header("Content-Type")] string contentType,
        [Body] Dictionary<string, string> body
        );
        
    }
}

