using Microsoft.OpenApi;
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

            if (path.Value is OpenApiPathItem pathItem)
            {
                pathItem.Parameters = pathItem.Parameters?.Where(p => p.Name != "version").ToList();
            }

            var operations = path.Value.Operations?.Values;
            if (operations != null)
            {
                foreach (var operation in operations)
                {
                    operation.Parameters = operation.Parameters?.Where(p => p.Name != "version").ToList();
                }
            }
        }

        document.Paths = newPaths;
    }
}
