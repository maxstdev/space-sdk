using Maxst.Settings;
using Retrofit;
using Retrofit.HttpImpl;
using Retrofit.Parameters;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MaxstXR.Place
{
    public class SpaceConsole : RestAdapter, ISpaceConsoleApi
    {
        private static readonly object lockObj = new ();
        private static SpaceConsole instance;
        private static volatile bool applicationIsQuitting = false;

        public static SpaceConsole Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning("[Singleton] Instance 'SpaceConsole'" +
                        " already destroyed on application quit." +
                        " Won't create again - returning null.");
                    return null;
                }

                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = (SpaceConsole)FindObjectOfType(typeof(SpaceConsole));

                        if (FindObjectsOfType(typeof(SpaceConsole)).Length > 1)
                        {
                            Debug.LogError("[Singleton] Something went really wrong");
                            return instance;
                        }

                        if (instance == null)
                        {
                            GameObject singleton = new GameObject();
                            instance = singleton.AddComponent<SpaceConsole>();
                            singleton.name = "(singleton) " + typeof(SpaceConsole).ToString();
                            DontDestroyOnLoad(singleton);
                            Debug.Log("[Singleton] An instance of SpaceConsole");
                        }
                        else
                        {
                            Debug.Log("[Singleton] Using SpaceConsole instance already created");
                        }
                    }

                    return instance;
                }
            }
        }

        public void OnDestroy()
        {
            applicationIsQuitting = true;
        }

        protected override void SetRestAPI()
        {
            baseUrl = GetUrl();
            iRestInterface = typeof(ISpaceConsoleApi);
        }

        protected override RequestInterceptor SetIntercepter()
        {
            return null;
        }

        protected override HttpImplement SetHttpImpl()
        {
            var httpImpl = new UnityWebRequestImpl
            {
                EnableDebug = true
            };
            return httpImpl;
        }

        private string GetUrl()
        {
            var DomainPrefix = EnvAdmin.Instance.CurrentEnv.Value == EnvType.Alpha ? "alpha-" : "";
            return $"https://{DomainPrefix}api.maxst.com/space";
        }

        public IObservable<SpaceDatas> ReqSpaceList(
            [Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization,
            [Query("page")] int page,
            [Query("size")] int size,
            [Query("sort_by")] string sortBy,
            [Query("sort_direction")] string sortDirection)
        {
            return SendRequest<SpaceDatas>(MethodBase.GetCurrentMethod(),
                authorization, page, size, sortBy, sortDirection) as IObservable<SpaceDatas>;
        }

        public IObservable<SpaceDatas> ReqSpaceListAll(
            [Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization,
            [Query("step")] SpaceStep step,
            [Query("page")] int page,
            [Query("size")] int size,
            [Query("sort_by")] string sortBy,
            [Query("sort_direction")] string sortDirection)
        {
            return SendRequest<SpaceDatas>(MethodBase.GetCurrentMethod(),
                authorization, step, page, size, sortBy, sortDirection) as IObservable<SpaceDatas>;
        }

        public IObservable<Space> ReqSpace(
            [Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization,
            [Path("space_id")] string spaceId)
        {
            return SendRequest<Space>(MethodBase.GetCurrentMethod(),
                authorization, spaceId) as IObservable<Space>;
        }

        public IObservable<SpaceTextureUrl> GetImageUrl(
            [Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization,
            [Query("space_id")] string space_id)
            //[Header("Content-Type")] string contentType)
        {
            return SendRequest<SpaceTextureUrl>(MethodBase.GetCurrentMethod(),
                authorization, space_id) as IObservable<SpaceTextureUrl>;
        }

        public IObservable<List<Poi>> ReqPoiListFromSpace(
            [Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization,
            [Path("space_id")] string spaceid)
        {
            return SendRequest<List<Poi>>(MethodBase.GetCurrentMethod(),
                authorization, spaceid) as IObservable<List<Poi>>;
        }
    }
}
