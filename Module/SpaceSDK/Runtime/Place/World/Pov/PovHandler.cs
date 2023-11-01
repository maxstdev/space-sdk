using System.Collections.Generic;
using System;
using UnityEngine;

namespace MaxstXR.Place
{
    public class PovHandler : AbstractGroup, IPoint
    {
        private Vector3 pos = Vector3.zero;

        private GameObject pov = null;
        private WeakReference<Chunk> wpChunk = null;
        private ChunkDelegate chunkDelegate = null;
        private bool isVisible = false;

        public PovHandler(GameObject pov)
            : base()
        {
            pos = pov.transform.position;
            this.pov = pov;
        }

        public PointType GetPointType()
        {
            return PointType.POV_TYPE;
        }

        public List<AbstractGroup> GetGroups()
        {
            return new() { this };
        }

        public bool IsIgnoreGroup()
        {
            return false;
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
            isVisible = false;
            ProcessVisible();
            return true;
        }

        public bool OnEnable()
        {
            isVisible = true;
            ProcessVisible();
            return true;
        }

        public bool OnInBounds()
        {
            isVisible = true;
            ProcessVisible();
            return true;
        }

        public bool OnOutBounds()
        {
            isVisible = false;
            ProcessVisible();
            return true;
        }

        public bool OnRender(ChunkEnv env, bool isRenderStatus)
        {
            ProcessVisible();
            return true;
        }

        public void ProcessVisible()
        {
            pov?.SetActive(isVisible);
        }

        public override long GetGroupId()
        {
            return -1L;
        }
    }
}