using Newtonsoft.Json;
using System;
using System.Globalization;

namespace Simplic.Data.Converter
{
    public class PreciseDecimalJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PreciseDecimal) || objectType == typeof(double);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<double?>(reader);
            if (value == null)
                return null;

            return new PreciseDecimal(value.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if(value == null)
                writer.WriteValue(default(PreciseDecimal));

            writer.WriteValue(((PreciseDecimal)value).ToDouble(null));
        }
    }
}