using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Nodes;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

public class SwaggerEnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema openApiSchema)
        {
            return;
        }

        var enumAttribute = context.MemberInfo?.GetCustomAttribute<SwaggerEnumAttribute>();

        if (enumAttribute is null)
        {
            return;
        }

        if (!enumAttribute.Type.IsEnum)
        {
            throw new InvalidOperationException($"Must use an enum type for '{nameof(SwaggerEnumAttribute)}'");
        }

        var isArray = openApiSchema.Type.HasValue && openApiSchema.Type.Value.HasFlag(JsonSchemaType.Array);
        var schemaType = isArray ? openApiSchema.Items?.Type : openApiSchema.Type;

        if (
            enumAttribute.Serializer is SwaggerEnumSerializer.Ref
            && TryGetEnumSchema(context, enumAttribute, out var enumSchema)
        )
        {
            if (enumSchema.Type.HasValue && schemaType.HasValue && !schemaType.Value.HasFlag(enumSchema.Type.Value))
            {
                throw new InvalidOperationException(
                    $"Enum schema '{enumAttribute.Type.Name}' type must be {schemaType}, but was {enumSchema.Type}."
                );
            }

            var enumSchemaId = GetEnumSchemaId(context, enumAttribute);

            if (isArray)
            {
                openApiSchema.Items = new OpenApiSchemaReference(enumSchemaId);
            }
            else
            {
                openApiSchema.AllOf ??= new List<IOpenApiSchema>();
                openApiSchema.AllOf.Add(new OpenApiSchemaReference(enumSchemaId));
            }

            return;
        }

        if (!IsValidEnumSerializerType(enumAttribute, schemaType))
        {
            throw new InvalidOperationException(
                $"Schema type {schemaType} is not compatible with {enumAttribute.Serializer} serialized enum"
            );
        }

        var enums = GetOpenApiEnums(context, enumAttribute);

        if (isArray)
        {
            if (openApiSchema.Items is OpenApiSchema itemsSchema)
            {
                itemsSchema.Enum = enums;
            }
        }
        else
        {
            openApiSchema.Enum = enums;
        }
    }

    private static bool IsValidEnumSerializerType(SwaggerEnumAttribute enumAttribute, JsonSchemaType? schemaType)
    {
        return enumAttribute.Serializer switch
        {
            SwaggerEnumSerializer.Int => schemaType.HasValue && schemaType.Value.HasFlag(JsonSchemaType.Integer),
            _ => schemaType.HasValue && schemaType.Value.HasFlag(JsonSchemaType.String),
        };
    }

    private static IList<JsonNode> GetOpenApiEnums(SchemaFilterContext context, SwaggerEnumAttribute enumAttribute)
    {
        if (
            enumAttribute.Serializer is SwaggerEnumSerializer.Schema
            && TryGetEnumSchema(context, enumAttribute, out var enumSchema)
        )
        {
            return enumSchema.Enum ?? [];
        }

        return Enum.GetValues(enumAttribute.Type)
            .Cast<Enum>()
            .Select(e => SerializeEnum(e, enumAttribute.Serializer))
            .ToList();
    }

    private static bool TryGetEnumSchema(
        SchemaFilterContext context,
        SwaggerEnumAttribute enumAttribute,
        [NotNullWhen(true)] out OpenApiSchema? enumSchema
    )
    {
        if (!context.SchemaRepository.TryLookupByType(enumAttribute.Type, out var refSchema))
        {
            enumSchema = null;
            return false;
        }

        if (
            context.SchemaRepository.Schemas.TryGetValue(refSchema.Reference.Id ?? string.Empty, out var schema)
            && schema is OpenApiSchema concreteSchema
        )
        {
            enumSchema = concreteSchema;
            return true;
        }

        enumSchema = null;
        return false;
    }

    private static string GetEnumSchemaId(SchemaFilterContext context, SwaggerEnumAttribute enumAttribute)
    {
        if (!context.SchemaRepository.TryLookupByType(enumAttribute.Type, out var refSchema))
        {
            throw new InvalidOperationException($"Could not find enum schema ID for {enumAttribute.Type.Name}");
        }

        return refSchema.Reference.Id
            ?? throw new InvalidOperationException($"Could not find enum schema ID for {enumAttribute.Type.Name}");
    }

    private static JsonNode SerializeEnum(Enum @enum, SwaggerEnumSerializer serializer)
    {
        return serializer switch
        {
            SwaggerEnumSerializer.Int => JsonValue.Create(Convert.ToInt32(@enum))!,
            SwaggerEnumSerializer.String => JsonValue.Create(@enum.ToString())!,
            SwaggerEnumSerializer.Value => JsonValue.Create(@enum.GetEnumValue())!,
            SwaggerEnumSerializer.Label => JsonValue.Create(@enum.GetEnumLabel())!,
            _ => JsonValue.Create(Convert.ToInt32(@enum))!,
        };
    }
}
