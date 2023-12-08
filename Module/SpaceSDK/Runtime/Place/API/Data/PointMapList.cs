using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace MaxstXR.Place
{
    [Serializable]
    public class PointMapList
    {
        [JsonProperty("id")] public string id;
        [JsonProperty("seq")] public string seq;
        [JsonProperty("point_coordinate")] public PointCoordinate pointCoordinate;
    }

    public class PointCoordinate
    {
        [JsonProperty("pick_point")] public PickPoint pickPoint;
        [JsonProperty("map")] public Map map;
    }

    public class PickPoint
    {
        [JsonProperty("x")] public float x;
        [JsonProperty("y")] public float y;
        [JsonProperty("z")] public float z;
    }

    public class Map
    {
        [JsonProperty("x")] public float x;
        [JsonProperty("y")] public float y;
    }
}
