using System.Text.Json;
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
    public void SerializeEnumString()
    {
        var schema = GenerateSchema();

        var enums = schema.Properties["string"]
            .Enum
            .Cast<OpenApiString>()
            .ToList();

        Assert.Equal(2, enums.Count);
        Assert.Equal(TestEnum.Sample1.ToString(), enums[0].Value);
        Assert.Equal(TestEnum.Sample2.ToString(), enums[1].Value);
    }

    [Fact]
    public void SerializeEnumStringsForList()
    {
        var schema = GenerateSchema();

        var enums = schema.Properties["stringList"]
            .Items
            .Enum
            .Cast<OpenApiString>()
            .ToList();

        Assert.Equal(2, enums.Count);
        Assert.Equal(TestEnum.Sample1.ToString(), enums[0].Value);
        Assert.Equal(TestEnum.Sample2.ToString(), enums[1].Value);
    }

    [Fact]
    public void SerializeEnumInt()
    {
        var schema = GenerateSchema();

        var enums = schema.Properties["int"]
            .Enum
            .Cast<OpenApiInteger>()
            .ToList();

        Assert.Equal(2, enums.Count);
        Assert.Equal((int)TestEnum.Sample1, enums[0].Value);
        Assert.Equal((int)TestEnum.Sample2, enums[1].Value);
    }

    [Fact]
    public void SerializeEnumLabel()
    {
        var schema = GenerateSchema();

        var enums = schema.Properties["enumLabel"]
            .Enum
            .Cast<OpenApiString>()
            .ToList();

        Assert.Equal(2, enums.Count);
        Assert.Equal(TestEnum.Sample1.GetEnumLabel(), enums[0].Value);
        Assert.Equal(TestEnum.Sample2.GetEnumLabel(), enums[1].Value);
    }

    [Fact]
    public void SerializeEnumValue()
    {
        var schema = GenerateSchema();

        var enums = schema.Properties["enumValue"]
            .Enum
            .Cast<OpenApiString>()
            .ToList();

        Assert.Equal(2, enums.Count);
        Assert.Equal(TestEnum.Sample1.GetEnumValue(), enums[0].Value);
        Assert.Equal(TestEnum.Sample2.GetEnumValue(), enums[1].Value);
    }

    [Fact]
    public void SerializeDefault()
    {
        var schema = GenerateSchema();

        var enums = schema.Properties["default"]
            .Enum
            .Cast<OpenApiInteger>()
            .ToList();

        Assert.Equal(2, enums.Count);
        Assert.Equal((int)TestEnum.Sample1, enums[0].Value);
        Assert.Equal((int)TestEnum.Sample2, enums[1].Value);
    }

    [Fact]
    public void SerializeDefault_WithGeographicLevelSchemaFilter()
    {
        _schemaGeneratorOptions.SchemaFilters.Add(new GeographicLevelSchemaFilter());

        var schemaGenerator = BuildSchemaGenerator();

        schemaGenerator.GenerateSchema(typeof(GeographicLevel), _schemaRepository);
        schemaGenerator.GenerateSchema(typeof(TestClassWithGeographicLevel), _schemaRepository);

        var schema = _schemaRepository.Schemas[nameof(TestClassWithGeographicLevel)];

        var enums = schema.Properties["geographicLevel"]
            .Enum
            .Cast<OpenApiString>()
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
            new JsonSerializerDataContractResolver(
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
            )
        );
    }

    private enum TestEnum
    {
        [EnumLabelValue("Sample 1", "sample-1")]
        Sample1,

        [EnumLabelValue("Sample 2", "sample-2")]
        Sample2,
    }

    private class TestClass
    {
        [SwaggerEnum(typeof(TestEnum), SwaggerEnumSerializer.String)]
        public string String { get; set; }

        [SwaggerEnum(typeof(TestEnum), SwaggerEnumSerializer.String)]
        public List<string> StringList { get; set; }

        [SwaggerEnum(typeof(TestEnum), SwaggerEnumSerializer.Int)]
        public int Int { get; set; }

        [SwaggerEnum(typeof(TestEnum), SwaggerEnumSerializer.Label)]
        public string EnumLabel { get; set; }

        [SwaggerEnum(typeof(TestEnum), SwaggerEnumSerializer.Value)]
        public string EnumValue { get; set; }

        [SwaggerEnum(typeof(TestEnum))]
        public string Default { get; set; }
    }

    private class TestClassWithGeographicLevel
    {
        [SwaggerEnum(typeof(GeographicLevel))]
        public string GeographicLevel { get; set; }
    }

    private class InvalidTestClass
    {
        [SwaggerEnum(typeof(string))]
        public string Invalid { get; set; }
    }
}
