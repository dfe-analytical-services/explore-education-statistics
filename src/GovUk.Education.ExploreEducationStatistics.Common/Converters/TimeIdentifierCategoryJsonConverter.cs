using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters
{
    public class TimeIdentifierCategoryJsonConverter : JsonConverter<TimeIdentifierCategory>
    {
        public override void WriteJson(JsonWriter writer, TimeIdentifierCategory value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("value");
            writer.WriteValue(value.GetEnumValue());
            writer.WritePropertyName("label");
            writer.WriteValue(value.GetEnumLabel());
            writer.WriteEndObject();
        }

        public override TimeIdentifierCategory ReadJson(JsonReader reader, Type objectType,
            TimeIdentifierCategory existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotSupportedException("Currently ReadJson is not supported");
        }
    }
}