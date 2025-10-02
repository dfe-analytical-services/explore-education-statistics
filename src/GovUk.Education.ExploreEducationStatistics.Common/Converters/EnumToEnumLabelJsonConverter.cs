#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters;

public class EnumToEnumLabelJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : Enum
{
    public override void WriteJson(JsonWriter writer, TEnum? value, JsonSerializer serializer)
    {
        if (value != null)
        {
            writer.WriteValue(value.GetEnumLabel());
        }
    }

    public override TEnum ReadJson(
        JsonReader reader,
        Type objectType,
        TEnum? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        return EnumUtil.GetFromEnumLabel<TEnum>(reader.Value?.ToString() ?? string.Empty);
    }
}
