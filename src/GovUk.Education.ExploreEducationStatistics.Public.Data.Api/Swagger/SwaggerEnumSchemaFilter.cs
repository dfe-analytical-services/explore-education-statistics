using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

public class SwaggerEnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var enumAttribute = context.MemberInfo?.GetCustomAttribute<SwaggerEnumAttribute>();

        if (enumAttribute is null)
        {
            return;
        }

        if (!enumAttribute.Type.IsEnum)
        {
            throw new InvalidOperationException(
                $"Must use an enum type for '{nameof(SwaggerEnumAttribute)}'"
            );
        }

        var schemaType = schema.Type == "array" ? schema.Items.Type : schema.Type;

        if (
            enumAttribute.Serializer is SwaggerEnumSerializer.Ref
            && TryGetEnumSchema(context, enumAttribute, out var enumSchema)
        )
        {
            if (enumSchema.Type != schemaType)
            {
                throw new InvalidOperationException(
                    $"Enum schema '{enumAttribute.Type.Name}' type must be {schemaType}, but was {enumSchema.Type}."
                );
            }

            var enumSchemaId = GetEnumSchemaId(context, enumAttribute);

            var enumRef = new OpenApiReference { Type = ReferenceType.Schema, Id = enumSchemaId };

            if (schema.Type == "array")
            {
                schema.Items.Reference = enumRef;
            }
            else
            {
                schema.AllOf.Add(new OpenApiSchema { Reference = enumRef });
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

        if (schema.Type == "array")
        {
            schema.Items.Enum = enums;
        }
        else
        {
            schema.Enum = enums;
        }
    }

    private static bool IsValidEnumSerializerType(
        SwaggerEnumAttribute enumAttribute,
        string schemaType
    )
    {
        return enumAttribute.Serializer switch
        {
            SwaggerEnumSerializer.Int => schemaType == "integer",
            _ => schemaType == "string",
        };
    }

    private static IList<IOpenApiAny> GetOpenApiEnums(
        SchemaFilterContext context,
        SwaggerEnumAttribute enumAttribute
    )
    {
        if (
            enumAttribute.Serializer is SwaggerEnumSerializer.Schema
            && TryGetEnumSchema(context, enumAttribute, out var enumSchema)
        )
        {
            return enumSchema.Enum;
        }

        return Enum.GetValues(enumAttribute.Type)
            .Cast<Enum>()
            .Select(e => SerializeEnum(e, enumAttribute.Serializer))
            .ToList<IOpenApiAny>();
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

        return context.SchemaRepository.Schemas.TryGetValue(refSchema.Reference.Id, out enumSchema);
    }

    private static string GetEnumSchemaId(
        SchemaFilterContext context,
        SwaggerEnumAttribute enumAttribute
    )
    {
        if (!context.SchemaRepository.TryLookupByType(enumAttribute.Type, out var refSchema))
        {
            throw new InvalidOperationException(
                $"Could not find enum schema ID for {enumAttribute.Type.Name}"
            );
        }

        return refSchema.Reference.Id;
    }

    private static IOpenApiPrimitive SerializeEnum(Enum @enum, SwaggerEnumSerializer serializer)
    {
        return serializer switch
        {
            SwaggerEnumSerializer.Int => new OpenApiInteger(Convert.ToInt32(@enum)),
            SwaggerEnumSerializer.String => new OpenApiString(@enum.ToString()),
            SwaggerEnumSerializer.Value => new OpenApiString(@enum.GetEnumValue()),
            SwaggerEnumSerializer.Label => new OpenApiString(@enum.GetEnumLabel()),
            _ => new OpenApiInteger(Convert.ToInt32(@enum)),
        };
    }
}
