#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;

public class EnumToEnumValueJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : Enum
{
    public override TEnum Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var value = reader.GetString();

        if (value is null)
        {
            throw new NullReferenceException("Enum value cannot be null");
        }

        return EnumUtil.GetFromEnumValue<TEnum>(value);
    }

    public override void Write(Utf8JsonWriter writer, TEnum? value, JsonSerializerOptions options)
    {
        if (value != null)
        {
            writer.WriteStringValue(value.GetEnumValue());
        }
    }
}
