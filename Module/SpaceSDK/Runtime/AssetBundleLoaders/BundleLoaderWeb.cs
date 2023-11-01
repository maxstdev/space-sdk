using System.Collections;
using UnityEngine;
using UnityEngine.Networking;


namespace MaxstXR.Place
{
    public class BundleLoaderWeb : MonoBehaviour
    {
        private string bundleUrl = "https://xr-client-web.s3.ap-northeast-2.amazonaws.com/assetBundle/testbundle.unity3d";
        //private string bundleUrl = "http://localhost:8000/testbundle.unity3d";

        [SerializeField] private string assetName = "BundleTestCube.prefab";

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(LoadBundle());
        }

        private IEnumerator LoadBundle()
        {
            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                    var names = bundle.GetAllAssetNames();
                    foreach (var name in names)
                    {
                        Debug.Log($">>> {name}");
                    }
                    Instantiate(bundle.LoadAsset(assetName));
                }
            }
        }

        private IEnumerator TestAccess()
        {
            using (UnityWebRequest www = UnityWebRequest.Get(bundleUrl))
            {
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    Debug.Log(">>> Access successful");
                }
            }
        }
    }
}