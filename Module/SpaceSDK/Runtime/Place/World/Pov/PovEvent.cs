using MaxstUtils;
using System.Collections.Generic;

namespace MaxstXR.Place
{
    public class PovEvent
    {
        public readonly LiveEvent<SpotController> receiveSpotController = new();
        public readonly LiveEvent<List<IPoint>> receivePoints = new();
        public readonly LiveEvent<PointType> removeAllPointType = new();
        public readonly LiveEvent<bool> povVisible = new();

        protected PovEvent()
        {

        }
    }
}