using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaxstXR.Place
{
    public class TopologyViewModel : Injector
    {
        [DI(DIScope.component, DIComponent.place)] private CustomerRepo CustomerRepo { get; }
        public TopologyAPIServiceHelper TopologyAPIServiceHelper { get; set; }
        protected TopologyViewModel()
        {

        }

        public void Awake(GameObject parent)
        {
            TopologyAPIServiceHelper = TopologyAPIServiceHelper.Build(parent);
        }

        /*public async UniTask LoadTopologyAsync(long spotId, Action<List<LineData>> success)
        {
            string topology;
            string cachedData = TopologyDataHelper.LoadData(spotId);
            if (cachedData != null)
            {
                topology = cachedData;
            }
            else
            {
                var topologyJson = await GetSpotTopologyAsync(spotId);
                topology = topologyJson;
            }
            var topologyData = JsonConvert.DeserializeObject<TopologyNavigationData>(topology);
            var lineDatas = GetLineDataFromLinker(topologyData.GetLinkers(), topologyData.GetNodesDictionary());
            success?.Invoke(lineDatas);
        }*/

        private List<LineData> GetLineDataFromLinker(List<Linker> linkers, Dictionary<Guid, NavigationNode> dictionary)
        {
            return linkers.ConvertAll(linker =>
            new LineData(dictionary[linker.FromGUID].position, dictionary[linker.ToGUID].position));
        }

        /*private async UniTask<string> GetSpotTopologyAsync(long spotId)
        {
            CredentialsToken credentialsToken = await CustomerRepo.GetAuthoringToken();
            var result = await TopologyAPIServiceHelper.GetTopologyData(credentialsToken.Authorization, spotId);
            TopologyDataHelper.SaveData(result, spotId);
            return result;
        }*/
    }
}