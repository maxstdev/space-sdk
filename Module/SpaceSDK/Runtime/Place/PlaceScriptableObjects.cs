using maxstAR;
using MaxstXR.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MaxstXR.Place
{
    public enum ZoomLevel
    {
        level_1 = 1,        //min = 30, max = 9999,
        level_2 = 2,        //min = 20, max = 40,
        level_3 = 3,        //min = 10, max = 30,
        level_4 = 4,        //min = 10, max = 20,
        level_5 = 5,        //min = 5, max = 15,
        level_6 = 6,        //min = 5, max = 10,
    }

    [Serializable]
    public class ZoomRange
    {
        [SerializeField] public float min = 5F;
        [SerializeField] public float max = 10F;

        public ZoomRange() { }

        public ZoomRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }

    [Serializable]
    public class SpotMapping
    {
        [SerializeField]
        public string povName;
        [SerializeField]
        public string trackableName;
    }

    [CreateAssetMenu(fileName = "PlaceResource", menuName = "ScriptableObjects/PlaceScriptableObjects", order = 1)]
    public class PlaceScriptableObjects : ScriptableObject
    {
        public const string DEFAULT_SERVER_PATH = "maxst";  //"lgseocho";

        public List<GameObject> trackableList = new();
        public List<GameObject> povList = new();

        [SerializeField] private SerializeStringDictionary<SpotMapping> spotMappingDic = new();
        [SerializeField] private string multiPlayerServerPath;
        [SerializeField] private SerializeEnumDictionary<ZoomLevel, ZoomRange> zoomRanges = new()
        {
            { ZoomLevel.level_1, new ZoomRange(0, 30) },
            { ZoomLevel.level_2, new ZoomRange(25, 80) },
            { ZoomLevel.level_3, new ZoomRange(70, 350) },
            { ZoomLevel.level_4, new ZoomRange(300, 550) },
            { ZoomLevel.level_5, new ZoomRange(500, 750) },
            { ZoomLevel.level_6, new ZoomRange(700, 9999) },
        };
        

        [field: SerializeField] public KnnStartPose KnnStartPose { get; set; }

        public SpotMapping FindPovObjName(string key)
        {
            return spotMappingDic.Find(key);
        }

        public string GetPovObjName(string key)
        {
            return spotMappingDic.Find(key)?.povName ?? "";
        }

        public string GetTrackableObjName(string key)
        {
            return spotMappingDic.Find(key)?.trackableName ?? "";
        }

        public IEnumerable<GameObject> IteratorTrackable()
        {
            foreach (var t in trackableList)
            {
                yield return t;
            }
        }

        public IEnumerable<GameObject> IteratorPov()
        {
            foreach (var p in povList)
            {
                yield return p;
            }
        }

        public IEnumerator ProcessMapping(GameObject XRStudio,
            GameObject TrackableRoot, Action complete = null)
        {
            yield return new WaitForEndOfFrame();
            var spotControllers = XRStudio.GetComponentsInChildren<SpotController>();
            var vpsTrackables = TrackableRoot.GetComponentsInChildren<VPSTrackable>();
            var result = false;
            foreach (var mapping in spotMappingDic.Values)
            {
                var spotController = Find(spotControllers, mapping.povName);
                var vpsTrackable = Find(vpsTrackables, mapping.trackableName);
                result = spotController != null && vpsTrackable != null;
                if (result) break;
            }
            //Debug.Log($"Find Spot : {result}");
            complete?.Invoke();
        }

        public bool ProcessMapping()
        {
            Dictionary<string, SpotController> spotControllerDict = povList.SelectMany(pov => pov.GetComponentsInChildren<SpotController>()).ToDictionary(sc => sc.gameObject.name);
            Dictionary<string, VPSTrackable> vpsTrackableDict = trackableList.SelectMany(vps => vps.GetComponentsInChildren<VPSTrackable>()).ToDictionary(vt => vt.gameObject.name);

            foreach (var mapping in spotMappingDic.Values)
            {
                if (spotControllerDict.ContainsKey(mapping.povName) && vpsTrackableDict.ContainsKey(mapping.trackableName))
                {
                    return true;
                }
            }
            return false;
        }


        private SpotController Find(SpotController[] controllers, string name)
        {
            foreach (var sc in controllers)
            {
                if (sc.gameObject.name == name) return sc;
            }
            return null;
        }

        private VPSTrackable Find(VPSTrackable[] trackables, string name)
        {
            foreach (var vt in trackables)
            {
                if (vt.gameObject.name == name) return vt;
            }
            return null;
        }

        public void ShowSelectedSpotObjs(GameObject XRStudio, GameObject TrackableRoot,
            Material ibrCullBackMaterial, Material ibrCullFrontMaterial,
            Spot spot, Action<SpotController> OnActiveSpotController = null)
        {
            string spotName = spot.vpsSpotName;
            var vpsTrackables = TrackableRoot.GetComponentsInChildren<VPSTrackable>();
            foreach (var vt in vpsTrackables)
            {
                vt.gameObject.SetActive(false);
            }
            var spotTrackableObj = Find(vpsTrackables, GetTrackableObjName(spotName))?.gameObject;
            spotTrackableObj?.SetActive(true);

            var spotControllers = XRStudio.GetComponentsInChildren<SpotController>();
            foreach (var sp in spotControllers)
            {
                sp.gameObject.SetActive(false);
            }
            var spotPOVObj = Find(spotControllers, GetPovObjName(spotName))?.gameObject;
            spotPOVObj?.SetActive(true);

            if (spotPOVObj != null)
            {
                OnActiveSpotController?.Invoke(spotPOVObj.GetComponent<SpotController>());
            }

            var sibr = XRStudio.GetComponentInChildren<SmoothIbrManager>();
            if (sibr) sibr.SetIbrMaterials(ibrCullBackMaterial, ibrCullFrontMaterial);

            if (spotTrackableObj != null)
            {
                XRStudio.GetComponentInChildren<PovManager>().Trackable = spotTrackableObj;
                //XRAPI.Instance.InitPlaceSpot();
                XRServiceManager.Instance(XRStudio).InitPlaceSpot();
            }

            //Debug.Log($"ShowSelectedSpotObjs {spotTrackableObj.gameObject.name}/{spotPOVObj.gameObject.name} ");
        }

        public void HidePov(GameObject XRStudio)
        {
            foreach (Transform childTransform in XRStudio.transform)
            {
                childTransform.gameObject.SetActive(false);
            }
        }

        public void NotifyTrackableInfo(GameObject TrackableRoot,
            Action<VPSMap, List<GameObject>, List<GameObject>, GameObject> trackableInfoCallback)
        {
            var vpsMap = TrackableRoot.GetComponentsInChildren<VPSMap>();
            foreach (var vm in vpsMap)
            {
                trackableInfoCallback?.Invoke(vm, vm.meshRoots, vm.groundRoots, vm.minimapSpriteObject);
            }
        }

        public void NotifySpotControllerInfo(GameObject XRStudio,
            Action<SpotController[]> povRootsCallback)
        {
            povRootsCallback?.Invoke(XRStudio.GetComponentsInChildren<SpotController>());
        }

        public string GetMultiPlayServerPath()
        {
            return string.IsNullOrEmpty(multiPlayerServerPath) ? DEFAULT_SERVER_PATH : multiPlayerServerPath;
        }

        public void ShowSelectedPovObjs(GameObject XRStudio,
            Material ibrCullBackMaterial, Material ibrCullFrontMaterial,
            string spotName, Action<SpotController> OnActiveSpotController = null)
        {
            var spotControllers = XRStudio.GetComponentsInChildren<SpotController>();
            foreach (var sp in spotControllers)
            {
                sp.gameObject.SetActive(false);
            }

            var sc = Find(spotControllers, GetPovObjName(spotName));
            if (sc)
            {
                sc.gameObject.SetActive(true);
                OnActiveSpotController?.Invoke(sc);
            }

            var sibr = XRStudio.GetComponentInChildren<SmoothIbrManager>();
            if (sibr) sibr.SetIbrMaterials(ibrCullBackMaterial, ibrCullFrontMaterial);

            //Debug.Log($"ShowSelectedSpotObjs {spotTrackableObj.gameObject.name}/{spotPOVObj.gameObject.name} ");
        }

        public void ShowSelectedMapObjs(GameObject XRStudio, GameObject TrackableRoot, string spotName)
        {
            var vpsTrackables = TrackableRoot.GetComponentsInChildren<VPSTrackable>();
            foreach (var vt in vpsTrackables)
            {
                vt.gameObject.SetActive(false);
            }
            var spotTrackableObj = Find(vpsTrackables, GetTrackableObjName(spotName))?.gameObject;
            spotTrackableObj?.SetActive(true);

            if (spotTrackableObj != null)
            {
                XRStudio.GetComponentInChildren<PovManager>().Trackable = spotTrackableObj;
                //XRAPI.Instance.InitPlaceSpot();
                XRServiceManager.Instance(XRStudio).InitPlaceSpot();
            }
        }

        public AbstractGroup[] AvailableZoomLevel(float visibleSize)
        {
            var ret = new AbstractGroup[Enum.GetValues(typeof(ZoomLevel)).Length + (int)ZoomLevel.level_1];
            foreach (var entry in zoomRanges)
            {
                if (!(entry.Value.max < visibleSize
                    || entry.Value.min > visibleSize))
                {
                    ret[(int)entry.Key] = new ChunkGroup((long)entry.Key);
                }
            }
            return ret;
        }
    }
}
