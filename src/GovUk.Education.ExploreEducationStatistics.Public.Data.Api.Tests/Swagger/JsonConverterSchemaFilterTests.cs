using System.Text.Json;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Swagger;

public class JsonConverterSchemaFilterTests
{
    private readonly ISchemaGenerator _schemaGenerator = new SchemaGenerator(
        new SchemaGeneratorOptions
        {
            UseAllOfToExtendReferenceSchemas = true,
            SchemaFilters = [new JsonConverterSchemaFilter()],
        },
        new JsonSerializerDataContractResolver(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        })
    );

    private readonly SchemaRepository _schemaRepository = new("Default");

    [Theory]
    [InlineData(nameof(TestEnumConverters.StringEnum))]
    [InlineData(nameof(TestEnumConverters.StringEnumTyped))]
    public void JsonStringEnumConverters(string propertyName)
    {
        var schema = GenerateTestEnumConvertersSchema();

        var schemaPropertyName = propertyName.CamelCase();
        var propertySchema = schema.Properties[schemaPropertyName];

        Assert.Null(propertySchema.Reference);
        Assert.Empty(propertySchema.AllOf);

        Assert.Equal("string", propertySchema.Type);
        Assert.Equal(2, propertySchema.Enum.Count);

        var enumString1 = Assert.IsType<OpenApiString>(propertySchema.Enum[0]);
        Assert.Equal(nameof(TestEnum.Sample1), enumString1.Value);

        var enumString2 = Assert.IsType<OpenApiString>(propertySchema.Enum[1]);
        Assert.Equal(nameof(TestEnum.Sample2), enumString2.Value);
    }

    [Fact]
    public void EnumToEnumLabelConverter()
    {
        var schema = GenerateTestEnumConvertersSchema();

        var schemaPropertyName = nameof(TestEnumConverters.EnumToEnumLabel).CamelCase();
        var propertySchema = schema.Properties[schemaPropertyName];

        Assert.Null(propertySchema.Reference);
        Assert.Empty(propertySchema.AllOf);

        Assert.Equal("string", propertySchema.Type);
        Assert.Equal(2, propertySchema.Enum.Count);

        var enumString1 = Assert.IsType<OpenApiString>(propertySchema.Enum[0]);
        Assert.Equal(TestEnum.Sample1.GetEnumLabel(), enumString1.Value);

        var enumString2 = Assert.IsType<OpenApiString>(propertySchema.Enum[1]);
        Assert.Equal(TestEnum.Sample2.GetEnumLabel(), enumString2.Value);
    }

    private OpenApiSchema GenerateTestEnumConvertersSchema()
    {
        _schemaRepository.RegisterType(typeof(TestEnum), nameof(TestEnum));
        _schemaRepository.AddDefinition(nameof(TestEnum), TestEnumSchema);

        _schemaGenerator.GenerateSchema(typeof(TestEnumConverters), _schemaRepository);

        return _schemaRepository.Schemas[nameof(TestEnumConverters)];
    }

    private static OpenApiSchema TestEnumSchema => new()
    {
        Type = "number",
        Enum = Enum.GetValues<TestEnum>()
            .Select(e => new OpenApiInteger((int)e))
            .ToList<IOpenApiAny>()
    };

    private class TestEnumConverters
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TestEnum StringEnum { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter<TestEnum>))]
        public TestEnum StringEnumTyped { get; init; }

        [JsonConverter(typeof(EnumToEnumLabelJsonConverter<TestEnum>))]
        public TestEnum EnumToEnumLabel { get; init; }
    }

    private enum TestEnum
    {
        [EnumLabelValue("Sample 1", "sample-1")]
        Sample1,

        [EnumLabelValue("Sample 2", "sample-2")]
        Sample2,
    }
}
