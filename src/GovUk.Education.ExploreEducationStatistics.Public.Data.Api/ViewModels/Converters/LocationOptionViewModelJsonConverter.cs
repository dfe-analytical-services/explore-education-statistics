using System.Text.Json;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels.Converters;

public class LocationOptionViewModelJsonConverter : JsonConverter<LocationOptionViewModel>
{
    public override bool CanConvert(Type type)
    {
        return type.IsAssignableFrom(typeof(LocationOptionViewModel));
    }

    public override LocationOptionViewModel Read(
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

            var propertyNames = doc
                .RootElement.EnumerateObject()
                .Select(p => char.ToUpper(p.Name[0]) + p.Name[1..])
                .ToHashSet();

            if (
                propertyNames.Contains(nameof(LocationSchoolOptionViewModel.Urn))
                && propertyNames.Contains(nameof(LocationSchoolOptionViewModel.LaEstab))
            )
            {
                return JsonSerializer.Deserialize<LocationSchoolOptionViewModel>(rootElement, options)!;
            }

            if (propertyNames.Contains(nameof(LocationProviderOptionViewModel.Ukprn)))
            {
                return JsonSerializer.Deserialize<LocationProviderOptionViewModel>(rootElement, options)!;
            }

            if (
                propertyNames.Contains(nameof(LocationLocalAuthorityOptionViewModel.Code))
                && propertyNames.Contains(nameof(LocationLocalAuthorityOptionViewModel.OldCode))
            )
            {
                return JsonSerializer.Deserialize<LocationLocalAuthorityOptionViewModel>(rootElement, options)!;
            }

            if (propertyNames.Contains(nameof(LocationCodedOptionViewModel.Code)))
            {
                return JsonSerializer.Deserialize<LocationCodedOptionViewModel>(rootElement, options)!;
            }

            return JsonSerializer.Deserialize<LocationRscRegionOptionViewModel>(rootElement, options)!;
        }
        catch (JsonException exception)
        {
            throw new JsonException(message: null, exception);
        }
    }

    public override void Write(Utf8JsonWriter writer, LocationOptionViewModel? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        try
        {
            switch (value)
            {
                case LocationSchoolOptionViewModel:
                    JsonSerializer.Serialize(writer, value, typeof(LocationSchoolOptionViewModel), options);
                    break;
                case LocationRscRegionOptionViewModel:
                    JsonSerializer.Serialize(writer, value, typeof(LocationRscRegionOptionViewModel), options);
                    break;
                case LocationProviderOptionViewModel:
                    JsonSerializer.Serialize(writer, value, typeof(LocationProviderOptionViewModel), options);
                    break;
                case LocationLocalAuthorityOptionViewModel:
                    JsonSerializer.Serialize(writer, value, typeof(LocationLocalAuthorityOptionViewModel), options);
                    break;
                case LocationCodedOptionViewModel:
                    JsonSerializer.Serialize(writer, value, typeof(LocationCodedOptionViewModel), options);
                    break;
            }
        }
        catch (JsonException exception)
        {
            throw new JsonException(message: null, exception);
        }
    }
}
