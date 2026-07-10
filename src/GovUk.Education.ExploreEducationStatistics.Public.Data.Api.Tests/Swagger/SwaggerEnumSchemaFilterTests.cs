using System.Text.Json;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using Microsoft.OpenApi;
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

        var propertySchema = GetPropertySchema(schema, nameof(TestClass.Ref));
        var allOfRef = GetAllOfSchemaReference(propertySchema);

        Assert.Equal(nameof(TestEnum), allOfRef.Reference.Id);
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

        var propertySchema = GetPropertySchema(schema, nameof(TestClass.RefList));
        var itemsRef = GetItemsSchemaReference(propertySchema);

        Assert.Equal(nameof(TestEnum), itemsRef.Reference.Id);
    }

    [Fact]
    public void SerializeEnumString()
    {
        var schema = GenerateSchema();

        var propertySchema = GetPropertySchema(schema, nameof(TestClass.String));
        var stringValues = GetSchemaEnumValues<string>(propertySchema);

        Assert.Equal([nameof(TestEnum.Sample1), nameof(TestEnum.Sample2)], stringValues);
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

        var propertySchema = GetPropertySchema(schema, nameof(TestClass.StringList));
        var stringValues = GetSchemaEnumValues<string>(propertySchema.Items);

        Assert.Equal([nameof(TestEnum.Sample1), nameof(TestEnum.Sample2)], stringValues);
    }

    [Fact]
    public void SerializeEnumInt()
    {
        var schema = GenerateSchema();

        var propertySchema = GetPropertySchema(schema, nameof(TestClass.Int));
        var intValues = GetSchemaEnumValues<int>(propertySchema);

        Assert.Equal([(int)TestEnum.Sample1, (int)TestEnum.Sample2], intValues);
    }

    [Fact]
    public void SerializeEnumLabel()
    {
        var schema = GenerateSchema();

        var propertySchema = GetPropertySchema(schema, nameof(TestClass.EnumLabel));
        var stringValues = GetSchemaEnumValues<string>(propertySchema);

        Assert.Equal([TestEnum.Sample1.GetEnumLabel(), TestEnum.Sample2.GetEnumLabel()], stringValues);
    }

    [Fact]
    public void SerializeEnumValue()
    {
        var schema = GenerateSchema();

        var propertySchema = GetPropertySchema(schema, nameof(TestClass.EnumValue));
        var stringValues = GetSchemaEnumValues<string>(propertySchema);

        Assert.Equal([TestEnum.Sample1.GetEnumValue(), TestEnum.Sample2.GetEnumValue()], stringValues);
    }

    [Fact]
    public void SerializeEnumSchema()
    {
        var schema = GenerateSchema();

        var propertySchema = GetPropertySchema(schema, nameof(TestClass.Schema));
        var stringValues = GetSchemaEnumValues<string>(propertySchema);

        Assert.Equal([nameof(TestEnum.Sample1), nameof(TestEnum.Sample2)], stringValues);
    }

    [Fact]
    public void SerializeEnumSchema_WithGeographicLevelSchemaFilter()
    {
        _schemaGeneratorOptions.SchemaFilters.Add(new GeographicLevelSchemaFilter());

        var schemaGenerator = BuildSchemaGenerator();

        schemaGenerator.GenerateSchema(typeof(GeographicLevel), _schemaRepository);
        schemaGenerator.GenerateSchema(typeof(TestClassWithGeographicLevel), _schemaRepository);

        var schema = _schemaRepository.Schemas[nameof(TestClassWithGeographicLevel)];

        var propertySchema = GetPropertySchema(schema, nameof(TestClassWithGeographicLevel.GeographicLevel));
        var stringValues = GetSchemaEnumValues<string>(propertySchema);

        var geographicLevels = EnumUtil.GetEnumValues<GeographicLevel>();

        Assert.Equal(stringValues.Order(), geographicLevels.Order());
    }

    [Fact]
    public void ThrowsOnInvalidEnumType()
    {
        Assert.Throws<InvalidOperationException>(GenerateInvalidSchema);
    }

    private IOpenApiSchema GenerateSchema()
    {
        var schemaGenerator = BuildSchemaGenerator();

        schemaGenerator.GenerateSchema(typeof(TestEnum), _schemaRepository);
        schemaGenerator.GenerateSchema(typeof(TestClass), _schemaRepository);

        return _schemaRepository.Schemas[nameof(TestClass)];
    }

    private IOpenApiSchema GenerateInvalidSchema()
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

    private static IOpenApiSchema GetPropertySchema(IOpenApiSchema schema, string name)
    {
        Assert.NotNull(schema.Properties);
        Assert.True(schema.Properties.TryGetValue(name, out var property), $"Schema has no property '{name}'.");
        return property;
    }

    private static OpenApiSchemaReference GetAllOfSchemaReference(IOpenApiSchema? schema)
    {
        Assert.NotNull(schema);
        Assert.NotNull(schema.AllOf);
        return Assert.IsType<OpenApiSchemaReference>(Assert.Single(schema.AllOf));
    }

    private static OpenApiSchemaReference GetItemsSchemaReference(IOpenApiSchema? schema)
    {
        Assert.NotNull(schema);
        Assert.NotNull(schema.Items);
        return Assert.IsType<OpenApiSchemaReference>(schema.Items);
    }

    private static IReadOnlyList<T> GetSchemaEnumValues<T>(IOpenApiSchema? schema)
    {
        Assert.NotNull(schema);
        Assert.NotNull(schema.Enum);
        return [.. schema.Enum.Select(e => e.GetValue<T>())];
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
