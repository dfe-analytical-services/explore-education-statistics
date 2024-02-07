using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
        new JsonSerializerDataContractResolver(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        })
    );

    private readonly SchemaRepository _schemaRepository = new("Default");

    [Fact]
    public void ConvertsEnumsToStringAndRemovesStaged()
    {
        var schema = GenerateSchema();

        var dataSetStatusSchema = schema[nameof(DataSetStatus)];
        var testClassSchema = schema[nameof(TestClass)];

        var dataSetStatusSchemaPropertyName = nameof(TestClass.DataSetStatus).CamelCase();

        var dataSetStatusPropertySchema = testClassSchema.Properties[dataSetStatusSchemaPropertyName];

        Assert.Equal("string", dataSetStatusSchema.Type);
        Assert.Equal(3, dataSetStatusSchema.Enum.Count);

        var enumString1 = Assert.IsType<OpenApiString>(dataSetStatusSchema.Enum[0]);
        Assert.Equal(DataSetStatus.Published.ToString(), enumString1.Value);

        var enumString2 = Assert.IsType<OpenApiString>(dataSetStatusSchema.Enum[1]);
        Assert.Equal(DataSetStatus.Deprecated.ToString(), enumString2.Value);

        var enumString3 = Assert.IsType<OpenApiString>(dataSetStatusSchema.Enum[2]);
        Assert.Equal(DataSetStatus.Unpublished.ToString(), enumString3.Value);

        Assert.Equal("object", testClassSchema.Type);

        var allOf = Assert.Single(dataSetStatusPropertySchema.AllOf);
        Assert.Equal(nameof(DataSetStatus), allOf.Reference.Id);
        Assert.Equal("#/definitions/DataSetStatus", allOf.Reference.ReferenceV2);
        Assert.Equal("#/components/schemas/DataSetStatus", allOf.Reference.ReferenceV3);
    }

    private IReadOnlyDictionary<string, OpenApiSchema> GenerateSchema()
    {
        _schemaGenerator.GenerateSchema(typeof(DataSetStatus), _schemaRepository);
        _schemaGenerator.GenerateSchema(typeof(TestClass), _schemaRepository);

        return _schemaRepository.Schemas;
    }

    private class TestClass
    {
        public DataSetStatus DataSetStatus { get; init; }
    }
}
