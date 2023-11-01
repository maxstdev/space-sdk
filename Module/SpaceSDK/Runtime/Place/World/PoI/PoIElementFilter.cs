using System.Collections.Generic;
using System.Linq;

namespace MaxstXR.Place
{
    public class PoIElementFilter : ChunkElementFilter
    {
        public HashSet<AbstractGroup> FilterTypeList { get; private set; } = null;
        public override bool UsedFilter => FilterTypeList != null;

        public PoIElementFilter() : base()
        {

        }

        public void UpdateFilterType(HashSet<AbstractGroup> filterTypeList)
        {
            FilterTypeList = filterTypeList;
            UpdateTimestamp = TimeUtil.CurrentTimeMillis();
        }

        public override bool CheckVaild(List<AbstractGroup> filterType)
        {
			return FilterTypeList == null || FilterTypeList.Intersect(filterType) != null;
        }

        public override void FilterValueInit()
        {
            FilterTypeList = null;
        }
    }
}
