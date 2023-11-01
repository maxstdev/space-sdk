using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MaxstXR.Place
{
    public abstract class ChunkElementFilter
    {
        public static List<AbstractGroup> DefaultGroup { get; private set; } = new() { new ChunkGroup(WILDCARD_ID) };
        public const long WILDCARD_ID = -1L;

        public virtual bool UsedFilter { get; }
        public long UpdateTimestamp { get; protected set; } = TimeUtil.CurrentTimeMillis();

        protected ChunkElementFilter()
        {

        }

        public abstract void FilterValueInit();
        public abstract bool CheckVaild(List<AbstractGroup> filterType);
    }
}
