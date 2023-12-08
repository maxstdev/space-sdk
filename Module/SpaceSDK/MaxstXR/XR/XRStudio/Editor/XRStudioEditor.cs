using maxstAR;
using MaxstXR.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(XRStudioController))]
public class XRStudioEditor : Editor
{
    public const string vpsPath = "XRData";
    public const string vpsXRMap = "XRMap";
    public const string vpsSimulationData = "XRSimulationData";
    public const string vpsServerName = "";

    private int simulate_selectIndex = 0;
    private int before_simulate_ChoiceIndex = -1;

    private int selectIndex = 0;
    private int beforeChoiceIndex = -1;

    public override void OnInspectorGUI()
    {
        XRStudioController xrStudioController = (XRStudioController)target;

        EditorGUILayout.LabelField("XR Map");
        var dataFullPath = FindParentFolder(vpsPath);
        if (string.IsNullOrEmpty(dataFullPath)) return;

        string folderPath = Path.Combine(dataFullPath, vpsXRMap);
        string[] directories = Directory.GetDirectories(folderPath);

        List<string> directory_name = new List<string>();
        foreach (string directory in directories)
        {
            var name = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            directory_name.Add(name);
        }
        selectIndex = EditorGUILayout.Popup(xrStudioController.SelectIndex, directory_name.ToArray());

        GUILayout.Space(10);

        EditorGUILayout.LabelField("XR Simulation Data");
        string selectVPSName = directory_name[selectIndex];
        string[] simulate_directories = null;
        if (selectVPSName != "")
        {
            folderPath = Path.Combine(dataFullPath, vpsSimulationData, selectVPSName);
         
            if(Directory.Exists(folderPath))
            {
                simulate_directories = Directory.GetDirectories(folderPath);
            }

            if(simulate_directories != null && simulate_directories.Length != 0)
            {
                List<string> simulate_directory_name = new List<string>();
                foreach (string directory in simulate_directories)
                {
                    var name = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                    simulate_directory_name.Add(name);
                }

                simulate_selectIndex = EditorGUILayout.Popup(xrStudioController.Simulate_SelectIndex, simulate_directory_name.ToArray());
            }
        }
        
        GUILayout.Space(10);

        EditorGUILayout.Separator();
        GUIContent makeContent = new GUIContent("Load XR Map");
        if (GUILayout.Button(makeContent, GUILayout.MaxWidth(Screen.width), GUILayout.MaxHeight(50)))
        {
            LoadMap(xrStudioController);
        }
        GUILayout.Space(10);

        GUIContent clearContent = new GUIContent("Clear");
        if (GUILayout.Button(clearContent, GUILayout.MaxWidth(Screen.width), GUILayout.MaxHeight(50)))
        {
            Clear(xrStudioController);
        }
        GUILayout.Space(10);


        DrawDefaultInspector();

        bool isDirty = false;
        if (selectIndex != beforeChoiceIndex || simulate_selectIndex != before_simulate_ChoiceIndex)
        {
            isDirty = true;
            beforeChoiceIndex = selectIndex;
            xrStudioController.SelectIndex = selectIndex;

            before_simulate_ChoiceIndex = simulate_selectIndex;
            xrStudioController.Simulate_SelectIndex = simulate_selectIndex;
        }

        if (GUI.changed || isDirty)
        {
            xrStudioController.xrPath = directories[selectIndex];

            if(simulate_directories != null)
            {
                if(simulate_directories.Length >= simulate_selectIndex-1)
                {
                    try
                    {
                        xrStudioController.xrSimulatePath = simulate_directories[simulate_selectIndex];
                    }
                    catch (Exception)
                    {
                        simulate_selectIndex = 0;
                        before_simulate_ChoiceIndex = simulate_selectIndex;
                    }
                }
                else
                {
                    xrStudioController.xrSimulatePath = simulate_directories[0];
                    simulate_selectIndex = 0;
                    before_simulate_ChoiceIndex = simulate_selectIndex;
                    xrStudioController.Simulate_SelectIndex = simulate_selectIndex;

                }
            }
            
            EditorUtility.SetDirty(target);
        }
    }

    public void LoadMap(XRStudioController xrStudioController)
    {
        var name = Path.GetFileName(xrStudioController.xrPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        XRStudioController.vpsName = name;
        EditorCoroutineUtility.StartCoroutine(LoadAssetResource(xrStudioController), this);
    }

    public IEnumerator LoadAssetResource(XRStudioController xrStudioController)
    {
        string destinationfolderPath = Application.streamingAssetsPath + 
            Path.DirectorySeparatorChar + ".." + 
            Path.DirectorySeparatorChar + "MaxstXR" + 
            Path.DirectorySeparatorChar + "Contents" + 
            Path.DirectorySeparatorChar + XRStudioController.vpsName;
        if (!Directory.Exists(destinationfolderPath))
        {
            Directory.CreateDirectory(destinationfolderPath);
        }

        string mapPath = xrStudioController.xrPath;
        string[] files = Directory.GetFiles(mapPath);
        string destinationFolder = destinationfolderPath;
        List<string> loadPrefabs = new List<string>();
        string xrdataPath = "";

        foreach (string file in files)
        {
            string destinationFile = "";
            string extension = Path.GetExtension(file);

            if (extension == ".fbx" || extension == ".meta" || extension == ".prefab" 
                || extension == ".mat" || extension == ".obj" || extension == ".jpg" 
                || extension == ".png" || extension == ".atn" || extension == ".atm")
            {
                destinationFile = destinationFolder + Path.DirectorySeparatorChar + Path.GetFileName(file);
                if (Path.GetFileNameWithoutExtension(destinationFile).Contains("Trackable") 
                    && Path.GetExtension(destinationFile) == ".prefab")
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
        foreach (string eachLoadFile in loadPrefabs)
        {
            GameObject local_meshObject = PrefabUtility.LoadPrefabContents(eachLoadFile);
            XRStudioController.Instance.meshObject = Instantiate(local_meshObject);
            XRStudioController.Instance.meshObject.name = Path.GetFileNameWithoutExtension(eachLoadFile);
            if (XRStudioController.Instance.meshObject.name.Contains("Trackable"))
            {
                VPSTrackable trackable = XRStudioController.Instance.meshObject.GetComponent<VPSTrackable>();
                var meshRenderers = trackable.GetComponentsInChildren<MeshRenderer>();
                foreach (var mr in meshRenderers ?? new MeshRenderer[0])
                {
                    var mc = mr.gameObject.TryGetOrAddComponent<MeshCollider>();
                    mc.enabled = true;
                }
                trackableGameObject = XRStudioController.Instance.meshObject;

                XRAPI.Instance.SetSpaceId(trackable.spaceId);
            }
        }

        GameObject xrdata_object = PrefabUtility.LoadPrefabContents(xrdataPath);
        xrdata_object = Instantiate(xrdata_object);
        xrdata_object.name = "XRPov";
        xrdata_object.transform.parent = XRStudioController.Instance.transform;
        var povManager = xrdata_object.GetComponent<PovManager>();
        povManager.Trackable = trackableGameObject;
        povManager.PovPrefab = XRStudioController.Instance.PovPrefeb;
        xrdata_object.GetComponent<IbrManager>();
        /*GameObject.DestroyImmediate(ibrManager);
        var smoothIbrManager = xrdata_object.TryGetOrAddComponent<SmoothIbrManager>();
        smoothIbrManager.SetIbrMaterials(XRStudioController.Instance.IbrCullBack, XRStudioController.Instance.IbrCullFront);*/
    }

    public void Clear(XRStudioController xrStudioController)
    {
        XRAPI.Instance.Clear();
        Clear(xrStudioController.gameObject);
    }

    private void Clear(GameObject clearObject)
    {
        PovManager povManager = clearObject.GetComponentInChildren<PovManager>();

        if (povManager.Trackable != null)
        {
            DestroyImmediate(povManager.Trackable, true);
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

    private string FindParentFolder(string folderName)
    {
        var currentFolder = new DirectoryInfo(Application.dataPath);
        while (true)
        {
            try
            {
                string path = Path.Combine(currentFolder.FullName, folderName);
                if (Directory.Exists(path))
                {
                    return path;
                }
                else
                {
                    currentFolder = currentFolder.Parent;
                    if (currentFolder != null)
                    {
                        continue;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                break;
            }
        }
        return null;
    }
}
