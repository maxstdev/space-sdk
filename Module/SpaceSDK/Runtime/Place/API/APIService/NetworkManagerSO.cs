using Maxst.Settings;
using Retrofit;
using Retrofit.HttpImpl;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace MaxstXR.Place
{
	public enum APICategory
	{
		Public,
		Topology,
		Resource
    }

	[CreateAssetMenu(fileName = "NetworkManagerSO", menuName = "ScriptableObjects/NetworkManagerSO", order = 2)]
	public class NetworkManagerSO : Maxst.ScriptableSingleton<NetworkManagerSO>
	{
        [Header("Maxverse")]
        // https://api.maxverse.io/poi-admin
        [SerializeField] private string publicUrl;
        //https://api.maxverse.io/poi-authoring-tool
        // https://api.maxst.com/space
        [SerializeField] private string topologyUrl;
        //https://api.maxverse.io/asset-integration
        [SerializeField] private string resourceUrl;
        
		[Space(16)]
        [SerializeField] private bool isSecure;
        [SerializeField] private bool enableLog;

		private string Http => isSecure ? "https://" : "http://";
        
        public string EndPoint(APICategory category) => GetEndPoint(category);
		public bool EnableLog => enableLog;

		private string GetEndPoint(APICategory category)
		{
            var DomainPrefix = EnvAdmin.Instance.CurrentEnv.Value == EnvType.Alpha ? "alpha-" : "";
		    return category switch
			{
				APICategory.Public => $"{Http}{DomainPrefix}{publicUrl}",
				APICategory.Topology => $"{Http}{DomainPrefix}{topologyUrl}",
				APICategory.Resource => $"{Http}{DomainPrefix}{resourceUrl}",
				_ => $"{Http}{DomainPrefix}{publicUrl}",
			};
		}

		private readonly Dictionary<int, RetrofitAdapter> retrofitAdapters = new();

		public RetrofitAdapter RetrofitAdapter<T>(APICategory category, int objectHashCode)
		{

			if (retrofitAdapters.ContainsKey(objectHashCode))
			{
				RetrofitAdapter rRetrofitAdapter = retrofitAdapters[objectHashCode];

				try
				{
					rRetrofitAdapter.Create<T>();
				}
				catch (Exception e) { Debug.Log(e); }
				return rRetrofitAdapter;
			}

			var adapter = new RetrofitAdapter.Builder()
				.EnableLog(EnableLog)
				.SetEndpoint(EndPoint(category))
				.SetClient(new UnityWebRequestImpl())
				.Build();

			retrofitAdapters.TryAdd(objectHashCode, adapter);

			try
			{
				adapter.Create<T>();
			}
			catch (Exception e) { Debug.Log(e); }
			return adapter;
		}
	}
}