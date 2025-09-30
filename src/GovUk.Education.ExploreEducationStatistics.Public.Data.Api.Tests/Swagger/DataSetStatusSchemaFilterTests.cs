using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Swagger;

public class DataSetStatusSchemaFilterTests
{
    private readonly ISchemaGenerator _schemaGenerator = new SchemaGenerator(
        new SchemaGeneratorOptions
        {
            UseAllOfToExtendReferenceSchemas = true,
            SchemaFilters = [new DataSetStatusSchemaFilter()],
        },
        new JsonSerializerDataContractResolver(
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        )
    );

    private readonly SchemaRepository _schemaRepository = new("Default");

    [Fact]
    public void ConvertsEnumsToStringAndRemovesStaged()
    {
        var schema = GenerateSchema();

        Assert.Equal("string", schema.Type);
        Assert.Equal(3, schema.Enum.Count);

        var enumString1 = Assert.IsType<OpenApiString>(schema.Enum[0]);
        Assert.Equal(DataSetStatus.Published.ToString(), enumString1.Value);

        var enumString2 = Assert.IsType<OpenApiString>(schema.Enum[1]);
        Assert.Equal(DataSetStatus.Deprecated.ToString(), enumString2.Value);

        var enumString3 = Assert.IsType<OpenApiString>(schema.Enum[2]);
        Assert.Equal(DataSetStatus.Withdrawn.ToString(), enumString3.Value);
    }

    private OpenApiSchema GenerateSchema()
    {
        _schemaGenerator.GenerateSchema(typeof(DataSetStatus), _schemaRepository);

        return _schemaRepository.Schemas[nameof(DataSetStatus)];
    }
}
