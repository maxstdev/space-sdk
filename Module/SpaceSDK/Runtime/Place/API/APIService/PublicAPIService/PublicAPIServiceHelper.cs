using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace MaxstXR.Place
{
	public class PublicAPIServiceHelper : MonoBehaviour
	{
		private IPublicAPIServiceability service;

		static public PublicAPIServiceHelper Build(GameObject parent)
		{
			if (parent.TryGetComponent<PublicAPIServiceHelper>(out var rComponent)) return rComponent;

			return parent.AddComponent<PublicAPIServiceHelper>();
		}

		protected void Awake()
		{
			var adapter = NetworkManagerSO.Instance.RetrofitAdapter<IPublicAPIServiceability>(APICategory.Public, this.GetHashCode());
			service = new PublicAPIService(adapter);
		}

		//public async UniTask<SpaceData> GetSpaceFromId(string spaceId)
		//{
		//	TaskCompletionSource<SpaceData> completionSource = new();
		//	service.GetPlaceFromId(spaceId)
		//		.ObserveOn(Scheduler.MainThread)
		//		.Subscribe(data =>
		//		{
		//			completionSource.TrySetResult(data);
		//		},
		//		error =>
		//		{
		//			Debug.LogError(error);
		//			completionSource.TrySetException(error);
		//			completionSource.SetCanceled();
		//		});
		//	return await completionSource.Task;
		//}

		//public async UniTask<List<Spot>> ReqSpotList(long placeId)
		//{
		//	TaskCompletionSource<List<Spot>> completionSource = new();
		//	service.ReqSpotList(placeId)
		//		.ObserveOn(Scheduler.MainThread)
		//		.Subscribe(data =>
		//		{
		//			completionSource.TrySetResult(data);
		//		},
		//		error =>
		//		{
		//			Debug.LogError(error);
		//			completionSource.TrySetException(error);
		//			completionSource.SetCanceled();
		//		});
		//	return await completionSource.Task;
		//}
	}
}