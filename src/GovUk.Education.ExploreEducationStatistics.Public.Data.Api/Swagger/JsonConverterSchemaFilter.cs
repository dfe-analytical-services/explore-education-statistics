using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.OpenApi;
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

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type is { IsClass: false })
        {
            return;
        }

        var properties = context
            .Type.GetProperties()
            .Where(property =>
                property.CustomAttributes.Any(attr => attr.AttributeType == typeof(JsonConverterAttribute))
            );

        foreach (var property in properties)
        {
            if (property.GetCustomAttribute(typeof(JsonConverterAttribute)) is not JsonConverterAttribute attribute)
            {
                continue;
            }

            var converterType = attribute.ConverterType;

            if (converterType is null || !converterType.IsAssignableTo(typeof(JsonConverter)))
            {
                continue;
            }

            if (
                schema.Properties is null
                || !schema.Properties.TryGetValue(property.Name.CamelCase(), out var propertySchema)
            )
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

            if (
                propertyType.GetGenericTypeDefinition().IsAssignableTo(typeof(IReadOnlyList<>))
                && propertyType.GenericTypeArguments[0].IsEnum
            )
            {
                ApplyReadOnlyListEnumConverter(propertySchema, converterType, context.SchemaRepository);
            }
        }
    }

    private void ApplyEnumConverter(
        Type enumType,
        IOpenApiSchema propertySchema,
        Type converterType,
        SchemaRepository schemaRepository
    )
    {
        if (propertySchema is not OpenApiSchema openApiSchema)
        {
            return;
        }

        var hasEnumConverter = false;

        var converterBaseType = converterType.IsGenericType ? converterType.GetGenericTypeDefinition() : converterType;

        if (
            converterBaseType == typeof(JsonStringEnumConverter)
            || converterBaseType == typeof(JsonStringEnumConverter<>)
        )
        {
            hasEnumConverter = true;

            openApiSchema.Type = JsonSchemaType.String;
            openApiSchema.Enum = Enum.GetNames(enumType).Select(JsonNode (name) => JsonValue.Create(name)).ToList();
        }
        else if (converterBaseType == typeof(EnumToEnumLabelJsonConverter<>))
        {
            hasEnumConverter = true;

            openApiSchema.Type = JsonSchemaType.String;
            openApiSchema.Enum = Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(JsonNode (name) => JsonValue.Create(name.GetEnumLabel()))
                .ToList();
        }
        else if (converterBaseType == typeof(EnumToEnumValueJsonConverter<>))
        {
            hasEnumConverter = true;

            openApiSchema.Type = JsonSchemaType.String;
            openApiSchema.Enum = Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(JsonNode (name) => JsonValue.Create(name.GetEnumValue()))
                .ToList();
        }

        if (hasEnumConverter && schemaRepository.TryLookupByType(enumType, out var enumSchemaRef))
        {
            // Clear any references to the enum schema as this would get merged together
            // with the property in the final document and produce incorrect enum values.
            if (openApiSchema.AllOf is not null)
            {
                openApiSchema.AllOf = openApiSchema
                    .AllOf.Where(s =>
                        s is not OpenApiSchemaReference openApiSchemaReference
                        || openApiSchemaReference.Reference.Id != enumSchemaRef.Reference.Id
                    )
                    .ToList();
            }
        }
    }

    private void ApplyReadOnlyListEnumConverter(
        IOpenApiSchema propertySchema,
        Type converterType,
        SchemaRepository schemaRepository
    )
    {
        if (propertySchema is not OpenApiSchema openApiSchema)
        {
            return;
        }

        if (converterType.GetGenericTypeDefinition() != typeof(ReadOnlyListJsonConverter<,>))
        {
            return;
        }

        var enumType = converterType.GenericTypeArguments[0].GetUnderlyingType();

        if (!enumType.IsEnum)
        {
            return;
        }

        if (openApiSchema.Items is not null)
        {
            // Items may be a $ref to the enum's shared component schema. Replace it
            // with a new inline schema so ApplyEnumConverter can set Type and Enum on it
            // without mutating the shared component schema used elsewhere in the document.
            var itemsSchema = new OpenApiSchema();
            openApiSchema.Items = itemsSchema;
            ApplyEnumConverter(enumType, itemsSchema, converterType.GenericTypeArguments[1], schemaRepository);
        }
    }
}
