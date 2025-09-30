using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

public class RequiredPropertySchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var nullabilityContext = new NullabilityInfoContext();
        var properties = context.Type.GetProperties();

        foreach (var property in properties)
        {
            if (property.HasAttribute<JsonIgnoreAttribute>()
                || !property.HasAttribute<RequiredMemberAttribute>())
            {
                continue;
            }

            var jsonName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;

            var jsonKey = schema.Properties.Keys.SingleOrDefault(key =>
                string.Equals(key, jsonName, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(jsonKey))
            {
                continue;
            }

            var isReferenceType = schema.Properties[jsonKey].Type == null;

            if (isReferenceType)
            {
                var nullabilityInfo = nullabilityContext.Create(property);

                // The required property is using a nullable reference type, however, this is not
                // permitted in an OpenAPI schema. Reference types can't be marked as both nullable
                // as they are simply represented as `$ref: '#/blah'`.
                // If nullable reference types cannot be marked as nullable in OpenAPI, they also shouldn't
                // be marked as required as this would imply they are non-nullable (which is incorrect).
                if (nullabilityInfo.ReadState == NullabilityState.Nullable)
                {
                    continue;
                }
            }

            schema.Required.Add(jsonKey);
        }
    }
}
