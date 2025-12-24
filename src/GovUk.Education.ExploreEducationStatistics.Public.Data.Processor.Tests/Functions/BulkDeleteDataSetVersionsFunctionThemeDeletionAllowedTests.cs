using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using LinqToDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using FileInfo = System.IO.FileInfo;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class BulkDeleteDataSetVersionsFunctionThemeDeletionAllowedTestsFixture()
    : OptimisedPublicDataProcessorCollectionFixture(
        capabilities: [PublicDataProcessorIntegrationTestCapability.Postgres]
    )
{
    public BulkDeleteDataSetVersionsFunction Function = null!;

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);

        Function = lookups.GetService<BulkDeleteDataSetVersionsFunction>();
    }
}

[CollectionDefinition(nameof(BulkDeleteDataSetVersionsFunctionThemeDeletionAllowedTestsFixture))]
public class BulkDeleteDataSetVersionsFunctionThemeDeletionAllowedTestsCollection
    : ICollectionFixture<BulkDeleteDataSetVersionsFunctionThemeDeletionAllowedTestsFixture>;

[Collection(nameof(BulkDeleteDataSetVersionsFunctionThemeDeletionAllowedTestsFixture))]
public abstract class BulkDeleteDataSetVersionsFunctionThemeDeletionAllowedTests(
    BulkDeleteDataSetVersionsFunctionThemeDeletionAllowedTestsFixture fixture
) : OptimisedFunctionAppIntegrationTestBase(fixture)
{
    private static readonly DataFixture DataFixture = new();

    public class ForceDeleteTests(BulkDeleteDataSetVersionsFunctionThemeDeletionAllowedTestsFixture fixture)
        : BulkDeleteDataSetVersionsFunctionThemeDeletionAllowedTests(fixture)
    {
        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.NonDeletableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task TestTheme_ThemeDeletionAllowed_Success(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithPublication(
                    DataFixture
                        .DefaultPublication()
                        .WithTheme(DataFixture.DefaultTheme().WithTitle($"UI test theme {Guid.NewGuid()}"))
                );

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.AddRange(releaseFile);
                });

            var dataSet = DataFixture
                .DefaultDataSet()
                .WithPublicationId(releaseVersion.PublicationId)
                .WithStatusPublished()
                .Generate();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.AddRange(dataSet));

            // Data set version that is not in a deletable state without force delete
            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            // Request force deletion with support from the environment configuration
            var response = await BulkDeleteDataSetVersions(releaseVersionId: releaseVersion.Id, forceDeleteAll: true);

            response.AssertNoContent();
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.NonDeletableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task ForceDeleteNotSpecified_Returns400(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithPublication(
                    DataFixture
                        .DefaultPublication()
                        .WithTheme(DataFixture.DefaultTheme().WithTitle($"UI test theme {Guid.NewGuid()}"))
                );

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.AddRange(releaseFile);
                });

            var dataSet = DataFixture
                .DefaultDataSet()
                .WithPublicationId(releaseVersion.PublicationId)
                .WithStatusPublished()
                .Generate();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.AddRange(dataSet));

            // Data set version that is not in a deletable state without force delete
            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            // Don't explicitly request force deletion.
            var response = await BulkDeleteDataSetVersions(releaseVersionId: releaseVersion.Id, forceDeleteAll: null);

            var validationProblem = response.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "releaseVersionId",
                expectedCode: ValidationMessages.OneOrMoreDataSetVersionsCanNotBeDeleted.Code
            );
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.NonDeletableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task NonTestTheme_ThemeDeletionAllowed_Returns400(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithPublication(
                    DataFixture
                        .DefaultPublication()
                        .WithTheme(DataFixture.DefaultTheme().WithTitle($"Non-test theme {Guid.NewGuid()}"))
                );

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.AddRange(releaseFile);
                });

            var dataSet = DataFixture
                .DefaultDataSet()
                .WithPublicationId(releaseVersion.PublicationId)
                .WithStatusPublished()
                .Generate();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.AddRange(dataSet));

            // Data set version that is not in a deletable state without force delete
            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            // Request force deletion with support from the environment configuration
            // but targeting a non-Test DataSetVersion.
            var response = await BulkDeleteDataSetVersions(releaseVersionId: releaseVersion.Id, forceDeleteAll: true);

            var validationProblem = response.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "releaseVersionId",
                expectedCode: ValidationMessages.OneOrMoreDataSetVersionsCanNotBeDeleted.Code
            );
        }
    }

    private async Task<IActionResult> BulkDeleteDataSetVersions(Guid releaseVersionId, bool? forceDeleteAll = false)
    {
        var request = new Mock<HttpRequest>(MockBehavior.Strict);

        var requestParams =
            forceDeleteAll != null
                ? new Dictionary<string, StringValues> { { nameof(forceDeleteAll), forceDeleteAll + "" } }
                : new Dictionary<string, StringValues>();

        request.Setup(r => r.Query).Returns(new QueryCollection(requestParams));

        return await fixture.Function.BulkDeleteDataSetVersions(
            httpRequest: request.Object,
            releaseVersionId: releaseVersionId,
            cancellationToken: CancellationToken.None
        );
    }
}
