using MaxstUtils;
using System.Collections.Generic;

namespace MaxstXR.Place
{
    public class MinimapPoiEvent
    {
        public LiveEvent<string, List<IPoint>> OnReceivePoints = new LiveEvent<string, List<IPoint>>();
        public LiveEvent<string, PointType> OnRemoveAllPointType = new LiveEvent<string, PointType>();
        public LiveEvent<AbstractGroup[]> OnReceiveChunkGroups = new LiveEvent<AbstractGroup[]>();

        protected MinimapPoiEvent()
        {

        }
    }
}
