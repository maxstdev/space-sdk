using MaxstUtils;
using System;
using System.Collections.Generic;

namespace MaxstXR.Place
{
    public class PoIEvent
    {
        public readonly Event<Action<ChunkEnv>> PrevRenderEvent = new();
        public readonly Event<Action<ChunkEnv>> PostRenderEvent = new();

        public readonly LiveEvent<string, List<IPoint>> OnReceivePoints = new();
        public readonly LiveEvent<string, PointType> OnRemoveAllPointType = new();
        public readonly LiveEvent<string, PointType> OnHidePointType = new();
        public readonly LiveEvent<HashSet<AbstractGroup>> ChunkGroupEvent = new();
        public readonly LiveEvent<bool> poiVisible = new();

        protected PoIEvent()
        {

        }
    }
}
