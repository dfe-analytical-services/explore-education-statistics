using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Extensions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

public class ErrorViewModelSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (
            context.MemberInfo == null
            && context.Type == typeof(ErrorViewModel)
            && schema is OpenApiSchema openApiSchema
        )
        {
            if (
                openApiSchema.Properties?.TryGetValue(
                    nameof(ErrorViewModel.Code).ToLowerFirst(),
                    out var codePropertySchema
                ) == true
                && codePropertySchema is OpenApiSchema openApiCodePropertySchema
            )
            {
                openApiCodePropertySchema.Type = openApiCodePropertySchema.Type.WithoutFlags(JsonSchemaType.Null);
            }

            openApiSchema.Required = new HashSet<string>
            {
                nameof(ErrorViewModel.Code).ToLowerFirst(),
                nameof(ErrorViewModel.Message).ToLowerFirst(),
            };
        }
    }
}
