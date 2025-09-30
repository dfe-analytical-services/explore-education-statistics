using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Swagger;

public class RequiredPropertySchemaFilterTests
{
    private readonly SchemaGenerator _schemaGenerator = new(
        new SchemaGeneratorOptions
        {
            UseAllOfToExtendReferenceSchemas = true,
            SchemaFilters = [new RequiredPropertySchemaFilter()],
            SupportNonNullableReferenceTypes = true,
        },
        new JsonSerializerDataContractResolver(
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        )
    );

    private readonly SchemaRepository _schemaRepository = new("Default");

    [Fact]
    public void AllRequiredPropertiesMarkedAsRequired()
    {
        var schema = GenerateSchema<TestClassAllRequired>();

        Assert.Equal(3, schema.Required.Count);
        Assert.Contains(nameof(TestClassAllRequired.FirstName).ToLowerFirst(), schema.Required);
        Assert.Contains(nameof(TestClassAllRequired.LastName).ToLowerFirst(), schema.Required);
        Assert.Contains(nameof(TestClassAllRequired.Age).ToLowerFirst(), schema.Required);
    }

    [Fact]
    public void SomeRequiredPropertiesMarkedAsRequired()
    {
        var schema = GenerateSchema<TestClassSomeRequired>();

        Assert.Single(schema.Required);
        Assert.Contains(nameof(TestClassSomeRequired.FirstName).ToLowerFirst(), schema.Required);
    }

    [Fact]
    public void RequiredReferenceTypePropertiesMarkedAsRequired()
    {
        var schema = GenerateSchema<TestClassReferenceTypes>();

        Assert.Equal(2, schema.Required.Count);
        Assert.Contains(
            nameof(TestClassReferenceTypes.RequiredNonNullable).ToLowerFirst(),
            schema.Required
        );
        Assert.Contains(
            nameof(TestClassReferenceTypes.RequiredNullable).ToLowerFirst(),
            schema.Required
        );
    }

    [Fact]
    public void RequiredNullableReferenceTypeMarkedAsNullable()
    {
        var schema = GenerateSchema<TestClassReferenceTypes>();

        Assert.True(
            schema
                .Properties[nameof(TestClassReferenceTypes.RequiredNullable).ToLowerFirst()]
                .Nullable
        );
        Assert.False(
            schema
                .Properties[nameof(TestClassReferenceTypes.RequiredNonNullable).ToLowerFirst()]
                .Nullable
        );
    }

    [Fact]
    public void JsonIgnoredPropertiesNotMarkedAsRequired()
    {
        var schema = GenerateSchema<TestClassJsonIgnored>();

        Assert.Empty(schema.Required);
    }

    [Fact]
    public void JsonPropertyNamesMarkedAsRequired()
    {
        var schema = GenerateSchema<TestClassJsonPropertyName>();

        Assert.Equal(2, schema.Required.Count);

        Assert.Contains("GivenName", schema.Required);
        Assert.Contains("AgeYears", schema.Required);
    }

    private OpenApiSchema GenerateSchema<T>()
    {
        _schemaGenerator.GenerateSchema(typeof(T), _schemaRepository);

        return _schemaRepository.Schemas[typeof(T).Name];
    }

    private record TestClassAllRequired
    {
        public required string FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        public required int Age { get; set; }
    }

    private record TestClassSomeRequired
    {
        public required string FirstName { get; set; }

        public string? LastName { get; set; }

        public int? Age { get; set; }
    }

    public record TestClassReferenceTypes
    {
        public required TestReference RequiredNonNullable { set; get; }

        public required TestReference? RequiredNullable { get; set; }

        public TestReference NonNullable { set; get; } = null!;

        public TestReference? Nullable { get; set; }
    }

    public record TestReference;

    private record TestClassJsonIgnored
    {
        [JsonIgnore]
        public required string FirstName { get; set; }

        [JsonIgnore]
        public required int Age { get; set; }
    }

    private record TestClassJsonPropertyName
    {
        [JsonPropertyName("GivenName")]
        public required string FirstName { get; set; }

        [JsonPropertyName("FamilyName")]
        public string? LastName { get; set; }

        [JsonPropertyName("AgeYears")]
        public required int Age { get; set; }
    }
}
