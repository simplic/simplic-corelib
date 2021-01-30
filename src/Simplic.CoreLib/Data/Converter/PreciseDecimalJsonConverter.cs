using Newtonsoft.Json;
using System;

namespace Simplic.Data.Converter
{
    /// <summary>
    /// Json precise decimal converter
    /// </summary>
    public class PreciseDecimalJsonConverter : JsonConverter<PreciseDecimal>
    {
        /// <summary>
        /// Write to json
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, PreciseDecimal value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToDouble(null));
        }

        /// <summary>
        /// Read from json
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="hasExistingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override PreciseDecimal ReadJson(JsonReader reader, Type objectType, PreciseDecimal existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return default(PreciseDecimal);

            var s = (double)reader.Value;
            return new PreciseDecimal(s);
        }
    }
}