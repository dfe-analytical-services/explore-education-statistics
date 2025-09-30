using System.Reflection;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

/// <summary>
/// <para>This filter is necessary to make Swashbuckle respect <see cref="JsonConverter" />
/// attributes that are commonly used for properties on our view models and requests.</para>
///
/// <para>Without this, Swashbuckle doesn't really know what the actual type should be
/// after the converter has ran and may incorrectly default to the property's type.</para>
/// </summary>
internal class JsonConverterSchemaFilter : ISchemaFilter
{
    private readonly HashSet<Type> _typesToIgnore =
    [
         typeof(GeographicLevel),
        typeof(IndicatorUnit),
        typeof(TimeIdentifier),
    ];

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type is { IsClass: false })
        {
            return;
        }

        var properties = context.Type
            .GetProperties()
            .Where(
                property => property.CustomAttributes
                    .Any(attr => attr.AttributeType == typeof(JsonConverterAttribute))
            );

        foreach (var property in properties)
        {
            if (property.GetCustomAttribute(typeof(JsonConverterAttribute))
                is not JsonConverterAttribute attribute)
            {
                continue;
            }

            var converterType = attribute.ConverterType;

            if (converterType is null || !converterType.IsAssignableTo(typeof(JsonConverter)))
            {
                continue;
            }

            if (!schema.Properties.TryGetValue(property.Name.CamelCase(), out var propertySchema))
            {
                continue;
            }

            var propertyType = property.PropertyType.GetUnderlyingType();

            if (propertyType.IsEnum && !_typesToIgnore.Contains(propertyType))
            {
                ApplyEnumConverter(propertyType, propertySchema, converterType, context.SchemaRepository);

                continue;
            }

            if (!propertyType.IsGenericType)
            {
                continue;
            }

            if (propertyType.GetGenericTypeDefinition().IsAssignableTo(typeof(IReadOnlyList<>))
                && propertyType.GenericTypeArguments[0].IsEnum)
            {
                ApplyReadOnlyListEnumConverter(propertySchema, converterType, context.SchemaRepository);
            }
        }
    }

    private void ApplyEnumConverter(
        Type enumType,
        OpenApiSchema propertySchema,
        Type converterType,
        SchemaRepository schemaRepository)
    {
        var hasEnumConverter = false;

        var converterBaseType = converterType.IsGenericType
            ? converterType.GetGenericTypeDefinition()
            : converterType;

        if (converterBaseType == typeof(JsonStringEnumConverter) ||
            converterBaseType == typeof(JsonStringEnumConverter<>))
        {
            hasEnumConverter = true;

            propertySchema.Type = "string";
            propertySchema.Enum = Enum.GetNames(enumType)
                .Select(name => new OpenApiString(name))
                .ToList<IOpenApiAny>();
        }
        else if (converterBaseType == typeof(EnumToEnumLabelJsonConverter<>))
        {
            hasEnumConverter = true;

            propertySchema.Type = "string";
            propertySchema.Enum = Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(name => new OpenApiString(name.GetEnumLabel()))
                .ToList<IOpenApiAny>();
        }
        else if (converterBaseType == typeof(EnumToEnumValueJsonConverter<>))
        {
            hasEnumConverter = true;

            propertySchema.Type = "string";
            propertySchema.Enum = Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(name => new OpenApiString(name.GetEnumValue()))
                .ToList<IOpenApiAny>();
        }

        if (hasEnumConverter && schemaRepository.TryLookupByType(enumType, out var enumSchema))
        {
            // Clear any references to the enum schema as this would get merged together
            // with the property in the final document and produce incorrect enum values.

            if (propertySchema.Reference is not null &&
                propertySchema.Reference.Id == enumSchema.Reference.Id)
            {
                propertySchema.Reference = null;
            }

            propertySchema.AllOf = propertySchema.AllOf
                .Where(s => s.Reference is null || s.Reference.Id != enumSchema.Reference.Id)
                .ToList();
        }
    }

    private void ApplyReadOnlyListEnumConverter(
        OpenApiSchema propertySchema,
        Type converterType,
        SchemaRepository schemaRepository)
    {
        if (converterType.GetGenericTypeDefinition() != typeof(ReadOnlyListJsonConverter<,>))
        {
            return;
        }

        var enumType = converterType.GenericTypeArguments[0].GetUnderlyingType();

        if (!enumType.IsEnum)
        {
            return;
        }

        ApplyEnumConverter(enumType, propertySchema.Items, converterType.GenericTypeArguments[1], schemaRepository);
    }
}
