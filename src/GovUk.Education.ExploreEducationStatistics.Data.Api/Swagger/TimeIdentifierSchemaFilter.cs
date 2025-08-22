#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Swagger;

/// <summary>
/// TimeIdentifier is special. Each enum value has an attached attribute that contains a different API value. 
/// </summary>
public class TimeIdentifierSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(TimeIdentifier))
        {
            schema.Enum.Clear();
            var timeIdentifiers = Enum.GetValues<TimeIdentifier>();
            var values = timeIdentifiers.Select(name => name.GetEnumValue());
            schema.Enum.AddRange(values.Select(v => new OpenApiString($"{v}")));;
        }
    }
}
