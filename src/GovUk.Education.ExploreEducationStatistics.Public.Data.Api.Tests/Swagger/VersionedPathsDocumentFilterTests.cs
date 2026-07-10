using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Swagger;

public class VersionedPathsDocumentFilterTests
{
    [Fact]
    public void VersionsInlinedIntoPaths()
    {
        var document = new OpenApiDocument
        {
            Info = new OpenApiInfo { Version = "1" },
            Paths = new OpenApiPaths
            {
                { "/v{version}/endpoint-1", new OpenApiPathItem() },
                { "/v{version}/endpoint-2/{id}", new OpenApiPathItem() },
            },
        };

        var filter = new VersionedPathsDocumentFilter();
        var context = CreateDocumentFilterContext();

        filter.Apply(document, context);

        Assert.Equal(2, document.Paths.Count);
        Assert.Contains("/v1/endpoint-1", document.Paths.Keys);
        Assert.Contains("/v1/endpoint-2/{id}", document.Paths.Keys);
    }

    [Fact]
    public void VersionParametersRemoved()
    {
        var document = new OpenApiDocument
        {
            Info = new OpenApiInfo { Version = "1" },
            Paths = new OpenApiPaths
            {
                {
                    "/v{version}/endpoint-1",
                    new OpenApiPathItem
                    {
                        Parameters = [new OpenApiParameter { Name = "version" }],
                        Operations = new Dictionary<HttpMethod, OpenApiOperation>
                        {
                            [HttpMethod.Get] = new() { Parameters = [new OpenApiParameter { Name = "version" }] },
                        },
                    }
                },
                {
                    "/v{version}/endpoint-2/{id}",
                    new OpenApiPathItem
                    {
                        Parameters = [new OpenApiParameter { Name = "version" }, new OpenApiParameter { Name = "id" }],
                        Operations = new Dictionary<HttpMethod, OpenApiOperation>
                        {
                            [HttpMethod.Get] = new()
                            {
                                Parameters =
                                [
                                    new OpenApiParameter { Name = "version" },
                                    new OpenApiParameter { Name = "id" },
                                ],
                            },
                        },
                    }
                },
            },
        };

        var filter = new VersionedPathsDocumentFilter();
        var context = CreateDocumentFilterContext();

        filter.Apply(document, context);

        Assert.Equal(2, document.Paths.Count);

        var endpoint1Paths = document.Paths["/v1/endpoint-1"];

        Assert.NotNull(endpoint1Paths.Parameters);
        Assert.Empty(endpoint1Paths.Parameters);

        Assert.NotNull(endpoint1Paths.Operations);
        var endpoint1GetOperationParameters = endpoint1Paths.Operations[HttpMethod.Get].Parameters;
        Assert.NotNull(endpoint1GetOperationParameters);
        Assert.Empty(endpoint1GetOperationParameters);

        var endpoint2Paths = document.Paths["/v1/endpoint-2/{id}"];

        Assert.NotNull(endpoint2Paths.Parameters);
        Assert.Single(endpoint2Paths.Parameters);
        Assert.Equal("id", endpoint2Paths.Parameters[0].Name);

        Assert.NotNull(endpoint2Paths.Operations);
        var endpoint2GetOperationParameters = endpoint2Paths.Operations[HttpMethod.Get].Parameters;
        Assert.NotNull(endpoint2GetOperationParameters);
        Assert.Single(endpoint2GetOperationParameters);
        Assert.Equal("id", endpoint2GetOperationParameters[0].Name);
    }

    private static DocumentFilterContext CreateDocumentFilterContext()
    {
        var schemaGenerator = new SchemaGenerator(
            new SchemaGeneratorOptions(),
            new JsonSerializerDataContractResolver(
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            )
        );
        var schemaRepository = new SchemaRepository();

        return new DocumentFilterContext([], schemaGenerator, schemaRepository);
    }
}
