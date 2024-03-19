using System.Text.Json;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels.Converters;

public class LocationOptionMetaJsonConverter : JsonConverter<LocationOptionMetaViewModel>
{
    public override bool CanConvert(Type type)
    {
        return type.IsAssignableFrom(typeof(LocationOptionMetaViewModel));
    }

    public override LocationOptionMetaViewModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!JsonDocument.TryParseValue(ref reader, out var doc))
        {
            throw new JsonException("Failed to parse JsonDocument");
        }

        var rootElement = doc.RootElement.GetRawText();

        var propertyNames = doc.RootElement
            .EnumerateObject()
            .Select(p => char.ToUpper(p.Name[0]) + p.Name[1..])
            .ToHashSet();

        if (propertyNames.Contains(nameof(LocationSchoolOptionMetaViewModel.Urn))
            && propertyNames.Contains(nameof(LocationSchoolOptionMetaViewModel.LaEstab)))
        {
            return JsonSerializer.Deserialize<LocationSchoolOptionMetaViewModel>(rootElement, options)!;
        }

        if (propertyNames.Contains(nameof(LocationProviderOptionMetaViewModel.Ukprn)))
        {
            return JsonSerializer.Deserialize<LocationProviderOptionMetaViewModel>(rootElement, options)!;
        }

        if (propertyNames.Contains(nameof(LocationLocalAuthorityOptionMetaViewModel.Code))
            && propertyNames.Contains(nameof(LocationLocalAuthorityOptionMetaViewModel.OldCode)))
        {
            return JsonSerializer.Deserialize<LocationLocalAuthorityOptionMetaViewModel>(rootElement, options)!;
        }

        if (propertyNames.Contains(nameof(LocationCodedOptionMetaViewModel.Code)))
        {
            return JsonSerializer.Deserialize<LocationCodedOptionMetaViewModel>(rootElement, options)!;
        }

        return JsonSerializer.Deserialize<LocationRscRegionOptionMetaViewModel>(rootElement, options)!;
    }

    public override void Write(Utf8JsonWriter writer, LocationOptionMetaViewModel? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var jsonSerializerOptions = new JsonSerializerOptions(options)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        switch (value)
        {
            case LocationSchoolOptionMetaViewModel:
                JsonSerializer.Serialize(writer, value, typeof(LocationSchoolOptionMetaViewModel), jsonSerializerOptions);
                break;
            case LocationRscRegionOptionMetaViewModel:
                JsonSerializer.Serialize(writer, value, typeof(LocationRscRegionOptionMetaViewModel), jsonSerializerOptions);
                break;
            case LocationProviderOptionMetaViewModel:
                JsonSerializer.Serialize(writer, value, typeof(LocationProviderOptionMetaViewModel), jsonSerializerOptions);
                break;
            case LocationLocalAuthorityOptionMetaViewModel:
                JsonSerializer.Serialize(writer, value, typeof(LocationLocalAuthorityOptionMetaViewModel), jsonSerializerOptions);
                break;
            case LocationCodedOptionMetaViewModel:
                JsonSerializer.Serialize(writer, value, typeof(LocationCodedOptionMetaViewModel), jsonSerializerOptions);
                break;
            default:
                throw new JsonException();
        }
    }
}
