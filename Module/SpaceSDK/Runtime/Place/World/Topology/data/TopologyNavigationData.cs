using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaxstXR.Place
{
    [Serializable]
    public class TopologyNavigationData
    {
        [JsonProperty("pois")] public List<NavigationNode> pois;
        [JsonProperty("nodes")] public List<NavigationNode> nodes;

        public Dictionary<Guid, NavigationNode> GetNodesDictionary()
        {
            return pois.Concat(nodes).ToDictionary(keySelector: node => node.guid);
        }

        public List<Linker> GetLinkers()
        {
            var linkers = new List<Linker>();
            var linkableNodes = pois.Concat(nodes);
            var linkableNodesByGuid = new Dictionary<Guid, NavigationNode>();

            foreach (var node in linkableNodes)
            {
                var nodeLinkers = node.GetLinkers();
                if (nodeLinkers == null)
                {
                    continue;
                }

                nodeLinkers = nodeLinkers
                    .Where(linker =>
                    {
                        var toNodeID = linker.ToGUID;
                        var isProcessed = linkableNodesByGuid.ContainsKey(toNodeID);
                        if (!isProcessed)
                        {
                            return true;
                        }

                        var isTwoWayLink = linkableNodesByGuid[toNodeID].toNodes.Contains(node.guid);
                        if (isTwoWayLink)
                        {
                            var twoWayLinker = linkers.Find(iter =>
                                iter.FromGUID == toNodeID && iter.ToGUID == node.guid);
                            twoWayLinker.linkerType = LinkerType.TwoWay;
                            return false;
                        }

                        return true;
                    }).ToList();
                linkers.AddRange(nodeLinkers);
                linkableNodesByGuid.Add(node.guid, node);
            }
            return linkers;
        }
    }
}
