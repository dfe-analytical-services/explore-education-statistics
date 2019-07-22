using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters
{
    public class TimeIdentifierCategoryJsonConverter : JsonConverter<TimeIdentifierCategory?>
    {
        public override void WriteJson(JsonWriter writer, TimeIdentifierCategory? value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("value");
            writer.WriteValue(value.GetEnumValue());
            writer.WritePropertyName("label");
            writer.WriteValue(value.GetEnumLabel());
            writer.WriteEndObject();
        }

        public override TimeIdentifierCategory? ReadJson(JsonReader reader, Type objectType,
            TimeIdentifierCategory? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            
            var jsonValue = JObject.Load(reader).GetValue("value").Value<string>();
            var categories = ((TimeIdentifierCategory[]) Enum.GetValues(typeof(TimeIdentifierCategory)));
            return categories.First(category => category.GetEnumValue() == jsonValue);
        }
    }
}