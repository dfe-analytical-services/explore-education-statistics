using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Swagger;

public class DataSetStatusSchemaFilterTests
{
    private readonly SchemaGenerator _schemaGenerator = new(
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

        Assert.Equal(JsonSchemaType.String, schema.Type);
        Assert.NotNull(schema.Enum);
        Assert.Equal(3, schema.Enum.Count);

        Assert.Equal(nameof(DataSetStatus.Published), schema.Enum[0].GetValue<string>());
        Assert.Equal(nameof(DataSetStatus.Deprecated), schema.Enum[1].GetValue<string>());
        Assert.Equal(nameof(DataSetStatus.Withdrawn), schema.Enum[2].GetValue<string>());
    }

    private IOpenApiSchema GenerateSchema()
    {
        _schemaGenerator.GenerateSchema(typeof(DataSetStatus), _schemaRepository);

        return _schemaRepository.Schemas[nameof(DataSetStatus)];
    }
}
