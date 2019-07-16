using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters
{
    public class TimeIdentifierJsonConverter : JsonConverter<TimeIdentifier>
    {
        public override void WriteJson(JsonWriter writer, TimeIdentifier value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("value");
            writer.WriteValue(value.GetEnumValue());
            writer.WritePropertyName("label");
            writer.WriteValue(value.GetEnumLabel());
            writer.WriteEndObject();
        }

        public override TimeIdentifier ReadJson(JsonReader reader, Type objectType, TimeIdentifier existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotSupportedException("Currently ReadJson is not supported");
        }
    }
}