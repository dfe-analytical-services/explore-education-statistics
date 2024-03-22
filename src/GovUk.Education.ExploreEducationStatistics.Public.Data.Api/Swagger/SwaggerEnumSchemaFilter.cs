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
                $"Must use an enum type for '{nameof(SwaggerEnumAttribute)}'");
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

    private IList<IOpenApiAny> GetOpenApiEnums(SchemaFilterContext context, SwaggerEnumAttribute enumAttribute)
    {
        if (enumAttribute.Serializer is SwaggerEnumSerializer.Default
            && context.SchemaRepository.Schemas.TryGetValue(enumAttribute.Type.Name, out var enumSchema))
        {
            return enumSchema.Enum;
        }

        return Enum.GetValues(enumAttribute.Type)
            .Cast<Enum>()
            .Select(e => SerializeEnum(e, enumAttribute.Serializer))
            .ToList<IOpenApiAny>();
    }

    private IOpenApiPrimitive SerializeEnum(Enum @enum, SwaggerEnumSerializer serializer)
    {
        return serializer switch
        {
            SwaggerEnumSerializer.Int => new OpenApiInteger(Convert.ToInt32(@enum)),
            SwaggerEnumSerializer.String => new OpenApiString(@enum.ToString()),
            SwaggerEnumSerializer.Value => new OpenApiString(@enum.GetEnumValue()),
            SwaggerEnumSerializer.Label => new OpenApiString(@enum.GetEnumLabel()),
            _ => new OpenApiInteger(Convert.ToInt32(@enum))
        };
    }
}
