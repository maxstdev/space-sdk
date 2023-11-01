using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaxstXR.Place
{
    public enum GeometryType
    {
        Point,
        LineString,
        Polygon
    }

    [Serializable]
    public class Geometry
    {
        //[JsonProperty("type")] 
        public GeometryType type;
        //[JsonProperty("coordinates")] 
        public Coordinates coordinates;
    }

    public class GeometryConverter : JsonConverter<Geometry>
    {
        public override Geometry ReadJson(JsonReader reader, Type objectType, 
            Geometry existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                return null;

            try
            {
                var geometry = new Geometry();
                JObject jObject = JObject.Load(reader);
                var isType = false;
                if (jObject.TryGetValue("type", out var type))
                {
                    //Debug.Log($"GeometryConverter type : {type}");
                    isType = Enum.TryParse(type?.ToString() ?? "", true, out geometry.type);
                }

                if (isType && jObject.TryGetValue("coordinates", out var coordinates))
                {
                    //Debug.Log($"GeometryConverter coordinates : {coordinates}");
                    if (coordinates != null)
                    {
                        geometry.coordinates = geometry.type switch
                        {
                            GeometryType.Point => new Point 
                            { 
                                pos = JsonConvert.DeserializeObject<List<float>>(coordinates.ToString()) 
                            },
                            GeometryType.LineString => new LineString 
                            { 
                                pos = JsonConvert.DeserializeObject<List<List<float>>>(coordinates.ToString()) 
                            },
                            GeometryType.Polygon => new Polygon 
                            { 
                                pos = JsonConvert.DeserializeObject<List<List<List<float>>>>(coordinates.ToString()) 
                            },
                            _ => null
                        };
                        //Debug.Log("GeometryConverter coordinates : " + JsonConvert.SerializeObject(geometry.coordinates));
                    }
                }
                return geometry;
            }
            catch(Exception e)
            {
                Debug.LogWarning($"GeometryConverter {e}");
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, Geometry value, JsonSerializer serializer)
        {
            writer.WriteValue(value.coordinates);
        }
    }
}
