using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace MaxstXR.Place
{
    public class ResourceAPIServiceHelper : MonoBehaviour
    {
        private IResourceAPIServiceability service;

        private string clientToken;
        private string bearerAccessClientToken => (string.IsNullOrEmpty(clientToken)) ? "" : "Bearer " + clientToken;
        public string BearerAccessClientToken => bearerAccessClientToken;

        static public ResourceAPIServiceHelper Build(GameObject parent)
        {
            if (parent.TryGetComponent<ResourceAPIServiceHelper>(out var rComponent)) return rComponent;

            return parent.AddComponent<ResourceAPIServiceHelper>();
        }

        protected void Awake()
        {
            var adapter = NetworkManagerSO.Instance.RetrofitAdapter<IResourceAPIServiceability>(APICategory.Resource, this.GetHashCode());
            service = new ResourceAPIService(adapter);
        }

        public void AddClientToken(string token)
        {
            this.clientToken = token;
        }

        public async UniTask<MapSpot> ReqMapSpots(string authorization, long spotId)
        {
            TaskCompletionSource<MapSpot> completionSource = new();
            service.GetMapSpots(authorization, spotId)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
                    Debug.Log($"[ReqMapSpots] data : {data.resource.GetResourcePath()}");
                    completionSource.TrySetResult(data);
                },
                error =>
                {
                    Debug.LogError(error);
                    completionSource.TrySetException(error);
                    completionSource.SetCanceled();
                });
            return await completionSource.Task;
        }

        public async UniTask<String> GetResourcePublicInfo(string bearerToken, string resourcePath)
        {
            UnityWebRequest www = UnityWebRequest.Get(resourcePath);
            www.SetRequestHeader("token", bearerToken);

            AsyncOperation asyncOperation = www.SendWebRequest();

            await UniTask.WaitUntil(() =>
            {
                return asyncOperation.isDone;
            });

            if (www.result != UnityWebRequest.Result.Success)
            {
                throw new Exception(www.error);
            }
            else
            {
                //return JsonConvert.DeserializeObject<T>(www.downloadHandler.text);
                return www.downloadHandler.text;
            }
        }
    }
}
