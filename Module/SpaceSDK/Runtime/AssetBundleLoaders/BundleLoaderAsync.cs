using System.Collections;
using System.IO;
using UnityEngine;

namespace MaxstXR.Place
{
    public class BundleLoaderAsync : MonoBehaviour
    {
        [SerializeField] private string bundleName = "testbundle";
        [SerializeField] private string assetName = "BundleTestCube";

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(LoadBundleAsync());
        }

        private IEnumerator LoadBundleAsync()
        {
            AssetBundleCreateRequest asyncBundleRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, bundleName));
            yield return asyncBundleRequest;
            Debug.Log("Successfully loaded bundle(async)");

            AssetBundle localAssetBundle = asyncBundleRequest.assetBundle;
            if (localAssetBundle == null)
            {
                Debug.LogError("Failed to load AssetBundle!");
                yield break;
            }

            AssetBundleRequest assetRequest = localAssetBundle.LoadAssetAsync<GameObject>(assetName);
            yield return assetRequest;

            GameObject asset = assetRequest.asset as GameObject;
            Instantiate(asset);

            localAssetBundle.Unload(false);
        }
    }
}
