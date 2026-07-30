using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS9107

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public class UnfinaliseDataSetVersionFunctionTestsFixture()
    : OptimisedPublicDataProcessorCollectionFixture(
        capabilities: [PublicDataProcessorIntegrationTestCapability.Postgres]
    )
{
    public UnfinaliseDataSetVersionFunction Function = null!;

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);
        Function = lookups.GetService<UnfinaliseDataSetVersionFunction>();
    }
}

[CollectionDefinition(nameof(UnfinaliseDataSetVersionFunctionTestsFixture))]
public class UnfinaliseDataSetVersionFunctionTestsCollection
    : ICollectionFixture<UnfinaliseDataSetVersionFunctionTestsFixture>;

[Collection(nameof(UnfinaliseDataSetVersionFunctionTestsFixture))]
public class UnfinaliseDataSetVersionFunctionTests(UnfinaliseDataSetVersionFunctionTestsFixture fixture)
    : OptimisedFunctionAppIntegrationTestBase(fixture)
{
    private static readonly DataFixture DataFixture = new();

    [Theory]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    public async Task Success_PreservesVersionMappingImportSourcesAndPreviewToken(int minor, int patch)
    {
        var dataSet = DataFixture.DefaultDataSet().WithStatusDraft().Generate();
        var sourceVersion = DataFixture
            .DefaultDataSetVersion()
            .WithDataSet(dataSet)
            .WithVersionNumber(major: 1, minor: 0)
            .WithStatusPublished()
            .Generate();
        var import = DataFixture
            .DefaultDataSetVersionImport()
            .WithStage(DataSetVersionImportStage.Completing)
            .Generate();
        import.Completed = DateTimeOffset.UtcNow;
        var targetVersion = DataFixture
            .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
            .WithDataSet(dataSet)
            .WithVersionNumber(major: 1, minor: minor, patch: patch)
            .WithStatusDraft()
            .WithImports(() => [import])
            .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
            .Generate();
        var mapping = DataFixture
            .DefaultDataSetVersionMapping()
            .WithSourceDataSetVersion(sourceVersion)
            .WithTargetDataSetVersion(targetVersion)
            .Generate();

        await fixture
            .GetPublicDataDbContext()
            .AddTestData(context =>
            {
                context.DataSetVersions.AddRange(sourceVersion, targetVersion);
                context.DataSetVersionMappings.Add(mapping);
            });

        var pathResolver = fixture.GetDataSetVersionPathResolver();
        Directory.CreateDirectory(pathResolver.DirectoryPath(targetVersion));
        var sourcePaths = new[]
        {
            pathResolver.CsvDataPath(targetVersion),
            pathResolver.CsvMetadataPath(targetVersion),
        };
        var derivedPaths = new[]
        {
            pathResolver.DuckDbPath(targetVersion),
            pathResolver.DuckDbLoadSqlPath(targetVersion),
            pathResolver.DuckDbSchemaSqlPath(targetVersion),
            pathResolver.DataPath(targetVersion),
            pathResolver.FiltersPath(targetVersion),
            pathResolver.IndicatorsPath(targetVersion),
            pathResolver.LocationsPath(targetVersion),
            pathResolver.TimePeriodsPath(targetVersion),
        };
        foreach (var path in sourcePaths.Concat(derivedPaths))
        {
            await System.IO.File.WriteAllTextAsync(path, "test");
        }

        var result = await fixture.Function.UnfinaliseDataSetVersion(null!, targetVersion.Id, CancellationToken.None);
        result.AssertNoContent();

        var updatedVersion = await fixture
            .GetPublicDataDbContext()
            .DataSetVersions.Include(version => version.Imports)
            .Include(version => version.PreviewTokens)
            .SingleAsync(version => version.Id == targetVersion.Id);
        Assert.Equal(DataSetVersionStatus.Mapping, updatedVersion.Status);
        Assert.Equal(0, updatedVersion.TotalResults);
        Assert.Null(updatedVersion.MetaSummary);
        Assert.Equal(DataSetVersionImportStage.ManualMapping, updatedVersion.Imports.Single().Stage);
        Assert.Null(updatedVersion.Imports.Single().Completed);
        Assert.Single(updatedVersion.PreviewTokens);
        Assert.True(
            await fixture
                .GetPublicDataDbContext()
                .DataSetVersionMappings.AnyAsync(existing => existing.Id == mapping.Id)
        );
        Assert.All(sourcePaths, path => Assert.True(System.IO.File.Exists(path)));
        Assert.All(derivedPaths, path => Assert.False(System.IO.File.Exists(path)));
        Assert.False(
            await fixture
                .GetPublicDataDbContext()
                .FilterMetas.AnyAsync(meta => meta.DataSetVersionId == targetVersion.Id)
        );
        Assert.False(
            await fixture
                .GetPublicDataDbContext()
                .IndicatorMetas.AnyAsync(meta => meta.DataSetVersionId == targetVersion.Id)
        );
    }

    [Theory]
    [InlineData(DataSetVersionStatus.Mapping)]
    [InlineData(DataSetVersionStatus.Processing)]
    [InlineData(DataSetVersionStatus.Finalising)]
    [InlineData(DataSetVersionStatus.Published)]
    [InlineData(DataSetVersionStatus.Failed)]
    [InlineData(DataSetVersionStatus.Cancelled)]
    public async Task InvalidStatus_ReturnsValidationProblem(DataSetVersionStatus status)
    {
        var dataSet = DataFixture.DefaultDataSet().WithStatusDraft().Generate();
        var version = DataFixture
            .DefaultDataSetVersion()
            .WithDataSet(dataSet)
            .WithVersionNumber(major: 1, minor: 1)
            .WithStatus(status)
            .WithImports(() => DataFixture.DefaultDataSetVersionImport().Generate(1))
            .Generate();
        await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersions.Add(version));

        var result = await fixture.Function.UnfinaliseDataSetVersion(null!, version.Id, CancellationToken.None);

        var validationProblem = result.AssertBadRequestWithValidationProblem();
        validationProblem.AssertHasError(
            expectedPath: "dataSetVersionId",
            expectedCode: ValidationMessages.DataSetVersionCanNotBeUnfinalised.Code
        );
    }

    [Fact]
    public async Task VersionDoesNotExist_ReturnsNotFound()
    {
        var result = await fixture.Function.UnfinaliseDataSetVersion(null!, Guid.NewGuid(), CancellationToken.None);

        result.AssertNotFoundResult();
    }
}
