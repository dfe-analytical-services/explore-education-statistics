using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Swagger;

public class GeographicLevelSchemaFilterTests
{
    private readonly ISchemaGenerator _schemaGenerator = new SchemaGenerator(
        new SchemaGeneratorOptions
        {
            UseAllOfToExtendReferenceSchemas = true,
            SchemaFilters = [new GeographicLevelSchemaFilter()],
        },
        new JsonSerializerDataContractResolver(
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        )
    );

    private readonly SchemaRepository _schemaRepository = new("Default");

    [Fact]
    public void CorrectSchema()
    {
        var schema = GenerateSchema();
        var geographicLevels = EnumUtil.GetEnumValues<GeographicLevel>().ToHashSet();

        Assert.Equal("string", schema.Type);
        Assert.Equal(geographicLevels.Count, schema.Enum.Count);

        Assert.All(
            schema.Enum,
            e =>
            {
                var enumString = Assert.IsType<OpenApiString>(e);
                Assert.Contains(enumString.Value, geographicLevels);
            }
        );
    }

    private OpenApiSchema GenerateSchema()
    {
        _schemaGenerator.GenerateSchema(typeof(GeographicLevel), _schemaRepository);

        return _schemaRepository.Schemas[nameof(GeographicLevel)];
    }
}
