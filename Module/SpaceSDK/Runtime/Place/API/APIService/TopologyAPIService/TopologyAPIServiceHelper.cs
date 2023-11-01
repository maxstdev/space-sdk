using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace MaxstXR.Place
{
    public class TopologyAPIServiceHelper : MonoBehaviour
    {
        private ITopologyAPIServiceability service;

        static public TopologyAPIServiceHelper Build(GameObject parent)
        {
            if (parent.TryGetComponent<TopologyAPIServiceHelper>(out var rComponent)) return rComponent;

            return parent.AddComponent<TopologyAPIServiceHelper>();
        }

        protected void Awake()
        {
            var adapter = NetworkManagerSO.Instance.RetrofitAdapter<ITopologyAPIServiceability>(APICategory.Topology, this.GetHashCode());
            service = new TopologyAPIService(adapter);
        }

        public async UniTask<string> GetTopologyData(string authorization, long spotId)
        {
            TaskCompletionSource<string> completionSource = new();
            service.GetTopologyData(authorization, spotId)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
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
    }
}