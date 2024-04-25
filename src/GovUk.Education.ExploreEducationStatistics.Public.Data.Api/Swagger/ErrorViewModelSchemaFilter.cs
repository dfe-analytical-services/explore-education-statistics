using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

public class ErrorViewModelSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo is not null || context.Type != typeof(ErrorViewModel))
        {
            return;
        }

        schema.Properties[nameof(ErrorViewModel.Code).ToLowerFirst()].Nullable = false;

        schema.Required = new HashSet<string>
        {
            nameof(ErrorViewModel.Code).ToLowerFirst(),
            nameof(ErrorViewModel.Message).ToLowerFirst(),
        };
    }
}
