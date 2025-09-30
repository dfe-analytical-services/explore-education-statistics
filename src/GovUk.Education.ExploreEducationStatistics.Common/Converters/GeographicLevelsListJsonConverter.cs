#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters;

public class GeographicLevelsListJsonConverter : JsonConverter<IList<GeographicLevel>>
{
    public override void WriteJson(
        JsonWriter writer,
        IList<GeographicLevel>? value,
        JsonSerializer serializer
    )
    {
        writer.WriteStartArray();
        if (value != null)
        {
            foreach (var gl in value)
            {
                writer.WriteValue(gl.GetEnumValue());
            }
        }
        writer.WriteEndArray();
    }

    public override List<GeographicLevel>? ReadJson(
        JsonReader reader,
        Type objectType,
        IList<GeographicLevel>? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var geographicLevels = (GeographicLevel[])Enum.GetValues(typeof(GeographicLevel));
        var result = new List<GeographicLevel>();

        while (reader.Read() && reader.TokenType == JsonToken.String)
        {
            var value = (string)reader.Value;
            var geographicLevel = geographicLevels.First(gl => gl.GetEnumValue() == value);
            result.Add(geographicLevel);
        }

        if (reader.TokenType != JsonToken.EndArray)
        {
            return null;
        }

        return result;
    }
}
