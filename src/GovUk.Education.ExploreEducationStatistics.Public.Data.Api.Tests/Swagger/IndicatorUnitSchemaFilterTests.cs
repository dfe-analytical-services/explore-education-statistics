using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Swagger;

public class IndicatorUnitSchemaFilterTests
{
    private readonly SchemaGenerator _schemaGenerator = new(
        new SchemaGeneratorOptions
        {
            UseAllOfToExtendReferenceSchemas = true,
            SchemaFilters = [new IndicatorUnitSchemaFilter()],
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
        var indicatorUnits = EnumUtil.GetEnumLabels<IndicatorUnit>().ToHashSet();

        Assert.Equal("string", schema.Type);
        Assert.Equal(indicatorUnits.Count, schema.Enum.Count);

        Assert.All(
            schema.Enum,
            e =>
            {
                var enumString = Assert.IsType<OpenApiString>(e);
                Assert.Contains(enumString.Value, indicatorUnits);
            }
        );
    }

    private OpenApiSchema GenerateSchema()
    {
        _schemaGenerator.GenerateSchema(typeof(IndicatorUnit), _schemaRepository);

        return _schemaRepository.Schemas[nameof(IndicatorUnit)];
    }
}
