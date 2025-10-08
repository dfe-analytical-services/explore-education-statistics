using System.Text.Json;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Swagger;

public class SwaggerEnumSchemaFilterTests
{
    private readonly SchemaGeneratorOptions _schemaGeneratorOptions = new()
    {
        UseAllOfToExtendReferenceSchemas = true,
        SchemaFilters = [new SwaggerEnumSchemaFilter()],
    };

    private readonly SchemaRepository _schemaRepository = new("Default");

    [Fact]
    public void SerializeEnumRef()
    {
        var schema = GenerateSchema();

        var allOf = Assert.Single(schema.Properties[nameof(TestClass.Ref)].AllOf);

        Assert.Equal(nameof(TestEnum), allOf.Reference.Id);
    }

    [Fact]
    public void SerializeEnumRef_IncompatibleTypes_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            var schemaGenerator = BuildSchemaGenerator();

            schemaGenerator.GenerateSchema(typeof(TestClassWithInvalidEnumRef), _schemaRepository);
        });
    }

    [Fact]
    public void SerializeEnumRefForList()
    {
        var schema = GenerateSchema();

        Assert.Equal(nameof(TestEnum), schema.Properties[nameof(TestClass.RefList)].Items.Reference.Id);
    }

    [Fact]
    public void SerializeEnumString()
    {
        var schema = GenerateSchema();

        var enums = schema.Properties[nameof(TestClass.String)].Enum.Cast<OpenApiString>().ToList();

        Assert.Equal(2, enums.Count);
        Assert.Equal(TestEnum.Sample1.ToString(), enums[0].Value);
        Assert.Equal(TestEnum.Sample2.ToString(), enums[1].Value);
    }

    [Fact]
    public void SerializeEnumString_IncompatibleTypes_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            var schemaGenerator = BuildSchemaGenerator();

            schemaGenerator.GenerateSchema(typeof(TestClassWithInvalidEnumString), _schemaRepository);
        });
    }

    [Fact]
    public void SerializeEnumStringsForList()
    {
        var schema = GenerateSchema();

        var enums = schema.Properties[nameof(TestClass.StringList)].Items.Enum.Cast<OpenApiString>().ToList();

        Assert.Equal(2, enums.Count);
        Assert.Equal(TestEnum.Sample1.ToString(), enums[0].Value);
        Assert.Equal(TestEnum.Sample2.ToString(), enums[1].Value);
    }

    [Fact]
    public void SerializeEnumInt()
    {
        var schema = GenerateSchema();

        var enums = schema.Properties[nameof(TestClass.Int)].Enum.Cast<OpenApiInteger>().ToList();

        Assert.Equal(2, enums.Count);
        Assert.Equal((int)TestEnum.Sample1, enums[0].Value);
        Assert.Equal((int)TestEnum.Sample2, enums[1].Value);
    }

    [Fact]
    public void SerializeEnumLabel()
    {
        var schema = GenerateSchema();

        var enums = schema.Properties[nameof(TestClass.EnumLabel)].Enum.Cast<OpenApiString>().ToList();

        Assert.Equal(2, enums.Count);
        Assert.Equal(TestEnum.Sample1.GetEnumLabel(), enums[0].Value);
        Assert.Equal(TestEnum.Sample2.GetEnumLabel(), enums[1].Value);
    }

    [Fact]
    public void SerializeEnumValue()
    {
        var schema = GenerateSchema();

        var enums = schema.Properties[nameof(TestClass.EnumValue)].Enum.Cast<OpenApiString>().ToList();

        Assert.Equal(2, enums.Count);
        Assert.Equal(TestEnum.Sample1.GetEnumValue(), enums[0].Value);
        Assert.Equal(TestEnum.Sample2.GetEnumValue(), enums[1].Value);
    }

    [Fact]
    public void SerializeEnumSchema()
    {
        var schema = GenerateSchema();

        var enums = schema.Properties[nameof(TestClass.Schema)].Enum.Cast<OpenApiString>().ToList();

        Assert.Equal(2, enums.Count);
        Assert.Equal(TestEnum.Sample1.ToString(), enums[0].Value);
        Assert.Equal(TestEnum.Sample2.ToString(), enums[1].Value);
    }

    [Fact]
    public void SerializeEnumSchema_WithGeographicLevelSchemaFilter()
    {
        _schemaGeneratorOptions.SchemaFilters.Add(new GeographicLevelSchemaFilter());

        var schemaGenerator = BuildSchemaGenerator();

        schemaGenerator.GenerateSchema(typeof(GeographicLevel), _schemaRepository);
        schemaGenerator.GenerateSchema(typeof(TestClassWithGeographicLevel), _schemaRepository);

        var schema = _schemaRepository.Schemas[nameof(TestClassWithGeographicLevel)];

        var enums = schema
            .Properties[nameof(TestClassWithGeographicLevel.GeographicLevel)]
            .Enum.Cast<OpenApiString>()
            .Select(e => e.Value)
            .ToList();

        var geographicLevels = EnumUtil.GetEnumValues<GeographicLevel>();

        Assert.Equal(enums.Order(), geographicLevels.Order());
    }

    [Fact]
    public void ThrowsOnInvalidEnumType()
    {
        Assert.Throws<InvalidOperationException>(GenerateInvalidSchema);
    }

    private OpenApiSchema GenerateSchema()
    {
        var schemaGenerator = BuildSchemaGenerator();

        schemaGenerator.GenerateSchema(typeof(TestEnum), _schemaRepository);
        schemaGenerator.GenerateSchema(typeof(TestClass), _schemaRepository);

        return _schemaRepository.Schemas[nameof(TestClass)];
    }

    private OpenApiSchema GenerateInvalidSchema()
    {
        var schemaGenerator = BuildSchemaGenerator();

        schemaGenerator.GenerateSchema(typeof(InvalidTestClass), _schemaRepository);

        return _schemaRepository.Schemas[nameof(InvalidTestClass)];
    }

    private SchemaGenerator BuildSchemaGenerator()
    {
        return new SchemaGenerator(
            _schemaGeneratorOptions,
            new JsonSerializerDataContractResolver(new JsonSerializerOptions())
        );
    }

    [JsonConverter(typeof(JsonStringEnumConverter<TestEnum>))]
    private enum TestEnum
    {
        [EnumLabelValue("Sample 1", "sample-1")]
        Sample1,

        [EnumLabelValue("Sample 2", "sample-2")]
        Sample2,
    }

    private class TestClass
    {
        [SwaggerEnum(typeof(TestEnum))]
        public required string Ref { get; set; }

        [SwaggerEnum(typeof(TestEnum))]
        public required List<string> RefList { get; set; }

        [SwaggerEnum(typeof(TestEnum), SwaggerEnumSerializer.String)]
        public required string String { get; set; }

        [SwaggerEnum(typeof(TestEnum), SwaggerEnumSerializer.String)]
        public required List<string> StringList { get; set; }

        [SwaggerEnum(typeof(TestEnum), SwaggerEnumSerializer.Int)]
        public required int Int { get; set; }

        [SwaggerEnum(typeof(TestEnum), SwaggerEnumSerializer.Label)]
        public required string EnumLabel { get; set; }

        [SwaggerEnum(typeof(TestEnum), SwaggerEnumSerializer.Value)]
        public required string EnumValue { get; set; }

        [SwaggerEnum(typeof(TestEnum), SwaggerEnumSerializer.Schema)]
        public required string Schema { get; set; }
    }

    private class TestClassWithGeographicLevel
    {
        [SwaggerEnum(typeof(GeographicLevel), SwaggerEnumSerializer.Schema)]
        public required string GeographicLevel { get; set; }
    }

    private class TestClassWithInvalidEnumRef
    {
        [SwaggerEnum(typeof(TestEnum))]
        public required int Invalid { get; set; }
    }

    private class TestClassWithInvalidEnumString
    {
        [SwaggerEnum(typeof(TestEnum), SwaggerEnumSerializer.String)]
        public required int Invalid { get; set; }
    }

    private class InvalidTestClass
    {
        [SwaggerEnum(typeof(string))]
        public required string Invalid { get; set; }
    }
}
