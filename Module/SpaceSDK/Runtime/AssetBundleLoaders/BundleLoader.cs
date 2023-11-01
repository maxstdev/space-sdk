using System.IO;
using UnityEngine;

namespace MaxstXR.Place
{
    public class BundleLoader : MonoBehaviour
    {
        [SerializeField] private string bundleName = "testbundle";
        [SerializeField] private string assetName = "BundleTestCube";

        // Start is called before the first frame update
        void Start()
        {
            LoadBundle();
        }

        private void LoadBundle()
        {
            AssetBundle localAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, bundleName));
            Debug.Log("Successfully loaded bundle");

            if (localAssetBundle == null)
            {
                Debug.LogError("Failed to load AssetBundle!");
                return;
            }

            GameObject asset = localAssetBundle.LoadAsset<GameObject>(assetName);
            Instantiate(asset);

            localAssetBundle.Unload(false);
        }

    }
}
