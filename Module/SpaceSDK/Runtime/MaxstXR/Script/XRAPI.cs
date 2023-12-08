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
        public string spaceId = "";

        private SpaceData spaceData = null;
        private bool debugMode = false;
        private string debugTexturePath = "";
        public string accessToken = "";
        public const string domain = "https://api.maxst.com/space";
        public const string domain2 = "https://api.maxst.com/vps";
        public const string naviURL = domain2 + "/v1/path";
        public const string spaceUrl = domain + "/v1/space/";
        public const string newImageDownloadUrl = domain + "/v1/space/";
        private string imagePolicy = "";
        private DateTime imagePolicyExpireTime = DateTime.MinValue;


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
            this.spaceId = "";
        }

        public void Start()
        {
            PovManager povManager = GetComponentInChildren<PovManager>(true);
            GameObject trackable = povManager.Trackable;
            VPSTrackable vPSTrackable = trackable.GetComponent<VPSTrackable>();
            this.spaceId = vPSTrackable.spaceId;
            TrackerManager.GetInstance().SetAccessToken(this.accessToken);
            SetSpaceId(this.spaceId);
        }

        public void SetSpaceId(string spaceId)
        {
            TrackerManager.GetInstance().AddTrackerData("{\"vps_spaceid\":\"" + XRAPI.Instance.spaceId + "\"}");
            this.spaceId = spaceId;

            if(debugMode)
            {
                return;
            }

            if (this.accessToken == "")
            {
                Debug.LogError("No AccessToken");
            }

            var headers = GetHeaders();
            StartCoroutine(APIController.GET(spaceUrl + spaceId + "/public", headers, null, 10, (resultString) =>
            {
                //Debug.Log(resultString);
                if (resultString != "")
                {
                    this.spaceData = JsonReader.Deserialize<SpaceData>(resultString);
                }
            }, (failString) => { }));


            StartCoroutine(APIController.GET(newImageDownloadUrl + spaceId + "/texture" , headers, null, 10, (resultString) =>
            {
                //Debug.Log($"newImageDownloadUrl resultString : {resultString}");
                if (resultString != "")
                {
                    VRImagePolicyData vrImagePolicyData = JsonReader.Deserialize<VRImagePolicyData>(resultString);
                    imagePolicy = vrImagePolicyData.pre_signed_url;
                    imagePolicyExpireTime = DateTime.Now;
                    TextureManager.TexturesDirectory = imagePolicy;
                }
            }, (failString) => { }));
        }

        public IEnumerator GetVRImagePathCoroutine(System.Action<string> result)
        {
            string vrImageURL = newImageDownloadUrl + spaceId + "/texture";

            if (this.accessToken == "")
            {
                Debug.LogError("No AccessToken");
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
                this.spaceId = vPSTrackable.spaceId;

                var headers = GetHeaders();
                yield return StartCoroutine(APIController.GET(newImageDownloadUrl + spaceId + "/texture", headers, null, 10, (resultString) =>
                {
                    //Debug.Log(resultString);
                    if (resultString != "")
                    {
                        VRImagePolicyData vrImagePolicyData = JsonReader.Deserialize<VRImagePolicyData>(resultString);
                        imagePolicy = vrImagePolicyData.pre_signed_url;
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
            string vrImageURL = newImageDownloadUrl + spaceId + "/texture";

            if (this.accessToken == "")
            {
                Debug.LogError("No AccessToken");
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
                this.spaceId = vPSTrackable.spaceId;

                if(this.accessToken == "") {
                    Debug.LogError("No AccessToken");
                }

                var headers = GetHeaders();
                await APIController.GETAsync(newImageDownloadUrl + spaceId + "/texture", headers, null, 10, (resultString) =>
                {
                    if (resultString != "")
                    {
                        VRImagePolicyData vrImagePolicyData = JsonReader.Deserialize<VRImagePolicyData>(resultString);
                        imagePolicy = vrImagePolicyData.pre_signed_url;
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
            string vrImageURL = newImageDownloadUrl + spaceId + "/texture";

            if (this.accessToken == "")
            {
                Debug.LogError("No AccessToken");
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
                this.spaceId = vPSTrackable.spaceId;

                var headers = GetHeaders();

                StartCoroutine(APIController.GET(newImageDownloadUrl + spaceId + "/texture", headers, null, 10, (resultString) =>
                {
                    //Debug.Log(resultString);
                    if (resultString != "")
                    {
                        VRImagePolicyData vrImagePolicyData = JsonReader.Deserialize<VRImagePolicyData>(resultString);
                        imagePolicy = vrImagePolicyData.pre_signed_url;
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
                { "Authorization", "Bearer " + this.accessToken},
                { "Content-Type", "application/json" }
            };
        }


        public void SetAccessToken(string accessToken)
        {
            this.accessToken = accessToken;
            TrackerManager.GetInstance().SetAccessToken(this.accessToken);
        }

        public string GetAccessToken()
        {
            if (this.accessToken == "")
            {
                Debug.LogError("No AccessToken");
            }
            return this.accessToken;
        }
    }
}

