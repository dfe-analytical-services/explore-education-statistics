using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Swagger;

public class ErrorViewModelSchemaFilterTests
{
    private readonly SchemaGenerator _schemaGenerator = new(
        new SchemaGeneratorOptions
        {
            UseAllOfToExtendReferenceSchemas = true,
            SchemaFilters = [new ErrorViewModelSchemaFilter()],
        },
        new JsonSerializerDataContractResolver(
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        )
    );

    private readonly SchemaRepository _schemaRepository = new("Default");

    [Fact]
    public void CorrectSchema()
    {
        var schema = GenerateSchema();

        Assert.False(schema.Properties["code"].Nullable);
        Assert.Contains("message", schema.Required);
        Assert.Contains("code", schema.Required);
    }

    private OpenApiSchema GenerateSchema()
    {
        _schemaGenerator.GenerateSchema(typeof(ErrorViewModel), _schemaRepository);

        return _schemaRepository.Schemas[nameof(ErrorViewModel)];
    }
}
