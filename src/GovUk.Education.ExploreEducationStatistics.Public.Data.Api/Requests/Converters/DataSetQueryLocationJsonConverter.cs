using System.Text.Json;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests.Converters;

public class DataSetQueryLocationJsonConverter : JsonConverter<IDataSetQueryLocation>
{
    public override bool CanConvert(Type type)
    {
        return type.IsAssignableFrom(typeof(IDataSetQueryLocation));
    }

    public override IDataSetQueryLocation? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (!JsonDocument.TryParseValue(ref reader, out var doc))
        {
            throw new JsonException();
        }

        var rootElement = doc.RootElement.GetRawText();

        var propertyNames = doc.RootElement.EnumerateObject().Select(p => p.Name.ToUpperFirst()).ToHashSet();

        if (!doc.RootElement.TryGetProperty(nameof(IDataSetQueryLocation.Level).ToLowerFirst(), out var levelProperty))
        {
            throw new JsonException();
        }

        try
        {
            var levelString = levelProperty.GetString() ?? string.Empty;

            if (EnumUtil.TryGetFromEnumValue<GeographicLevel>(levelString, out var level))
            {
                switch (level)
                {
                    case GeographicLevel.LocalAuthority
                        when propertyNames.Contains(nameof(DataSetQueryLocationLocalAuthorityCode.Code)):
                        return JsonSerializer.Deserialize<DataSetQueryLocationLocalAuthorityCode>(rootElement, options);

                    case GeographicLevel.LocalAuthority
                        when propertyNames.Contains(nameof(DataSetQueryLocationLocalAuthorityOldCode.OldCode)):
                        return JsonSerializer.Deserialize<DataSetQueryLocationLocalAuthorityOldCode>(
                            rootElement,
                            options
                        );

                    case GeographicLevel.School
                        when propertyNames.Contains(nameof(DataSetQueryLocationSchoolLaEstab.LaEstab)):
                        return JsonSerializer.Deserialize<DataSetQueryLocationSchoolLaEstab>(rootElement, options);

                    case GeographicLevel.School when propertyNames.Contains(nameof(DataSetQueryLocationSchoolUrn.Urn)):
                        return JsonSerializer.Deserialize<DataSetQueryLocationSchoolUrn>(rootElement, options);

                    case GeographicLevel.Provider
                        when propertyNames.Contains(nameof(DataSetQueryLocationProviderUkprn.Ukprn)):
                        return JsonSerializer.Deserialize<DataSetQueryLocationProviderUkprn>(rootElement, options);
                }
            }

            if (propertyNames.Contains(nameof(DataSetQueryLocationCode.Code)))
            {
                return JsonSerializer.Deserialize<DataSetQueryLocationCode>(rootElement, options);
            }

            return JsonSerializer.Deserialize<DataSetQueryLocationId>(rootElement, options);
        }
        catch (JsonException exception)
        {
            throw new JsonException(message: null, exception);
        }
    }

    public override void Write(Utf8JsonWriter writer, IDataSetQueryLocation value, JsonSerializerOptions options)
    {
        try
        {
            switch (value)
            {
                case DataSetQueryLocationLocalAuthorityCode:
                    JsonSerializer.Serialize(writer, value, typeof(DataSetQueryLocationLocalAuthorityCode), options);
                    return;
                case DataSetQueryLocationLocalAuthorityOldCode:
                    JsonSerializer.Serialize(writer, value, typeof(DataSetQueryLocationLocalAuthorityOldCode), options);
                    return;
                case DataSetQueryLocationProviderUkprn:
                    JsonSerializer.Serialize(writer, value, typeof(DataSetQueryLocationProviderUkprn), options);
                    return;
                case DataSetQueryLocationSchoolLaEstab:
                    JsonSerializer.Serialize(writer, value, typeof(DataSetQueryLocationSchoolLaEstab), options);
                    return;
                case DataSetQueryLocationSchoolUrn:
                    JsonSerializer.Serialize(writer, value, typeof(DataSetQueryLocationSchoolUrn), options);
                    return;
                case DataSetQueryLocationCode:
                    JsonSerializer.Serialize(writer, value, typeof(DataSetQueryLocationCode), options);
                    return;
                case DataSetQueryLocationId:
                    JsonSerializer.Serialize(writer, value, typeof(DataSetQueryLocationId), options);
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
