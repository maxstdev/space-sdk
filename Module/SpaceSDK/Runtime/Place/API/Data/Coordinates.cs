using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace MaxstXR.Place
{
    [Serializable]
    public abstract class Coordinates
    {
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public GeometryType type;

        public Coordinates(GeometryType type)
        {
            this.type = type;
        }

        [JsonIgnore] public abstract List<List<List<float>>> Pos { get; }
    }

    [Serializable]
    public class Point : Coordinates
    {
        [JsonProperty("coordinates")] public List<float> pos;

        public Point() : base(GeometryType.Point)
        {
        }

        [JsonIgnore]
        public override List<List<List<float>>> Pos => new()
        {
            new List<List<float>>() 
            {
                pos
            }
        };
    }

    [Serializable]
    public class LineString : Coordinates
    {
        [JsonProperty("coordinates")] public List<List<float>> pos;

        public LineString() : base(GeometryType.LineString)
        {
        }

        [JsonIgnore]
        public override List<List<List<float>>> Pos => new()
        {
            pos
        };
    }

    [Serializable]
    public class Polygon : Coordinates
    {
        [JsonProperty("coordinates")] public List<List<List<float>>> pos;

        public Polygon() : base(GeometryType.Polygon)
        {
        }

        [JsonIgnore]
        public override List<List<List<float>>> Pos => pos;
    }
}