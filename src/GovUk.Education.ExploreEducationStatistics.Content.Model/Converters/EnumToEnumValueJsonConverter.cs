using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Converters
{
    public class EnumToEnumValueJsonConverter<TEnum> : JsonConverter<TEnum> where TEnum : Enum
    {
        public override void WriteJson(JsonWriter writer, TEnum value, JsonSerializer serializer)
        {
            if (value != null)
            {
                writer.WriteValue(value.GetEnumValue());
            }
        }

        public override TEnum ReadJson(JsonReader reader, Type objectType, TEnum existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var value = reader.Value;
            foreach (var val in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
            {
                if (val.GetEnumValue().Equals(value))
                {
                    return val;
                }
            }

            throw new ArgumentException("The value '" + value + "' is not supported.");
        }
    }
}