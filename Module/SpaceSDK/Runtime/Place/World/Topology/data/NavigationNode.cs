using JsonFx.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaxstXR.Place
{
    [Serializable]
    public class NavigationNode
    {
        [JsonProperty("toNodes")] public List<Guid> toNodes;
        [JsonProperty("position")] public JVector3 position;
        [JsonProperty("name")] public string name;
        [JsonProperty("guid")] public Guid guid;

        public List<Linker> GetLinkers()
        {
            var isEmptyConnectedNode = toNodes == null || toNodes.Count == 0;
            if (isEmptyConnectedNode)
            {
                return null;
            }

            List<Linker> linkers = toNodes.Select(iter =>
            {
                var linker = new Linker();
                linker.FromGUID = guid;
                linker.ToGUID = iter;
                linker.linkerType = LinkerType.OneWay;
                return linker;
            }).ToList();
            return linkers;
        }
    }
}
