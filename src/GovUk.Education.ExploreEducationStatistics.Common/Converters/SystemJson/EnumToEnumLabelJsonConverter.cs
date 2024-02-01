using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;

public class EnumToEnumLabelJsonConverter<TEnum> : JsonConverter<TEnum> where TEnum : Enum
{
    public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return EnumUtil.GetFromLabel<TEnum>(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (value != null)
        {
            writer.WriteStringValue(value.GetEnumLabel());
        }
    }
}
