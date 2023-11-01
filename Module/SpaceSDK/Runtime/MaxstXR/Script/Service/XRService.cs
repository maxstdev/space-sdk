using Retrofit;
using Retrofit.HttpImpl;
using Retrofit.Parameters;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HeaderAttribute = Retrofit.Parameters.HeaderAttribute;

namespace MaxstXR.Place
{
    public class XRService : RestAdapter, IXRApi
    {
        private static XRService instance;
        public static XRService Instance
        {

            get
            {
                if(instance == null)
                {
                    var xrService = new GameObject(typeof(XRService).FullName);
                    instance = xrService.TryGetOrAddComponent<XRService>();
                    if(Application.isPlaying)
                        DontDestroyOnLoad(xrService);
                }
                return instance;
            }
        }

        protected override HttpImplement SetHttpImpl()
        {
            var httpImpl = new UnityWebRequestImpl();
            httpImpl.EnableDebug = true;
            return httpImpl;
        }

        protected override RequestInterceptor SetIntercepter()
        {
            return null;
        }

        protected override void SetRestAPI()
        {
            baseUrl = "https://api.maxverse.io";
            iRestInterface = typeof(IXRApi);
        }

        public IObservable<string> ImageDownloadUrl(
            [Header("Authorization")] string authorization, 
            [Header("Content-Type")] string contentType, 
            [Query("texturePath")] string texturePath)
        {
            return SendRequest<string>(MethodBase.GetCurrentMethod(),
                authorization, contentType, texturePath) as IObservable<string>;
        }
        public IObservable<string> GetImageUrl(
            [Query("spot_id")] string spot_id, 
            [Header("Authorization")] string authorization, 
            [Header("Content-Type")] string contentType)
        {
            return SendRequest<string>(MethodBase.GetCurrentMethod(), spot_id,
                authorization, contentType) as IObservable<string>;
        }

        public IObservable<string> GetPlaceData(
            [Header("Authorization")] string authorization, 
            [Header("Content-Type")] string contentType, 
            [Path("placeid")] string placeid)
        {
            return SendRequest<string>(MethodBase.GetCurrentMethod(),
                authorization, contentType, placeid) as IObservable<string>;
        }

        public IObservable<string> GetSpotData(
            [Header("Authorization")] string authorization, 
            [Header("Content-Type")] string contentType, 
            [Path("spotid")] string spotid)
        {
            return SendRequest<string>(MethodBase.GetCurrentMethod(),
                authorization, contentType, spotid) as IObservable<string>;
        }


        public IObservable<string> GetLog(
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
            [Query("uid")] string uid)
        {
            return SendRequest<string>(MethodBase.GetCurrentMethod(),
                authorization, aid, aname, env, mcc, mpn, msn, oid, on, pn, sid, success, sv, uid) as IObservable<string>;
        }

        public IObservable<string> PostNaviData(
            [Header("Authorization")] string authorization, 
            [Header("Content-Type")] string contentType, 
            [Body] Dictionary<string, string> body)
        {
            return SendRequest<string>(MethodBase.GetCurrentMethod(),
                authorization, contentType, body) as IObservable<string>;
        }

    }
}

