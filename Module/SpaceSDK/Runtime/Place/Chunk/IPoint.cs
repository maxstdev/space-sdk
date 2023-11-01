using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaxstXR.Place
{
    public enum PointType : int
    {
        POI_TYPE = 0,
        ARROW_TYPE = 1,
        SIGN_3D_TYPE = 2,
        DEST_3D_TYPE = 3,
        MINIMAP_POI_TYPE = 4,
        MINIMAP_LOCAL_USER_TYPE= 5,
        MINIMAP_REMOTE_USER_TYPE = 6,
        MINIMAP_NAVI_LINE_TYPE = 7,
		POV_TYPE = 8,
    }

    public interface IPoint
    {
        ref Vector3 GetPosition();
        PointType GetPointType();
        List<AbstractGroup> GetGroups();
        bool IsIgnoreGroup();
        void SetRelationship(WeakReference<Chunk> wpChunk, ChunkDelegate chunkDelegate);
        bool OnRender(ChunkEnv env, bool isActive);
        bool OnEnable();
        bool OnInBounds();
        bool OnOutBounds();
        bool OnDispose();
    }
}
