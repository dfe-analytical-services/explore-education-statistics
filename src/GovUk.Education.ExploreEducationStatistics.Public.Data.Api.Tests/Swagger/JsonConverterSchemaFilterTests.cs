using System.Text.Json;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Swagger;

public class JsonConverterSchemaFilterTests
{
    private readonly SchemaGenerator _schemaGenerator = new(
        new SchemaGeneratorOptions
        {
            UseAllOfToExtendReferenceSchemas = true,
            SchemaFilters = [new JsonConverterSchemaFilter()],
        },
        new JsonSerializerDataContractResolver(
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        )
    );

    private readonly SchemaRepository _schemaRepository = new("Default");

    [Theory]
    [InlineData(nameof(TestEnumConverters.StringEnum))]
    [InlineData(nameof(TestEnumConverters.StringEnumTyped))]
    public void JsonStringEnumConverters(string propertyName)
    {
        var schema = GenerateSchema<TestEnumConverters>();

        var schemaPropertyName = propertyName.CamelCase();
        var propertySchema = GetPropertySchema(schema, schemaPropertyName);

        Assert.NotNull(propertySchema.AllOf);
        Assert.Empty(propertySchema.AllOf);

        Assert.Equal(JsonSchemaType.String, propertySchema.Type);
        Assert.NotNull(propertySchema.Enum);
        Assert.Equal(2, propertySchema.Enum.Count);

        Assert.Equal(nameof(TestEnum.Sample1), propertySchema.Enum[0].GetValue<string>());
        Assert.Equal(nameof(TestEnum.Sample2), propertySchema.Enum[1].GetValue<string>());
    }

    [Fact]
    public void EnumToEnumLabelConverter()
    {
        var schema = GenerateSchema<TestEnumConverters>();

        var schemaPropertyName = nameof(TestEnumConverters.EnumToEnumLabel).CamelCase();
        var propertySchema = GetPropertySchema(schema, schemaPropertyName);

        Assert.NotNull(propertySchema.AllOf);
        Assert.Empty(propertySchema.AllOf);

        Assert.Equal(JsonSchemaType.String, propertySchema.Type);
        Assert.NotNull(propertySchema.Enum);
        Assert.Equal(2, propertySchema.Enum.Count);

        Assert.Equal(TestEnum.Sample1.GetEnumLabel(), propertySchema.Enum[0].GetValue<string>());
        Assert.Equal(TestEnum.Sample2.GetEnumLabel(), propertySchema.Enum[1].GetValue<string>());
    }

    [Fact]
    public void EnumToEnumValueConverter()
    {
        var schema = GenerateSchema<TestEnumConverters>();

        var schemaPropertyName = nameof(TestEnumConverters.EnumToEnumValue).CamelCase();
        var propertySchema = GetPropertySchema(schema, schemaPropertyName);

        Assert.NotNull(propertySchema.AllOf);
        Assert.Empty(propertySchema.AllOf);

        Assert.Equal(JsonSchemaType.String, propertySchema.Type);
        Assert.NotNull(propertySchema.Enum);
        Assert.Equal(2, propertySchema.Enum.Count);

        Assert.Equal(TestEnum.Sample1.GetEnumValue(), propertySchema.Enum[0].GetValue<string>());
        Assert.Equal(TestEnum.Sample2.GetEnumValue(), propertySchema.Enum[1].GetValue<string>());
    }

    [Fact]
    public void ListConverter_GeographicLevelEnumLabels()
    {
        var schema = GenerateSchema<TestReadOnlyListEnumConverters>();

        var schemaPropertyName = nameof(TestReadOnlyListEnumConverters.GeographicLevelEnumLabels).CamelCase();
        var propertySchema = GetPropertySchema(schema, schemaPropertyName);

        Assert.Null(propertySchema.AllOf);

        // TODO EES-6893
        // Assert.Equal(JsonSchemaType.Array, propertySchema.Type);

        var items = GetItemsSchemaReference(propertySchema);
        // TODO EES-6893
        //Assert.Null(items.Reference);
        Assert.Null(items.AllOf);

        // TODO EES-6893
        // var enumStrings = GetSchemaEnumValues<string>(items);
        // Assert.Equal(EnumUtil.GetEnumLabels<GeographicLevel>(), enumStrings);
    }

    [Fact]
    public void ListConverter_GeographicLevelEnumValues()
    {
        var schema = GenerateSchema<TestReadOnlyListEnumConverters>();

        var schemaPropertyName = nameof(TestReadOnlyListEnumConverters.GeographicLevelEnumValues).CamelCase();
        var propertySchema = GetPropertySchema(schema, schemaPropertyName);

        Assert.Null(propertySchema.AllOf);

        // TODO EES-6893
        // Assert.Equal(JsonSchemaType.Array, propertySchema.Type);

        var items = GetItemsSchemaReference(propertySchema);
        // TODO EES-6893
        // Assert.Null(items.Reference);
        Assert.Null(items.AllOf);

        // TODO EES-6893
        // var enumStrings = GetSchemaEnumValues<string>(items);
        // Assert.Equal(EnumUtil.GetEnumValues<GeographicLevel>(), enumStrings);
    }

    [Theory]
    [InlineData(nameof(TestIgnoredGeographicLevelConverters.StringEnum))]
    [InlineData(nameof(TestIgnoredGeographicLevelConverters.StringEnumTyped))]
    [InlineData(nameof(TestIgnoredGeographicLevelConverters.EnumToEnumLabel))]
    [InlineData(nameof(TestIgnoredGeographicLevelConverters.EnumToEnumValue))]
    public void GeographicLevel_Ignored(string propertyName)
    {
        var schema = GenerateSchema<TestIgnoredGeographicLevelConverters>();

        var schemaPropertyName = propertyName.CamelCase();
        var propertySchema = GetPropertySchema(schema, schemaPropertyName);

        Assert.NotNull(propertySchema.AllOf);
        Assert.NotEmpty(propertySchema.AllOf);

        Assert.Null(propertySchema.Type);
        Assert.Null(propertySchema.Enum);
    }

    [Theory]
    [InlineData(nameof(TestIgnoredIndicatorUnitConverters.StringEnum))]
    [InlineData(nameof(TestIgnoredIndicatorUnitConverters.StringEnumTyped))]
    [InlineData(nameof(TestIgnoredIndicatorUnitConverters.EnumToEnumLabel))]
    [InlineData(nameof(TestIgnoredIndicatorUnitConverters.EnumToEnumValue))]
    public void IndicatorUnit_Ignored(string propertyName)
    {
        var schema = GenerateSchema<TestIgnoredIndicatorUnitConverters>();

        var schemaPropertyName = propertyName.CamelCase();
        var propertySchema = GetPropertySchema(schema, schemaPropertyName);

        Assert.NotNull(propertySchema.AllOf);
        Assert.NotEmpty(propertySchema.AllOf);

        Assert.Null(propertySchema.Type);
        Assert.Null(propertySchema.Enum);
    }

    [Theory]
    [InlineData(nameof(TestIgnoredTimeIdentifierConverters.StringEnum))]
    [InlineData(nameof(TestIgnoredTimeIdentifierConverters.StringEnumTyped))]
    [InlineData(nameof(TestIgnoredTimeIdentifierConverters.EnumToEnumLabel))]
    [InlineData(nameof(TestIgnoredTimeIdentifierConverters.EnumToEnumValue))]
    public void TimeIdentifier_Ignored(string propertyName)
    {
        var schema = GenerateSchema<TestIgnoredTimeIdentifierConverters>();

        var schemaPropertyName = propertyName.CamelCase();
        var propertySchema = GetPropertySchema(schema, schemaPropertyName);

        Assert.NotNull(propertySchema.AllOf);
        Assert.NotEmpty(propertySchema.AllOf);

        Assert.Null(propertySchema.Type);
        Assert.Null(propertySchema.Enum);
    }

    private IOpenApiSchema GenerateSchema<TConverters>()
    {
        _schemaGenerator.GenerateSchema(typeof(TestEnum), _schemaRepository);
        _schemaGenerator.GenerateSchema(typeof(TConverters), _schemaRepository);

        return _schemaRepository.Schemas[typeof(TConverters).Name];
    }

    private static IOpenApiSchema GetPropertySchema(IOpenApiSchema schema, string name)
    {
        Assert.NotNull(schema.Properties);
        Assert.True(schema.Properties.TryGetValue(name, out var property), $"Schema has no property '{name}'.");
        return property;
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

    // ReSharper disable once ClassNeverInstantiated.Local
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
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class TestIgnoredGeographicLevelConverters
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GeographicLevel StringEnum { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter<GeographicLevel>))]
        public GeographicLevel StringEnumTyped { get; init; }

        [JsonConverter(typeof(EnumToEnumLabelJsonConverter<GeographicLevel>))]
        public GeographicLevel EnumToEnumLabel { get; init; }

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<GeographicLevel>))]
        public GeographicLevel EnumToEnumValue { get; init; }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class TestIgnoredIndicatorUnitConverters
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public IndicatorUnit StringEnum { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter<IndicatorUnit>))]
        public IndicatorUnit StringEnumTyped { get; init; }

        [JsonConverter(typeof(EnumToEnumLabelJsonConverter<IndicatorUnit>))]
        public IndicatorUnit EnumToEnumLabel { get; init; }

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<IndicatorUnit>))]
        public IndicatorUnit EnumToEnumValue { get; init; }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class TestIgnoredTimeIdentifierConverters
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TimeIdentifier StringEnum { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter<TimeIdentifier>))]
        public TimeIdentifier StringEnumTyped { get; init; }

        [JsonConverter(typeof(EnumToEnumLabelJsonConverter<TimeIdentifier>))]
        public TimeIdentifier EnumToEnumLabel { get; init; }

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
        public TimeIdentifier EnumToEnumValue { get; init; }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class TestReadOnlyListEnumConverters
    {
        [JsonConverter(
            typeof(ReadOnlyListJsonConverter<GeographicLevel, EnumToEnumLabelJsonConverter<GeographicLevel>>)
        )]
        public IReadOnlyList<GeographicLevel> GeographicLevelEnumLabels { get; init; } = [];

        [JsonConverter(
            typeof(ReadOnlyListJsonConverter<GeographicLevel, EnumToEnumValueJsonConverter<GeographicLevel>>)
        )]
        public IReadOnlyList<GeographicLevel> GeographicLevelEnumValues { get; init; } = [];
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private enum TestEnum
    {
        [EnumLabelValue("Sample 1", "sample-1")]
        Sample1,

        [EnumLabelValue("Sample 2", "sample-2")]
        Sample2,
    }
}
