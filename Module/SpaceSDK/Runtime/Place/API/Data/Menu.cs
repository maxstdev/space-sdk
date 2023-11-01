using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MaxstXR.Place
{
    [Serializable]
    public class Menu
    {
        [JsonProperty("ko")]
        //[JsonConverter(typeof(MenuConverter))]
        //public IDictionary<string, string> ko;
        public string[] ko;
        [JsonProperty("en")]
        //[JsonConverter(typeof(MenuConverter))]
        //public IDictionary<string, string> en;
        public string[] en;
    }

    public class MenuConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {   
            try
            {
                var s = reader.Value.ToString();
                //Debug.Log($"ReadJson {objectType.FullName} : {s}");
                return JsonConvert.DeserializeObject<IDictionary<string, string>>(s);
            }
            catch (Exception)
            {
                //Debug.Log($"ReadJson Exception : {e.Message}");
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var s = JsonConvert.SerializeObject(value);
            //Debug.Log($"WriteJson {value.GetType().FullName} : {wrapper}");
            writer.WriteValue(s);
        }
    }
}
