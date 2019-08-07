using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters
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
            return EnumUtil.GetFromString<TEnum>(reader.Value.ToString());
        }
    }
}