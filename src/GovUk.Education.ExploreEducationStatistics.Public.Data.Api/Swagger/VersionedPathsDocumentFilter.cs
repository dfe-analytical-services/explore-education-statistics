using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

public class VersionedPathsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        var newPaths = new OpenApiPaths();

        foreach (var path in document.Paths)
        {
            var versionedPath = path.Key.Replace("{version}", document.Info.Version);

            newPaths[versionedPath] = path.Value;

            path.Value.Parameters = path.Value.Parameters.Where(p => p.Name != "version").ToList();

            foreach (var operation in path.Value.Operations.Values)
            {
                operation.Parameters = operation
                    .Parameters.Where(p => p.Name != "version")
                    .ToList();
            }
        }

        document.Paths = newPaths;
    }
}
