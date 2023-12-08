using Cysharp.Threading.Tasks;
using JsonFx.Json;
using Maxst.Passport;
using maxstAR;
using MaxstUtils;
using MaxstXR.Extension;
using MaxstXR.Place;
using System;
using UniRx;
using UnityEngine;
using static maxstAR.XRAPI;

public class XRServiceManager : InSceneUniqueBehaviour
{
    public static XRServiceManager Instance(GameObject go) => Instance<XRServiceManager>(go);

    [HideInInspector]
    public string spaceId = "";

    private string vrStoragePath = "";

    protected override void Awake()
    {
        base.Awake();
        InitPlaceSpot();
    }

    public void InitPlaceSpot()
    {
        //var povManager = GetComponentInChildren<PovManager>();

        var povManager = FindObjectOfType<PovManager>();
        if (povManager == null) return;
        GameObject trackable = povManager.Trackable;
        VPSTrackable vPSTrackable = trackable?.GetComponent<VPSTrackable>();
        if (vPSTrackable == null) return;
        SetPlaceIdSpotId();
    }

    public async void SetPlaceIdSpotId()
    {
        await SetAccessToken();
        //await SetXRPlaceData(placeId);
        //await SetXRSpotData(spotId);


        TextureManager.TexturesDirectory = GetVRImagePath();
        SmoothTextureManager.TexturesDirectory = GetVRImagePath();
    }

#if false
    private async UniTask SetXRPlaceData(int placeId)
    {
        var authorization = await XRTokenManager.Instance.GetActiveToken(TokenRepo.Instance.passportConfig);
        var ob = XRService.Instance.GetPlaceData(
                authorization,
                XRTokenManager.Instance.contentType,
                placeId.ToString()
            );

        ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
           .ObserveOn(Scheduler.MainThread)
           .Subscribe(data =>
           {
               if (data != null)
               {
                   placeData = JsonReader.Deserialize<PlaceData>(data);
                   if (spotData != null)
                   {
                       AddDeviceInformation();
                   }
               }
           },
           error =>
           {
               Debug.Log($"place data error => {error}");
           },
           () =>
           {

           });

        await ob;
    }


    private async UniTask SetXRSpotData(int spotId)
    {
        var authorization = await XRTokenManager.Instance.GetActiveToken(TokenRepo.Instance.passportConfig);
        var ob = XRService.Instance.GetSpotData(
            authorization,
            XRTokenManager.Instance.contentType,
            spotId.ToString()
            );

        ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
           .ObserveOn(Scheduler.MainThread)
        .Subscribe(data =>
        {
            if (data != null)
            {
                   spotData = JsonReader.Deserialize<SpotData>(data);
                   vrStoragePath = spotData.spot_directory;
                   TextureManager.TexturesDirectory = GetVRImagePath();
                   SmoothTextureManager.TexturesDirectory = GetVRImagePath();
                   if (placeData != null)
                   {
                       AddDeviceInformation();
                   }
               }
           },
           error =>
           {
               Debug.Log($"spot data error => {error}");
           },
           () =>
           {

           });

        await ob;
    }
#endif

#if false
    public void AddDeviceInformation()
    {
        string deviceUUID = PlayerPrefs.GetString("device_uuid", "");
        if (deviceUUID.Equals(""))
        {
            deviceUUID = SystemInfo.deviceUniqueIdentifier;
            PlayerPrefs.SetString("device_uuid", deviceUUID);
        }

        string identifier = Application.identifier;
        string productName = Application.productName;
        string place_unique_name = this.placeData.place_unique_name;
        string vps_spot_name = this.spotData.vps_spot_name;
        TrackerManager.GetInstance().AddTrackerData("{\"vps_log\":1,\"device_uuid\":\"" + deviceUUID + "\",\"identifier\":\"" + identifier + "\",\"product_name\":\"" + productName + "\",\"place_unique_name\":\"" + place_unique_name + "\",\"vps_spot_name\":\"" + vps_spot_name + "\"}");
    }


    public async void SendLog(Operation operation, bool success)
     {
         await SetAccessToken();
        XRType xRType = XRType.AR;
        if (!XRStudioController.Instance.ARMode)
        {
            xRType = XRType.XR;
        }

        await SendLog(xRType, operation, success);
    }

    private async UniTask SendLog(XRType xRType, Operation operation, bool success)
    {
        string deviceUUID = PlayerPrefs.GetString("device_uuid", "");
        if (deviceUUID.Equals(""))
        {
            deviceUUID = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("device_uuid", deviceUUID);
        }

        string operationUUID = System.Guid.NewGuid().ToString();
        string platform = "";
        if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
        {
            platform = "MacOS";
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            platform = "iOS";
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            platform = "Android";
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            platform = "Windows";
        }
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            platform = "WebGL";
        }

        int successInt = 0;
        if (success)
        {
            successInt = 1;
        }

        string versoin = "0.11.0";
        string aid = Application.identifier;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            aid = "WebGL";
        }
        var authorization = await XRTokenManager.Instance.GetActiveToken(TokenRepo.Instance.passportConfig);
        var ob = XRService.Instance.GetLog(
            authorization: authorization,
            aid: aid,
            aname: Application.productName,
            env: "dev",
            mcc: xRType.ToString(),
            mpn: placeData.place_unique_name,
            msn: spotData.vps_spot_name,
            oid: operationUUID,
            on: operation.ToString(),
            pn: platform,
            sid: "MAXVERSE",
            success: successInt.ToString(),
            sv: versoin,
            uid: deviceUUID
        );

        ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
           .ObserveOn(Scheduler.MainThread)
           .Subscribe(data =>
           {
               Debug.Log("Send Log Success");
           },
           error =>
           {
               Debug.Log($"Send Log fail {error}");
           },
           () =>
           {

           });

        await ob;
    }
#endif

    public string GetVRImagePath()
    {
        string vrImageURL = vrStoragePath + "/textured/";
        return vrImageURL;
    }

    public async UniTask VRImagePath(Action<string> complete)
    {
        await UniTask.WaitUntil(() => !string.IsNullOrEmpty(vrStoragePath));
        string vrImageURL = vrStoragePath + "/textured/";
        
        complete?.Invoke(vrImageURL);
    }

    public async UniTask RefreshToken(Action complete = null)
    {
        await SetAccessToken();

        complete?.Invoke();
    }

    private async UniTask SetAccessToken()
    {
        var token = await XRTokenManager.Instance.GetActiveToken(TokenRepo.Instance.passportConfig);
        TrackerManager.GetInstance().SetAccessToken(token);
    }

    public void Clear()
    {
        TokenRepo tokenrepo = FindObjectOfType<TokenRepo>();
        if (tokenrepo != null) 
        {
            DestroyImmediate(tokenrepo.gameObject);
        }
    }
}
