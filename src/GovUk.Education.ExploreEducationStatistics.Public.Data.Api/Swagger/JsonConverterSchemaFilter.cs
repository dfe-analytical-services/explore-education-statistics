using System.Reflection;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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

            if (property.PropertyType.IsEnum)
            {
                ApplyEnumConverter(property, propertySchema, converterType, context.SchemaRepository);
            }
        }
    }

    private static void ApplyEnumConverter(
        PropertyInfo property,
        OpenApiSchema propertySchema,
        Type converterType,
        SchemaRepository schemaRepository)
    {
        Type? enumType = null;

        var converterBaseType = converterType.IsGenericType
            ? converterType.GetGenericTypeDefinition()
            : converterType;

        if (converterBaseType == typeof(JsonStringEnumConverter) ||
            converterBaseType == typeof(JsonStringEnumConverter<>))
        {
            enumType = property.PropertyType;

            propertySchema.Type = "string";
            propertySchema.Enum = Enum.GetNames(enumType)
                .Select(name => new OpenApiString(name))
                .ToList<IOpenApiAny>();
        }
        else if (converterBaseType == typeof(EnumToEnumLabelJsonConverter<>))
        {
            enumType = property.PropertyType;

            propertySchema.Type = "string";
            propertySchema.Enum = Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(name => new OpenApiString(name.GetEnumLabel()))
                .ToList<IOpenApiAny>();
        }

        if (enumType is not null && schemaRepository.TryLookupByType(enumType, out var enumSchema))
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
}
