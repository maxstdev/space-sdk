using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MaxstXR.Place
{
    public class MinimapElementFilter : ChunkElementFilter
    {
        public AbstractGroup[] FilterTypeArray { get; set; } = null;
        public override bool UsedFilter => FilterTypeArray != null;

        public MinimapElementFilter() : base()
        {

        }

        public void UpdateFilterType(AbstractGroup[] filterTypeArray)
        {
            FilterTypeArray = filterTypeArray;
            UpdateTimestamp = TimeUtil.CurrentTimeMillis();
        }

        public override bool CheckVaild(List<AbstractGroup> filterType)
        {
            return FilterTypeArray == null? true : FilterTypeArray[filterType[0].GetGroupId()] != null;
        }

        public override void FilterValueInit()
        {
            FilterTypeArray = null;
        }
    }
}
