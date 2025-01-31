using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters
{
    public class TimeIdentifierJsonConverter : JsonConverter<TimeIdentifier?>
    {
        public override void WriteJson(JsonWriter writer, TimeIdentifier? value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("value");
            writer.WriteValue(value.GetEnumValue());
            writer.WritePropertyName("label");
            writer.WriteValue(value.GetEnumLabel());
            writer.WriteEndObject();
        }

        public override TimeIdentifier? ReadJson(JsonReader reader, Type objectType, TimeIdentifier? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            var jsonValue = JObject.Load(reader).GetValue("value").Value<string>();
            var timeIdentifiers = ((TimeIdentifier[]) Enum.GetValues(typeof(TimeIdentifier)));
            return timeIdentifiers.First(identifier => identifier.GetEnumValue() == jsonValue);
        }
    }
}