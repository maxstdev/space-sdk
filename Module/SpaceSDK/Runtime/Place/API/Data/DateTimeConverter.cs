using Newtonsoft.Json.Converters;

namespace MaxstXR.Place
{
    class DateTimeConverter : IsoDateTimeConverter
    {
        public DateTimeConverter()
        {
            //2022-01-12T14:34:09
            base.DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss";
        }
    }
}
