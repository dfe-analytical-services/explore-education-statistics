using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;

public class ListJsonConverter<T, TConverter> : JsonConverter<IReadOnlyList<T>> 
    where TConverter : JsonConverter
{
    public override IReadOnlyList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        var jsonSerializerOptions = new JsonSerializerOptions(options);
        jsonSerializerOptions.Converters.Clear();
        jsonSerializerOptions.Converters.Add(Activator.CreateInstance<TConverter>());

        var _valueType = typeof(T);

        var list = new List<T>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return list;
            }

            var value = (T)JsonSerializer.Deserialize(ref reader, _valueType, jsonSerializerOptions);

            list.Add(value);
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<T> value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var jsonSerializerOptions = new JsonSerializerOptions(options);
        jsonSerializerOptions.Converters.Clear();
        jsonSerializerOptions.Converters.Add(Activator.CreateInstance<TConverter>());

        writer.WriteStartArray();

        foreach (T item in value)
        {
            JsonSerializer.Serialize(writer, item, jsonSerializerOptions);
        }

        writer.WriteEndArray();
    }
}
