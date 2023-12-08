using Maxst.Settings;
using Retrofit;
using Retrofit.HttpImpl;
using Retrofit.Parameters;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static System.Net.WebRequestMethods;

namespace MaxstXR.Place
{
    public class CustomerService : RestAdapter, ICustomerPoiApi
    {
        private static readonly object lockObj = new ();
        private static CustomerService instance;
        private static volatile bool applicationIsQuitting = false;

        public static CustomerService Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning("[Singleton] Instance 'CustomerService'" +
                        " already destroyed on application quit." +
                        " Won't create again - returning null.");
                    return null;
                }

                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = (CustomerService)FindObjectOfType(typeof(CustomerService));

                        if (FindObjectsOfType(typeof(CustomerService)).Length > 1)
                        {
                            Debug.LogError("[Singleton] Something went really wrong");
                            return instance;
                        }

                        if (instance == null)
                        {
                            GameObject singleton = new GameObject();
                            instance = singleton.AddComponent<CustomerService>();
                            singleton.name = "(singleton) " + typeof(CustomerService).ToString();
                            DontDestroyOnLoad(singleton);

                            Debug.Log("[Singleton] An instance of CustomerService");
                        }
                        else
                        {
                            Debug.Log("[Singleton] Using instance already created");
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
            iRestInterface = typeof(ICustomerPoiApi);
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
            return $"https://{DomainPrefix}api.maxverse.io/poi-customer";
        }

        public IObservable<List<Place>> ReqPlaceList(
      [Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization)
        {
            return SendRequest<List<Place>>(MethodBase.GetCurrentMethod(),
          authorization) as IObservable<List<Place>>;

        }

        public IObservable<List<Place>> ReqPlaceListAll(
            [Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization)
        {
            return SendRequest<List<Place>>(MethodBase.GetCurrentMethod(),
                authorization) as IObservable<List<Place>>;
        }

        public IObservable<PlaceDetail> ReqPlaceDetail(
			[Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization,
			[Path("place_id")] long placeId)
        {
            return SendRequest<PlaceDetail>(MethodBase.GetCurrentMethod(),
                authorization, placeId) as IObservable<PlaceDetail>;
        }

        public IObservable<List<Spot>> ReqSpotList(
			[Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization,
			[Path("place_id")] long placeId)
        {
            return SendRequest<List<Spot>>(MethodBase.GetCurrentMethod(),
                authorization, placeId) as IObservable<List<Spot>>;
        }

        public IObservable<SpotDetail> ReqSpotDetail(
			[Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization,
			[Path("spot_id")] long spotId)
        {
            return SendRequest<SpotDetail>(MethodBase.GetCurrentMethod(),
                authorization, spotId) as IObservable<SpotDetail>;
        }

        public IObservable<List<Poi>> ReqPoiListFormPlace(
			[Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization,
			[Path("place_id")] long placeId)
        {
            return SendRequest<List<Poi>>(MethodBase.GetCurrentMethod(),
                authorization, placeId) as IObservable<List<Poi>>;
        }

        public IObservable<PoiDetail> ReqPoiDetail(
			[Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization,
			[Path("uuid")] string uuid)
        {
            return SendRequest<PoiDetail>(MethodBase.GetCurrentMethod(),
                authorization, uuid) as IObservable<PoiDetail>;
        }

        public IObservable<List<FirstCategory>> ReqCategoryList(
			[Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization
			)
        {
            return SendRequest<List<FirstCategory>>(MethodBase.GetCurrentMethod(),
                authorization) as IObservable<List<FirstCategory>>;
        }

        public IObservable<FirstCategory> ReqCategoryDetail(
			[Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization,
			[Path("category_id")] long categoryId)
        {
            return SendRequest<FirstCategory>(MethodBase.GetCurrentMethod(),
                authorization, categoryId) as IObservable<FirstCategory>;
        }

        public IObservable<List<JointType>> ReqCategoryJointType(
			[Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization
            )
        {
            return SendRequest<List<JointType>>(MethodBase.GetCurrentMethod(),
                authorization
                ) as IObservable<List<JointType>>;
        }

        public IObservable<List<AugmentType>> ReqCategoryAugmentType(
			[Retrofit.Parameters.Header(ApiConst.Authorization)] string authorization
            )
        {
            return SendRequest<List<AugmentType>>(MethodBase.GetCurrentMethod(),
                authorization) as IObservable<List<AugmentType>>;
        }
    }
}
