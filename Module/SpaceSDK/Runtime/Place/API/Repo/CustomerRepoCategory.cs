#define API_DETAIL_DEBUG
using Cysharp.Threading.Tasks;
using Maxst.Passport;
using maxstAR;
using MaxstUtils;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace MaxstXR.Place
{
	public partial class CustomerRepo
    {
        public Event<Dictionary<long, BaseCategory>> categoryDictionaryEvent = new();
        public Event<HashSet<BaseCategory>> categoryFilterList = new();

        public async UniTask<List<FirstCategory>> FetchCategoryList()
        {
            var completionSource = new TaskCompletionSource<List<FirstCategory>>();

            var token = await GetClientToken();

            var ob = CustomerService.Instance.ReqCategoryList(token.access_token);
            ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>   // on success
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"ReqCategoryList Received success data count : {data.Count}");
#endif
                    var dic = new Dictionary<long, BaseCategory>();
                    if (data?.Count > 0)
                    {
                        foreach (var f in data)
                        {
                            dic.Add(f.categoryId, f);
                            foreach (var s in f.secondCategoryList)
                            {
                                s.Parent = f;
                                dic.Add(s.categoryId, s);
                                foreach (var t in s.thirdcategoryList)
                                {
                                    t.Parent = s;
                                    dic.Add(t.categoryId, t);
                                }
                            }
                        }
                    }
                    categoryDictionaryEvent.Post(dic);
                    completionSource.TrySetResult(data);
                    cacheCategory = null;
                },
                error => // on error
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"ReqCategoryList fail : {error}");
#endif
                    completionSource.TrySetException(error);

                },
                () =>
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"ReqCategoryList req complete");
#endif

                });

            return await completionSource.Task;
        }

        public async UniTask<FirstCategory> FetchCategorDatail(long categoryId)
        {
            var completionSource = new TaskCompletionSource<FirstCategory>();

            var token = await GetClientToken();

            var ob = CustomerService.Instance.ReqCategoryDetail(token.access_token, categoryId);
            ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>   // on success
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"ReqCategoryDetail Received success data : {data.categoryName}");
#endif
                    completionSource.TrySetResult(data);
                },
                error => // on error
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"ReqCategoryDetail fail : {error}");
#endif
                    completionSource.TrySetException(error);

                },
                () =>
                {
#if API_DETAIL_DEBUG
                    Debug.Log($"ReqCategoryDetail req complete");
#endif

                });

			return await completionSource.Task;
        }

        internal void UpdateCategoryFilterList(HashSet<BaseCategory> filterList)
        {
            categoryFilterList.Post(filterList);
        }
    }
}
