using JsonFx.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace maxstAR
{
    public class XRAPI : MaxstSingleton<XRAPI>
    {
        [HideInInspector]
        public int spotId = -1;
        [HideInInspector]
        public int placeId = -1;

        public string authorizationURL = ""; //default https:/api.maxst.com/passport/token

        public string applicationId = "";
        public string applicationKey = "";
        public string grantType = "";

        private bool debugMode = false;
        private string debugTexturePath = "";

        private string accessToken = "";
        private string vrStoragePath = "";
        private long expiresTime = 0;

        public SpotData spotData;
        public PlaceData placeData;

        public const string domain = "https://api.maxverse.io";
        public const string apiURL = domain + "/poi-customer";
        public const string naviURL = domain + "/vps";
        public const string logServerURL = apiURL + "/v1/api/logs";
        public const string placeUrl = apiURL + "/v1/api/place/";
        public const string spotUrl = apiURL + "/v1/api/spot/";
        public const string newImageDownloadUrl = apiURL + "/v1/spot/textured-file";
        public string imagePolicy = "";
        private DateTime imagePolicyExpireTime = DateTime.MinValue;

        private bool isGettingAccessToken = false;
        private bool isGettingDatas = false;


        public enum Operation
        {
           Localizer,
           Navigation,
           POI
        }

        public enum XRType
        {
            AR,
            XR
        }

        public class AccessTokenData
        {
            public string access_token { get; set; }
        }

        public void Clear()
        {
            this.accessToken = "";
            this.vrStoragePath = "";
            this.placeId = -1;
            this.spotId = -1;
        }

        public void Start()
        {
            if (this.accessToken == "" || this.spotId == -1 || this.placeId == -1)
            {
                PovManager povManager = GetComponentInChildren<PovManager>(true);
                GameObject trackable = povManager.Trackable;
                VPSTrackable vPSTrackable = trackable.GetComponent<VPSTrackable>();
                this.spotId = vPSTrackable.spotId;
                this.placeId = vPSTrackable.placeId;

                SetPlaceIdSpotId(this.placeId, this.spotId);
                SetClientIdClientSecretKeyAndURLAndApplicationKey(this.applicationId, this.applicationKey, this.authorizationURL, this.grantType);
            }
            else
            {
                TrackerManager.GetInstance().SetAccessToken(this.accessToken);
            }
        }

        public void SetPlaceIdSpotId(int placeId, int spotId)
        {
            TrackerManager.GetInstance().AddTrackerData("{\"vps_placeid\":" + XRAPI.Instance.placeId + "}");
            this.accessToken = "";
            this.vrStoragePath = "";
            this.placeId = placeId;
            this.spotId = spotId;

            if(debugMode)
            {
                return;
            }

            StartCoroutine(ExecuteCoroutine(execution: () => {
      
                var headers = GetHeaders();
                StartCoroutine(APIController.GET(placeUrl + placeId, headers, null, 10, (resultString) =>
                {
                    Debug.Log(resultString);
                    if (resultString != "")
                    {
                        this.placeData = JsonReader.Deserialize<PlaceData>(resultString);

                        if(this.spotData != null)
                        {
                            AddDeviceInformation();
                        }
                    }
                }, (failString)=> { }));

                StartCoroutine(APIController.GET(spotUrl + spotId, headers, null, 10, (resultString) =>
                {
                    Debug.Log(resultString);
                    if (resultString != "")
                    {
                        this.spotData = JsonReader.Deserialize<SpotData>(resultString);
                        vrStoragePath = this.spotData.spot_directory;
                        if (this.placeData != null)
                        {
                            AddDeviceInformation();
                        }

                    }
                }, (failString) => { }));

                //Debug.Log($"newImageDownloadUrl : {newImageDownloadUrl}");
                Dictionary<string, string> vrImageParameter = new Dictionary<string, string>
                {
                    { "spot_id", "" + spotId }
                };
                StartCoroutine(APIController.GET(newImageDownloadUrl, headers, vrImageParameter, 10, (resultString) =>
                {
                    //Debug.Log($"newImageDownloadUrl resultString : {resultString}");
                    if (resultString != "")
                    {
                        imagePolicy = resultString;
                        imagePolicyExpireTime = DateTime.Now;
                        TextureManager.TexturesDirectory = imagePolicy;
                    }
                }, (failString) => { }));

            }));
        }

        private void AddDeviceInformation()
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

        private IEnumerator ExecuteCoroutine(System.Action  execution)
        {
            bool loop = true;
            while (loop)
            {
                if (!this.accessToken.Equals(""))
                {
                    loop = false;
                    break;
                }

                MakeAccessToken();

                yield return new WaitForSeconds(0.2f);
                if (!accessToken.Equals(""))
                {
                    loop = false;
                    break;
                }
            }
            //yield return new WaitForSeconds(0.1f);
            execution();
        }

        public void SendLog(Operation operation, bool success)
        {
            StartCoroutine(ExecuteCoroutine(execution: () => {
                XRType xRType = XRType.AR;
                if(!XRStudioController.Instance.ARMode)
                {
                    xRType = XRType.XR;
                }
                SendLog(this.accessToken, xRType, operation, placeData.place_unique_name, spotData.vps_spot_name, success);
            }));
        }

        public void SendLog(string accessToken, XRType xRType, Operation operation, string place, string spot, bool success)
        {
            string deviceUUID = PlayerPrefs.GetString("device_uuid", "");
            if(deviceUUID.Equals(""))
            {
                deviceUUID = System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString("device_uuid", deviceUUID);
            }

            string operationUUID = System.Guid.NewGuid().ToString();
            string platform = "";
            if(Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
            {
                platform = "MacOS";
            }
            else if(Application.platform == RuntimePlatform.IPhonePlayer)
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

            string tempAccessToken = accessToken;


            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "Authorization", "Bearer " + tempAccessToken}
            };

            int successInt = 0;
            if(success)
            {
                successInt = 1;
            }

            string versoin = "0.12.1";
            string aid = Application.identifier;

            if(Application.platform == RuntimePlatform.WebGLPlayer)
            {
                aid = "WebGL";
            }
#if false
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "aid", aid},
                { "aname",Application.productName },
                { "env", "dev" },
                { "mcc", xRType.ToString() },
                { "mpn", this.placeData.place_unique_name },
                { "msn", this.spotData.vps_spot_name },
                { "oid", operationUUID },
                { "on", operation.ToString() },
                { "pn",  platform },
                { "sid", "MAXVERSE" },
                { "success", successInt.ToString() },
                { "sv", versoin },
                { "uid", deviceUUID }
            };

            StartCoroutine(APIController.GET(logServerURL, headers, parameters, 10, (resultString) =>
            {
                Debug.Log("Send Log Success");
            }, (failString) => { Debug.Log("Send Log fail"); }));
#endif
        }


        public IEnumerator GetVRImagePathCoroutine(System.Action<string> result)
        {
            string vrImageURL = newImageDownloadUrl + vrStoragePath + "/textured/";

            if (this.accessToken == "")
            {
                MakeAccessToken();
            }

            if (debugTexturePath != "")
            {
                result(debugTexturePath);
                yield break;
            }

            TimeSpan imagePolicyExpireSpan = DateTime.Now - imagePolicyExpireTime;

            if (imagePolicy == "" || imagePolicyExpireSpan.Minutes > 25)
            {
                PovManager povManager = GetComponentInChildren<PovManager>(true);
                GameObject trackable = povManager.Trackable;
                VPSTrackable vPSTrackable = trackable.GetComponent<VPSTrackable>();
                this.spotId = vPSTrackable.spotId;
                this.placeId = vPSTrackable.placeId;

                var headers = GetHeaders();
                Dictionary<string, string> vrImageParameter = new Dictionary<string, string>
                {
                    { "spot_id", "" + spotId }
                };
                yield return StartCoroutine(APIController.GET(newImageDownloadUrl, headers, vrImageParameter, 10, (resultString) =>
                {
                    Debug.Log(resultString);
                    if (resultString != "")
                    {
                        imagePolicy = resultString;
                        imagePolicyExpireTime = DateTime.Now;
                    }
                }, (failString) => { }));
            }

            if (imagePolicy != "")
            {
                vrImageURL = imagePolicy;
            }

            result(vrImageURL);
        }

        public async Task<string> GetVRImagePathAsync()
        {
            string vrImageURL = newImageDownloadUrl + vrStoragePath + "/textured/";

            if (this.accessToken == "")
            {
                await MakeAccessTokenAsync();
            }

            if (debugTexturePath != "")
            {
                return debugTexturePath;
            }

            TimeSpan imagePolicyExpireSpan = DateTime.Now - imagePolicyExpireTime;

            if (imagePolicy == "" || imagePolicyExpireSpan.Minutes > 25)
            {
                PovManager povManager = GetComponentInChildren<PovManager>(true);
                GameObject trackable = povManager.Trackable;
                VPSTrackable vPSTrackable = trackable.GetComponent<VPSTrackable>();
                this.spotId = vPSTrackable.spotId;
                this.placeId = vPSTrackable.placeId;

                if(this.accessToken == "") {
                    await MakeAccessTokenAsync();
                }

                var headers = GetHeaders();
                Dictionary<string, string> vrImageParameter = new Dictionary<string, string>
                {
                    { "spot_id", "" + spotId }
                };
                await APIController.GETAsync(newImageDownloadUrl, headers, vrImageParameter, 10, (resultString) =>
                {
                    if (resultString != "")
                    {
                        imagePolicy = resultString;
                        imagePolicyExpireTime = DateTime.Now;
                    }
                }, (failString) => { });
            }

            if (imagePolicy != "")
            {
                vrImageURL = imagePolicy;
            }

            return vrImageURL;
        }

        public string GetNewVRImagePath()
        {
            string vrImageURL = newImageDownloadUrl + vrStoragePath + "/textured/";

            if (this.accessToken == "")
            {
                MakeAccessToken();
            }

            if (debugTexturePath != "")
            {
                return debugTexturePath;
            }

            TimeSpan imagePolicyExpireSpan = DateTime.Now - imagePolicyExpireTime;
            
            if (imagePolicy == "" || imagePolicyExpireSpan.Minutes > 25)
            {
                PovManager povManager = GetComponentInChildren<PovManager>(true);
                GameObject trackable = povManager.Trackable;
                VPSTrackable vPSTrackable = trackable.GetComponent<VPSTrackable>();
                this.spotId = vPSTrackable.spotId;
                this.placeId = vPSTrackable.placeId;

                var headers = GetHeaders();

                Dictionary<string, string> vrImageParameter = new Dictionary<string, string>
                {
                    { "spot_id", "" + spotId }
                };
                StartCoroutine(APIController.GET(newImageDownloadUrl, headers, vrImageParameter, 10, (resultString) =>
                {
                    Debug.Log(resultString);
                    if (resultString != "")
                    {
                        imagePolicy = resultString;
                        imagePolicyExpireTime = DateTime.Now;
                    }
                }, (failString) => { }));
            }

            if(imagePolicy != "")
            {
                vrImageURL = imagePolicy;
            }

            return vrImageURL;
        }

        public Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>()
            {
                { "Authorization",  "Bearer " + this.accessToken},
                { "Content-Type", "application/json" },
                { "X-App-Key", applicationKey }
            };
        }

        public void SetClientIdClientSecretKeyAndURLAndApplicationKey(string applicationId, string applicationKey, string authorizationURL, string grantType)
        {
            this.authorizationURL = authorizationURL;
            this.applicationId = applicationId;
            this.applicationKey = applicationKey;
            this.grantType = grantType;
        }

        public void MakeAccessToken()
        {
            if(this.applicationId == "" || this.applicationKey == "")
            {
                Debug.LogError("You need Client Id, Client SecretKey to get Token.");
                return;
            }

            if (isGettingAccessToken == false || IsTokenExpired())
            {
                isGettingAccessToken = true;
            }
            else
            {
                return;
            }

            var header = new Dictionary<string, string>()
            {
                { "Content-Type", "application/x-www-form-urlencoded" },
            };

            var param = new Dictionary<string, string>()
            {
                { "grant_type",grantType },
                { "client_id", applicationId },
                { "client_secret", applicationKey },
            };

            if(authorizationURL == "")
            {
                Debug.LogError("You need Authorization URL to get Access Token.");
                isGettingAccessToken = false;
                return;
            }
            //Debug.Log($"MakeAccessToken : {authorizationURL}/{header}/{param}");
            StartCoroutine(APIController.POST(authorizationURL, header, param, 5, completed: (resultString) =>
            {
                if (resultString != "")
                {
                    try
                    {
                        CredentialsToken accessTokenData = JsonReader.Deserialize<CredentialsToken>(resultString);
                        this.accessToken = accessTokenData.access_token;
                        this.expiresTime = CurrentTimeSeconds() + accessTokenData.expires_in;
                        TrackerManager.GetInstance().SetAccessToken(this.accessToken);
                    }
                    catch(Exception e)
                    {
                        isGettingAccessToken = false;
                    }
                    
                }
                isGettingAccessToken = false;
            }));
        }

        public async Task MakeAccessTokenAsync()
        {
            if (this.applicationId == "" || this.applicationKey == "")
            {
                Debug.LogError("You need Client Id, Client SecretKey to get Token.");
                return;
            }

            if (isGettingAccessToken == false || IsTokenExpired())
            {
                isGettingAccessToken = true;
            }
            else
            {
                return;
            }

            var header = new Dictionary<string, string>()
            {
                { "Content-Type", "application/x-www-form-urlencoded" },
            };

            var param = new Dictionary<string, string>()
            {
                { "grant_type", "client_credentials" },
                //{ "client_id", clientId },
                //{ "client_secret", clientSecretKey },
            };

            if (authorizationURL == "")
            {
                Debug.LogError("You need Authorization URL to get Access Token.");
                isGettingAccessToken = false;
                return;
            }

            await APIController.POSTAsync(authorizationURL, header, param, 5, completed: (resultString) =>
            {
                if (resultString != "")
                {
                    try
                    {
                        CredentialsToken accessTokenData = JsonReader.Deserialize<CredentialsToken>(resultString);
                        this.accessToken = accessTokenData.access_token;
                        this.expiresTime = CurrentTimeSeconds() + accessTokenData.expires_in;
                        TrackerManager.GetInstance().SetAccessToken(this.accessToken);
                    }
                    catch (Exception e)
                    {
                        isGettingAccessToken = false;
                    }

                }
                isGettingAccessToken = false;
            });
        }

        public string GetAccessToken()
        {
            return this.accessToken;
        }

        public string GetApplicationKey()
        {
            return this.applicationKey;
        }

        private bool IsTokenExpired()
        {
            return this.expiresTime - 30 < CurrentTimeSeconds();
        }

        private long CurrentTimeSeconds()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        [Serializable]
        public class CredentialsToken
        {
            public string access_token;
            public long expires_in;
            public string token_type;
        }
    }
}

