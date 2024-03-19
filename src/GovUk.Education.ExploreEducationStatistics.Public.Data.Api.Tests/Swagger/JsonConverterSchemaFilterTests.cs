using System.Text.Json;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
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

    [Fact]
    public void EnumToEnumValueConverter()
    {
        var schema = GenerateTestEnumConvertersSchema();

        var schemaPropertyName = nameof(TestEnumConverters.EnumToEnumValue).CamelCase();
        var propertySchema = schema.Properties[schemaPropertyName];

        Assert.Null(propertySchema.Reference);
        Assert.Empty(propertySchema.AllOf);

        Assert.Equal("string", propertySchema.Type);
        Assert.Equal(2, propertySchema.Enum.Count);

        var enumString1 = Assert.IsType<OpenApiString>(propertySchema.Enum[0]);
        Assert.Equal(TestEnum.Sample1.GetEnumValue(), enumString1.Value);

        var enumString2 = Assert.IsType<OpenApiString>(propertySchema.Enum[1]);
        Assert.Equal(TestEnum.Sample2.GetEnumValue(), enumString2.Value);
    }

    [Theory]
    [InlineData(nameof(TestEnumConverters.GeographicLevelStringEnum))]
    [InlineData(nameof(TestEnumConverters.GeographicLevelStringEnumTyped))]
    [InlineData(nameof(TestEnumConverters.GeographicLevelEnumToEnumLabel))]
    [InlineData(nameof(TestEnumConverters.GeographicLevelEnumToEnumValue))]
    public void GeographicLevel_Ignored(string propertyName)
    {
        var schema = GenerateTestEnumConvertersSchema();

        var schemaPropertyName = propertyName.CamelCase();
        var propertySchema = schema.Properties[schemaPropertyName];

        Assert.NotEmpty(propertySchema.AllOf);

        Assert.Null(propertySchema.Type);
        Assert.Empty(propertySchema.Enum);
    }

    [Theory]
    [InlineData(nameof(TestEnumConverters.TimeIdentifierStringEnum))]
    [InlineData(nameof(TestEnumConverters.TimeIdentifierStringEnumTyped))]
    [InlineData(nameof(TestEnumConverters.TimeIdentifierEnumToEnumLabel))]
    [InlineData(nameof(TestEnumConverters.TimeIdentifierEnumToEnumValue))]
    public void TimeIdentifier_Ignored(string propertyName)
    {
        var schema = GenerateTestEnumConvertersSchema();

        var schemaPropertyName = propertyName.CamelCase();
        var propertySchema = schema.Properties[schemaPropertyName];

        Assert.NotEmpty(propertySchema.AllOf);

        Assert.Null(propertySchema.Type);
        Assert.Empty(propertySchema.Enum);
    }

    private OpenApiSchema GenerateTestEnumConvertersSchema()
    {
        _schemaGenerator.GenerateSchema(typeof(TestEnum), _schemaRepository);
        _schemaGenerator.GenerateSchema(typeof(TestEnumConverters), _schemaRepository);

        return _schemaRepository.Schemas[nameof(TestEnumConverters)];
    }

    private class TestEnumConverters
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TestEnum StringEnum { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter<TestEnum>))]
        public TestEnum StringEnumTyped { get; init; }

        [JsonConverter(typeof(EnumToEnumLabelJsonConverter<TestEnum>))]
        public TestEnum EnumToEnumLabel { get; init; }

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<TestEnum>))]
        public TestEnum EnumToEnumValue { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeographicLevel GeographicLevelStringEnum { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter<GeographicLevel>))]
        public GeographicLevel GeographicLevelStringEnumTyped { get; init; }

        [JsonConverter(typeof(EnumToEnumLabelJsonConverter<GeographicLevel>))]
        public GeographicLevel GeographicLevelEnumToEnumLabel { get; init; }

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<GeographicLevel>))]
        public GeographicLevel GeographicLevelEnumToEnumValue { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TimeIdentifier TimeIdentifierStringEnum { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter<TimeIdentifier>))]
        public TimeIdentifier TimeIdentifierStringEnumTyped { get; init; }

        [JsonConverter(typeof(EnumToEnumLabelJsonConverter<TimeIdentifier>))]
        public TimeIdentifier TimeIdentifierEnumToEnumLabel { get; init; }

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
        public TimeIdentifier TimeIdentifierEnumToEnumValue { get; init; }
    }

    private enum TestEnum
    {
        [EnumLabelValue("Sample 1", "sample-1")]
        Sample1,

        [EnumLabelValue("Sample 2", "sample-2")]
        Sample2,
    }
}
