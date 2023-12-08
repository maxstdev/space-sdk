using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace MaxstXR.Place
{
    public abstract class SpaceDatasProperty
    {
        public abstract List<Space> SpaceList { get; }
    }

    public abstract class SpaceDataProperty
    {
        public abstract string SpaceId { get; }
        public abstract string SpaceName { get; }
    }

    [Serializable]
    public class SpaceDatas : SpaceDatasProperty
    {
        [JsonProperty("space_list")] public List<Space> spaceList;
        [JsonProperty("page")] public int page;
        [JsonProperty("size")] public int size;
        [JsonProperty("sort_by")] public string sortBy;
        [JsonProperty("sort_direction")] public string sortDirection;
        [JsonProperty("total_pages")] public int totalPages;
        [JsonProperty("total_elements")] public int totalElements;

        [JsonIgnore] public override List<Space> SpaceList => spaceList;
    }
    [Serializable]
    public class Space : SpaceDataProperty
    {
        [JsonProperty("space_id")] public string spaceId;
        [JsonProperty("creator_email")] public string creatorEmail;
        [JsonProperty("name")] public string name;                          // Sapce Name(Not Unique)
        [JsonProperty("space_status")] public string spaceStatus;           // Enum???
        [JsonProperty("publishing_status")] public string publishingStatus; // Enum???
        [JsonProperty("image_url")] public string imageUrl;
        [JsonProperty("type")] public string type;                          // Enum???
        [JsonProperty("address")] public string address;
        [JsonProperty("description")] public string description;
        [JsonProperty("created_at")] public string createdAt;
        //[JsonProperty("created_at")]
        //[JsonConverter(typeof(DateTimeConverter))] public DateTime createdAt;
        [JsonProperty("point_map_list")] public List<PointMapList> pointMapList;
        [JsonProperty("deploy_response")] public List<DeployResponse> deployResponse;
        [JsonProperty("learn_section")] public LearnSection learnSection;
        [JsonProperty("initial_point")]
        [JsonConverter(typeof(GeometryConverter))] public Geometry initialPoint;
        [JsonProperty("display_point")]
        [JsonConverter(typeof(GeometryConverter))] public Geometry displayPoint;

        [JsonIgnore] public override string SpaceName => name ?? string.Empty;
        [JsonIgnore] public override string SpaceId => spaceId;
    }

    public class DeployResponse
    {
        [JsonProperty("deploy_id")] public int deployId;
        [JsonProperty("deploy_type")] public string deployType;
        [JsonProperty("deploy_status")] public string deployStatus;
    }

    public class LearnSection
    {
        [JsonProperty("workflow_id")] public int workflowId;
        [JsonProperty("workflow_phase")] public string workflowPhase;
    }

    public class SpaceTextureUrl
    {
        [JsonProperty("pre_signed_url")] public string url;
    }

}
