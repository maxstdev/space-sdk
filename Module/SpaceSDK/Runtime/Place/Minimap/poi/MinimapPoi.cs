using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaxstXR.Place
{
    public class MinimapPoi : AbstractGroup, IPoint
    {
        private Vector3 pos = Vector3.zero;
        private PoiPromise poi;

        private GameObject obj = null;
        private GameObject parent = null;
        private WeakReference<Chunk> wpChunk = null;
        private ChunkDelegate chunkDelegate = null;
        private bool isDest = false;

        public MinimapPoi(PoiPromise poi, GameObject parent)
            : base()
        {
            this.poi = poi;
            this.parent = parent;
            pos = poi.GetVpsPosition();
            poi.OnDestination.AddObserver(this, OnDestination);
        }

        ~MinimapPoi()
        {
            poi.OnDestination.RemoveAllObserver(this);
        }

        public PointType GetPointType()
        {
            return PointType.MINIMAP_POI_TYPE;
        }

        public List<AbstractGroup> GetGroups()
        {
            return new() { this };
        }

        public bool IsIgnoreGroup()
        {
            return isDest;
        }

        public void SetRelationship(WeakReference<Chunk> wpChunk, ChunkDelegate chunkDelegate)
        {
            this.wpChunk = wpChunk;
            this.chunkDelegate = chunkDelegate;
        }

        public ref Vector3 GetPosition()
        {
            return ref pos;
        }

        public bool OnDispose()
        {
            ReleaseObject();
            return true;
        }

        public bool OnEnable()
        {
            CreateObject();
            obj?.SetActive(false);
            return true;
        }

        public bool OnInBounds()
        {
            CreateObject();
            obj?.SetActive(false);
            return true;
        }

        public bool OnOutBounds()
        {
            ReleaseObject();
            return true;
        }

        public bool OnRender(ChunkEnv env, bool isRenderStatus)
        {
            if (obj != null)
            {
                obj.SetActive(isRenderStatus);
                //obj.transform.localRotation = env.RelativeRotate();
                //obj.transform.localScale = env.RelativeScale();
            }
            return true;
        }

        private void CreateObject()
        {
            //if (!poi.ExtensionObject?.isMinimapAvailable ?? false) return;

            if (obj == null)
            {
                obj = GameObject.Instantiate(PlaceResources.Instance(parent).MinimapPoI, parent.transform);
                //Debug.Log("CreateObject : " + pathDetail.PosPtr);
                var p = poi.GetVpsPosition();
                p.y = MinimapCamera.MINIMAP_POI_POS_Y;
                obj.transform.localPosition = p;
                obj.SetActive(false);
                obj.name = poi.PoiName;
                obj.layer = PlaceResources.Instance(parent).MinimapLayer;
                var ts = obj.GetComponentsInChildren<Transform>();
                foreach (var t in ts)
                {
                    t.gameObject.layer = PlaceResources.Instance(parent).MinimapLayer;
                }
                UpdateContent(poi.OnDestination.Value);
            }
        }

        private void ReleaseObject()
        {
            if (obj != null)
            {
                GameObject.Destroy(obj);
                obj = null;
            }
        }

        private void UpdateContent(bool isDest)
        {
            if (obj != null)
            {
                var behaviour = obj.GetComponent<MinimapPoiBehaviour>();
                behaviour.UpdateContent(isDest, poi);
            }
        }

        public override long GetGroupId()
        {
            return poi.ViewLevel();
        }

        private void OnDestination(bool isDest)
        {
            UpdateContent(isDest);
            if (this.isDest != isDest)
            {
                this.isDest = isDest;
                NotifyGroupCondition();
            }
        }

        private void NotifyGroupCondition()
        {
            Chunk chunk = null;
            if (wpChunk?.TryGetTarget(out chunk) ?? false)
            {
                chunkDelegate?.UpdateGroupCondition(chunk, this);
            }
        }
    }
}
