using System.Text.Json;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests.Converters;

public class DataSetQueryCriteriaJsonConverter : JsonConverter<IDataSetQueryCriteria>
{
    public override bool CanConvert(Type type)
    {
        return type.IsAssignableFrom(typeof(IDataSetQueryCriteria));
    }

    public override IDataSetQueryCriteria? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (!JsonDocument.TryParseValue(ref reader, out var doc))
        {
            throw new JsonException();
        }

        try
        {
            var rootElement = doc.RootElement.GetRawText();

            var propertyNames = doc.RootElement.EnumerateObject().Select(p => p.Name.ToUpperFirst()).ToHashSet();

            if (propertyNames.Contains(nameof(DataSetQueryCriteriaAnd.And)))
            {
                return JsonSerializer.Deserialize<DataSetQueryCriteriaAnd>(rootElement, options);
            }

            if (propertyNames.Contains(nameof(DataSetQueryCriteriaOr.Or)))
            {
                return JsonSerializer.Deserialize<DataSetQueryCriteriaOr>(rootElement, options);
            }

            if (propertyNames.Contains(nameof(DataSetQueryCriteriaNot.Not)))
            {
                return JsonSerializer.Deserialize<DataSetQueryCriteriaNot>(rootElement, options);
            }

            return JsonSerializer.Deserialize<DataSetQueryCriteriaFacets>(rootElement, options);
        }
        catch (JsonException exception)
        {
            throw new JsonException(message: null, exception);
        }
    }

    public override void Write(Utf8JsonWriter writer, IDataSetQueryCriteria value, JsonSerializerOptions options)
    {
        try
        {
            switch (value)
            {
                case DataSetQueryCriteriaAnd:
                    JsonSerializer.Serialize(writer, value, typeof(DataSetQueryCriteriaAnd), options);
                    return;
                case DataSetQueryCriteriaOr:
                    JsonSerializer.Serialize(writer, value, typeof(DataSetQueryCriteriaOr), options);
                    return;
                case DataSetQueryCriteriaNot:
                    JsonSerializer.Serialize(writer, value, typeof(DataSetQueryCriteriaNot), options);
                    return;
                case DataSetQueryCriteriaFacets:
                    JsonSerializer.Serialize(writer, value, typeof(DataSetQueryCriteriaFacets), options);
                    return;
            }
        }
        catch (JsonException exception)
        {
            throw new JsonException(message: null, exception);
        }

        throw new JsonException();
    }
}
