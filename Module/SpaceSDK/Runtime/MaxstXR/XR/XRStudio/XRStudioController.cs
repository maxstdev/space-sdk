using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using maxstAR;
#if UNITY_EDITOR
//using Unity.EditorCoroutines.Editor;
#endif

#if false
[InitializeOnLoadAttribute]
public static class PlayModeStateChanged
{
    // register an event handler when the class is initialized
    static PlayModeStateChanged()
    {
        EditorApplication.playModeStateChanged += XRStudioController.PlayModeState;
    }
}
#endif
public class XRStudioController : MaxstSingleton<XRStudioController>
{
#if false
    public static void PlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {

        }
    }
#endif
    [HideInInspector]
    [SerializeField]
    public string xrPath = "";

    [HideInInspector]
    [SerializeField]
    private int selectIndex;

    [HideInInspector]
    [SerializeField]
    public string xrSimulatePath = "";

    [HideInInspector]
    [SerializeField]
    private int simulate_selectIndex;

    [HideInInspector]
    [SerializeField]
    public GameObject meshObject;

    [SerializeField]
    public bool ARMode;

    [field: SerializeField]
    public GameObject PovPrefeb { get; private set; }
    [field: SerializeField]
    public Material IbrCullBack { get; private set; }
    [field: SerializeField]
    public Material IbrCullFront { get; private set; }

    public int SelectIndex
    {
        get
        {
            return selectIndex;
        }
        set
        {
            selectIndex = value;
        }
    }

    public int Simulate_SelectIndex
    {
        get
        {
            return simulate_selectIndex;
        }
        set
        {
            simulate_selectIndex = value;
        }
    }

    [SerializeField]
    public static string vpsName = "";

    public int GetSelectedIndex()
    {
        return selectIndex;
    }

    public void SetSelectedIndex(int index)
    {
        selectIndex = index;
    }

#if false
    public void LoadMap()
    {
        var name = Path.GetFileName(xrPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        vpsName = name;


        EditorCoroutineUtility.StartCoroutine(LoadAssetResource(xrPath, vpsName), this);
    }

    public IEnumerator LoadAssetResource(string path, string vpsName)
    {
        string destinationfolderPath = Application.streamingAssetsPath + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "MaxstXR" + Path.DirectorySeparatorChar + "Contents" + Path.DirectorySeparatorChar + vpsName;
        if (!Directory.Exists(destinationfolderPath))
        {
            Directory.CreateDirectory(destinationfolderPath);
        }

        string mapPath = path;
        string[] files = Directory.GetFiles(mapPath);
        string destinationFolder = destinationfolderPath;
        List<string> loadPrefabs = new List<string>();
        string xrdataPath = "";
      
        foreach (string file in files)
        {
            string destinationFile = "";
            string extension = Path.GetExtension(file);

            if (extension == ".fbx" || extension == ".meta" || extension == ".prefab" || extension == ".mat" || extension == ".obj" || extension == ".jpg" || extension == ".png" || extension == ".atn" || extension == ".atm")
            {
                destinationFile = destinationFolder + Path.DirectorySeparatorChar + Path.GetFileName(file);
                if (Path.GetFileNameWithoutExtension(destinationFile).Contains("Trackable") && Path.GetExtension(destinationFile) == ".prefab")
                {
                    loadPrefabs.Add(destinationFile);
                }

                string fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName == "XRPov")
                {
                    xrdataPath = destinationFolder + Path.DirectorySeparatorChar + Path.GetFileName(file);
                }
                //Debug.Log(fileName);
            }

            if (destinationFile != "")
            {
                System.IO.File.Copy(file, destinationFile, true);
            }
        }
      
        yield return new WaitForEndOfFrame();

        AssetDatabase.Refresh();

        GameObject trackableGameObject = null;
        foreach(string eachLoadFile in loadPrefabs)
        {
            GameObject local_meshObject = PrefabUtility.LoadPrefabContents(eachLoadFile);
            meshObject = Instantiate(local_meshObject);
            meshObject.name = Path.GetFileNameWithoutExtension(eachLoadFile);
            if(meshObject.name.Contains("Trackable"))
            {
                VPSTrackable trackable = meshObject.GetComponent<VPSTrackable>();
                trackableGameObject = meshObject;

                XRAPI.Instance.SetSpaceId(trackable.spaceId);
            }
        }

        GameObject xrdata_object = PrefabUtility.LoadPrefabContents(xrdataPath);
        xrdata_object = Instantiate(xrdata_object);
        xrdata_object.name = "XRPov";
        xrdata_object.transform.parent = this.transform;
        xrdata_object.GetComponent<PovManager>().Trackable = trackableGameObject;
    }

    public void Clear()
    {
        XRAPI.Instance.Clear();
        Clear(gameObject);
    }

    private void Clear(GameObject clearObject)
    {
        PovManager povManager = clearObject.GetComponentInChildren<PovManager>();

        if(povManager.Trackable != null)
        {
            DestroyImmediate(povManager.Trackable,true);
        }
       
        foreach (Transform child in clearObject.transform)
        {
           
            DestroyImmediate(child.gameObject);
        }

        if (clearObject.transform.childCount > 0)
        {
            Clear(clearObject);
        }

        
    }
#endif

    public void ReloadName()
    {
        var name = Path.GetFileName(xrPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        vpsName = name;
    }

    private void Awake()
    {
        if(Application.isPlaying && ARMode)
        {
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetActive(false);
            }
        }   
    }
}
