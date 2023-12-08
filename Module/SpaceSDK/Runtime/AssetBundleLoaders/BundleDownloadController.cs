using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MaxstXR.Place
{

	public class BundleDownloadController : InjectorBehaviour
	{
		public enum DownloadState
		{
			Idle,
			Initialize,
			UpdateCatalog,
			DownloadSize,
			DownloadStart,
			Downloading,
			Finished
		}

		[DI(DIScope.component, DIComponent.place)] private BundleDownloadViewModel BundleDownloadViewModel { get; }
        [DI(DIScope.component, DIComponent.place)] private SceneViewModel SceneViewModel { get; }

        private string label;
		private string id;
		
		public string SpaceId { get => id; }

		private const string STANDALONE_WINDOWS64 = "StandaloneWindows64/";
		private const string STANDALONE_OSX = "StandaloneOSX/";
		private const string WEBGL = "WebGL/";
		private string Platform = STANDALONE_WINDOWS64;

		private Dictionary<DownloadState, UnityAction<string>> stateToMethodMap;

		private DownloadState currentState = DownloadState.Idle;
		private DownloadState prevState = DownloadState.Idle;

        private void Awake()
        {
            BundleDownloadViewModel.Awake(gameObject);
        }

        private void OnEnable()
		{
#if UNITY_EDITOR_WIN
			Platform = STANDALONE_WINDOWS64;
#elif UNITY_EDITOR_OSX
			Platform = STANDALONE_OSX;
#elif UNITY_WEBGL
			Platform = WEBGL;
#endif
			Debug.Log($"Application.platform : {Application.platform}, set Platform : {Platform}");
		}


		private void Start()
		{
			stateToMethodMap = new Dictionary<DownloadState, UnityAction<string>>()
			{
				{ DownloadState.Initialize,  InitializedSystem },
				{ DownloadState.UpdateCatalog,  UpdateCatalog },
				{ DownloadState.DownloadSize,  DownloadSizeCheck },
				{ DownloadState.DownloadStart,  DownloadStart },
				{ DownloadState.Downloading,  Downloading },
			};
		}

		public async void StartFetchProcessAsync(string space)
		{
			currentState = prevState = DownloadState.Initialize;
			label = space;
			id = space;

			while (currentState != DownloadState.Finished)
			{
				await UniTask.NextFrame();
				if (false == stateToMethodMap.TryGetValue(currentState, out var process))
				{
					continue;
				}
				process.Invoke(space);
			}
		}

		private void Downloading(string space)
		{
			//Debug.Log("DonwloadController : Downloading");
			BundleDownloadViewModel.UpdateDownloadStatus(space);
		}

		private void DownloadStart(string space)
		{
			//Debug.Log("DonwloadController : DownloadStart");
			BundleDownloadViewModel.StartDownloadAsync(space).Forget();
			currentState = DownloadState.Downloading;
		}

		private void DownloadSizeCheck(string space)
		{
			//Debug.Log("DonwloadController : DownloadSizeCheck");
			currentState = DownloadState.Idle;
			BundleDownloadViewModel.DownloadSizeAsync(space).Forget();
		}

		private void UpdateCatalog(string space)
		{
			//Debug.Log("DonwloadController : UpdateCatalog");
			currentState = DownloadState.Idle;
			BundleDownloadViewModel.UpdateCatalogAsync(space).Forget();
		}

		private void InitializedSystem(string space)
		{
			//Debug.Log("DonwloadController : InitializedSystem");
			currentState = DownloadState.Idle;
            BundleDownloadViewModel.InitializedSystemAsync(space, label, SceneViewModel.CurrentBundleKey()).Forget();
		}

		public void GoNextStatus()
		{
			currentState = prevState switch
			{
				DownloadState.Initialize => DownloadState.UpdateCatalog,
				DownloadState.UpdateCatalog => DownloadState.DownloadSize,
				DownloadState.DownloadSize => DownloadState.DownloadStart,
				DownloadState.Downloading or DownloadState.DownloadStart => DownloadState.Finished,
				_ => throw new System.ArgumentOutOfRangeException(),
			};
			prevState = currentState;
		}
	}
}