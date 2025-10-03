#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.FilterHierarchiesOptionsUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters;

public class FilterHierarchiesOptionsConverter : JsonConverter<List<FilterHierarchyOptions>?>
{
    public override List<FilterHierarchyOptions>? ReadJson(
        JsonReader reader,
        Type objectType,
        List<FilterHierarchyOptions>? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var dictionary = serializer.Deserialize<IDictionary<Guid, List<FilterHierarchyOption>>>(reader);

        return dictionary
            ?.Select(kvp => new FilterHierarchyOptions { LeafFilterId = kvp.Key, Options = kvp.Value })
            .ToList();
    }

    public override void WriteJson(JsonWriter writer, List<FilterHierarchyOptions>? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var dictionary = FilterHierarchiesOptionsAsDictionary(value);

        serializer.Serialize(writer, dictionary);
    }
}
