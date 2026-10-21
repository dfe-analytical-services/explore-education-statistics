#nullable enable
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentValidationKeys = GovUk.Education.ExploreEducationStatistics.Common.Validators.FluentValidationKeys;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics;

// ReSharper disable once ClassNeverInstantiated.Global
public class PairedIndicatorsControllerTestsFixture()
    : OptimisedAdminCollectionFixture(capabilities: [AdminIntegrationTestCapability.UserAuth])
{
    public Mock<IPrivateBlobCacheService> PrivateBlobCacheServiceMock = null!;

    protected override void ConfigureServicesAndConfiguration(OptimisedServiceAndConfigModifications modifications)
    {
        base.ConfigureServicesAndConfiguration(modifications);
        modifications.ReplaceServiceWithMock<IPrivateBlobCacheService>(MockBehavior.Loose);
    }

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);
        PrivateBlobCacheServiceMock = lookups.GetMockService<IPrivateBlobCacheService>();
    }
}

[CollectionDefinition(nameof(PairedIndicatorsControllerTestsFixture))]
public class PairedIndicatorsControllerTestsCollection : ICollectionFixture<PairedIndicatorsControllerTestsFixture>;

[Collection(nameof(PairedIndicatorsControllerTestsFixture))]
public abstract class PairedIndicatorsControllerTests(PairedIndicatorsControllerTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private static readonly DataFixture DataFixture = new();

    public class GetPairedIndicatorsTests(PairedIndicatorsControllerTestsFixture fixture)
        : PairedIndicatorsControllerTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var dataSet = await SeedDataSet();
            var pairedIndicators = new List<PairedIndicatorEntry>
            {
                new(ParentIndicatorId: dataSet.IndicatorIds[0], ChildIndicatorId: dataSet.IndicatorIds[1]),
            };
            await SetPairedIndicators(dataSet.ReleaseFile, pairedIndicators);

            var response = await GetPairedIndicators(dataSet.ReleaseVersion.Id, dataSet.SubjectId);

            var viewModel = response.AssertOk<PairedIndicatorsViewModel>();
            var pair = Assert.Single(viewModel.PairedIndicators);
            Assert.Equal(dataSet.IndicatorIds[0], pair.ParentIndicatorId);
            Assert.Equal(dataSet.IndicatorIds[1], pair.ChildIndicatorId);
        }

        [Fact]
        public async Task NoPairedIndicators_ReturnsEmptyList()
        {
            var dataSet = await SeedDataSet();

            var response = await GetPairedIndicators(dataSet.ReleaseVersion.Id, dataSet.SubjectId);

            var viewModel = response.AssertOk<PairedIndicatorsViewModel>();
            Assert.Empty(viewModel.PairedIndicators);
        }

        [Fact]
        public async Task DataSetNotFound()
        {
            var dataSet = await SeedDataSet();

            var response = await GetPairedIndicators(dataSet.ReleaseVersion.Id, subjectId: Guid.NewGuid());

            response.AssertNotFound();
        }

        [Fact]
        public async Task UserDoesNotHavePermission()
        {
            var dataSet = await SeedDataSet();

            var response = await GetPairedIndicators(
                dataSet.ReleaseVersion.Id,
                dataSet.SubjectId,
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> GetPairedIndicators(
            Guid releaseVersionId,
            Guid subjectId,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            return await client.GetAsync(PairedIndicatorsUrl(releaseVersionId, subjectId));
        }
    }

    public class UpdatePairedIndicatorsTests(PairedIndicatorsControllerTestsFixture fixture)
        : PairedIndicatorsControllerTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var dataSet = await SeedDataSet();

            var response = await UpdatePairedIndicators(
                dataSet.ReleaseVersion.Id,
                dataSet.SubjectId,
                [
                    Pair(dataSet.IndicatorIds[0], dataSet.IndicatorIds[1]),
                    Pair(dataSet.IndicatorIds[2], dataSet.IndicatorIds[3]),
                ]
            );

            var viewModel = response.AssertOk<PairedIndicatorsViewModel>();
            Assert.Equal(2, viewModel.PairedIndicators.Count);

            var savedPairedIndicators = await GetSavedPairedIndicators(dataSet.ReleaseFile.Id);
            savedPairedIndicators.AssertDeepEqualTo([
                new PairedIndicatorEntry(dataSet.IndicatorIds[0], dataSet.IndicatorIds[1]),
                new PairedIndicatorEntry(dataSet.IndicatorIds[2], dataSet.IndicatorIds[3]),
            ]);

            var expectedCacheKey = new PrivateSubjectMetaCacheKey(dataSet.ReleaseVersion.Id, dataSet.SubjectId);
            fixture.PrivateBlobCacheServiceMock.Verify(
                mock => mock.DeleteItemAsync(It.Is<IBlobCacheKey>(key => key.Key == expectedCacheKey.Key)),
                Times.Once
            );
        }

        [Fact]
        public async Task EmptyList_RemovesPairedIndicators()
        {
            var dataSet = await SeedDataSet();
            await SetPairedIndicators(
                dataSet.ReleaseFile,
                [new PairedIndicatorEntry(dataSet.IndicatorIds[0], dataSet.IndicatorIds[1])]
            );

            var response = await UpdatePairedIndicators(dataSet.ReleaseVersion.Id, dataSet.SubjectId, []);

            var viewModel = response.AssertOk<PairedIndicatorsViewModel>();
            Assert.Empty(viewModel.PairedIndicators);
            Assert.Null(await GetSavedPairedIndicators(dataSet.ReleaseFile.Id));
        }

        [Fact]
        public async Task OtherReleaseVersionsUsingTheSameDataFileAreUnchanged()
        {
            var dataSet = await SeedDataSet(publishedVersions: 1);

            // Link the already seeded release version and data file by id
            var publishedReleaseFile = new ReleaseFile
            {
                ReleaseVersionId = dataSet.PublishedReleaseVersion!.Id,
                FileId = dataSet.ReleaseFile.FileId,
                PairedIndicators = [new PairedIndicatorEntry(dataSet.IndicatorIds[0], dataSet.IndicatorIds[1])],
            };
            await fixture.GetContentDbContext().AddTestData(context => context.ReleaseFiles.Add(publishedReleaseFile));

            var response = await UpdatePairedIndicators(
                dataSet.ReleaseVersion.Id,
                dataSet.SubjectId,
                [Pair(dataSet.IndicatorIds[2], dataSet.IndicatorIds[3])]
            );

            response.AssertOk<PairedIndicatorsViewModel>();

            (await GetSavedPairedIndicators(publishedReleaseFile.Id)).AssertDeepEqualTo([
                new PairedIndicatorEntry(dataSet.IndicatorIds[0], dataSet.IndicatorIds[1]),
            ]);
            (await GetSavedPairedIndicators(dataSet.ReleaseFile.Id)).AssertDeepEqualTo([
                new PairedIndicatorEntry(dataSet.IndicatorIds[2], dataSet.IndicatorIds[3]),
            ]);
        }

        [Fact]
        public async Task PublishedReleaseVersion_Forbidden()
        {
            var dataSet = await SeedDataSet(publishedVersions: 1, draftVersion: false);

            var response = await UpdatePairedIndicators(
                dataSet.ReleaseVersion.Id,
                dataSet.SubjectId,
                [Pair(dataSet.IndicatorIds[0], dataSet.IndicatorIds[1])]
            );

            response.AssertForbidden();
            Assert.Null(await GetSavedPairedIndicators(dataSet.ReleaseFile.Id));
        }

        [Fact]
        public async Task DataSetNotFound()
        {
            var dataSet = await SeedDataSet();

            var response = await UpdatePairedIndicators(
                dataSet.ReleaseVersion.Id,
                subjectId: Guid.NewGuid(),
                [Pair(dataSet.IndicatorIds[0], dataSet.IndicatorIds[1])]
            );

            response.AssertNotFound();
        }

        [Fact]
        public async Task UserDoesNotHavePermission()
        {
            var dataSet = await SeedDataSet();

            var response = await UpdatePairedIndicators(
                dataSet.ReleaseVersion.Id,
                dataSet.SubjectId,
                [Pair(dataSet.IndicatorIds[0], dataSet.IndicatorIds[1])],
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
            Assert.Null(await GetSavedPairedIndicators(dataSet.ReleaseFile.Id));
        }

        [Fact]
        public async Task ImportNotComplete()
        {
            var dataSet = await SeedDataSet(importStatus: DataImportStatus.STAGE_2);

            var response = await UpdatePairedIndicators(
                dataSet.ReleaseVersion.Id,
                dataSet.SubjectId,
                [Pair(dataSet.IndicatorIds[0], dataSet.IndicatorIds[1])]
            );

            AssertValidationError(response, ValidationMessages.PairedIndicatorsImportNotComplete);
            Assert.Null(await GetSavedPairedIndicators(dataSet.ReleaseFile.Id));
        }

        [Fact]
        public async Task IndicatorNotForSubject()
        {
            var dataSet = await SeedDataSet();
            var otherDataSet = await SeedDataSet();
            var existingPairedIndicators = new List<PairedIndicatorEntry>
            {
                new(dataSet.IndicatorIds[0], dataSet.IndicatorIds[1]),
            };
            await SetPairedIndicators(dataSet.ReleaseFile, existingPairedIndicators);

            var response = await UpdatePairedIndicators(
                dataSet.ReleaseVersion.Id,
                dataSet.SubjectId,
                [Pair(dataSet.IndicatorIds[0], otherDataSet.IndicatorIds[0])]
            );

            AssertValidationError(response, ValidationMessages.PairedIndicatorsIndicatorNotForSubject);
            (await GetSavedPairedIndicators(dataSet.ReleaseFile.Id)).AssertDeepEqualTo(existingPairedIndicators);
        }

        [Fact]
        public async Task InvalidRequest()
        {
            var dataSet = await SeedDataSet();

            var response = await UpdatePairedIndicators(
                dataSet.ReleaseVersion.Id,
                dataSet.SubjectId,
                [
                    Pair(dataSet.IndicatorIds[0], dataSet.IndicatorIds[1]),
                    Pair(dataSet.IndicatorIds[1], dataSet.IndicatorIds[2]),
                ]
            );

            AssertValidationError(response, ValidationMessages.PairedIndicatorsCannotBeChained);
            Assert.Null(await GetSavedPairedIndicators(dataSet.ReleaseFile.Id));
        }

        [Fact]
        public async Task NullPair_ReturnsValidationError()
        {
            var dataSet = await SeedDataSet();

            var client = fixture.CreateClient(user: OptimisedTestUsers.Bau);

            var response = await client.PutAsync(
                PairedIndicatorsUrl(dataSet.ReleaseVersion.Id, dataSet.SubjectId),
                new StringContent("""{ "pairedIndicators": [null] }""", Encoding.UTF8, "application/json")
            );

            var validationProblem = response.AssertValidationProblem();
            // The "Validator" suffix is dropped from FluentValidation error codes in responses
            var error = Assert.Single(validationProblem.Errors);
            Assert.Equal(FluentValidationKeys.NotNullValidator.Replace("Validator", ""), error.Code);
            Assert.Equal("pairedIndicators[0]", error.Path);
            Assert.Null(await GetSavedPairedIndicators(dataSet.ReleaseFile.Id));
        }

        private async Task<HttpResponseMessage> UpdatePairedIndicators(
            Guid releaseVersionId,
            Guid subjectId,
            List<PairedIndicatorRequest> pairedIndicators,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            return await client.PutAsJsonAsync(
                PairedIndicatorsUrl(releaseVersionId, subjectId),
                new PairedIndicatorsUpdateRequest { PairedIndicators = pairedIndicators }
            );
        }

        private static PairedIndicatorRequest Pair(Guid parentIndicatorId, Guid childIndicatorId) =>
            new() { ParentIndicatorId = parentIndicatorId, ChildIndicatorId = childIndicatorId };

        private static void AssertValidationError(HttpResponseMessage response, LocalizableMessage expected)
        {
            var validationProblem = response.AssertValidationProblem();
            var error = Assert.Single(validationProblem.Errors);
            Assert.Equal(expected.Code, error.Code);
        }
    }

    private record SeededDataSet(
        ReleaseVersion ReleaseVersion,
        ReleaseVersion? PublishedReleaseVersion,
        ReleaseFile ReleaseFile,
        Guid SubjectId,
        List<Guid> IndicatorIds
    );

    private async Task<SeededDataSet> SeedDataSet(
        DataImportStatus importStatus = DataImportStatus.COMPLETE,
        int publishedVersions = 0,
        bool draftVersion = true
    )
    {
        Publication publication = DataFixture
            .DefaultPublication()
            .WithReleases([
                DataFixture.DefaultRelease(publishedVersions: publishedVersions, draftVersion: draftVersion),
            ]);

        var versions = publication.Releases[0].Versions;
        var releaseVersion = versions[^1];
        var publishedReleaseVersion = draftVersion && publishedVersions > 0 ? versions[0] : null;

        Subject subject = DataFixture.DefaultSubject();
        IndicatorGroup indicatorGroup = DataFixture.DefaultIndicatorGroup(indicatorCount: 4).WithSubject(subject);

        var file = DataFixture.DefaultFile().WithType(FileType.Data).WithSubjectId(subject.Id).Generate();

        ReleaseFile releaseFile = DataFixture.DefaultReleaseFile().WithReleaseVersion(releaseVersion).WithFile(file);

        DataImport dataImport = DataFixture.DefaultDataImport().WithFile(file).WithStatus(importStatus);

        await fixture
            .GetContentDbContext()
            .AddTestData(context =>
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.Add(releaseFile);
                context.DataImports.Add(dataImport);
            });

        await fixture.GetStatisticsDbContext().AddTestData(context => context.IndicatorGroup.Add(indicatorGroup));

        return new SeededDataSet(
            ReleaseVersion: releaseVersion,
            PublishedReleaseVersion: publishedReleaseVersion,
            ReleaseFile: releaseFile,
            SubjectId: subject.Id,
            IndicatorIds: indicatorGroup.Indicators.Select(indicator => indicator.Id).ToList()
        );
    }

    private async Task SetPairedIndicators(ReleaseFile releaseFile, List<PairedIndicatorEntry> pairedIndicators)
    {
        var context = fixture.GetContentDbContext();
        var savedReleaseFile = await context.ReleaseFiles.SingleAsync(rf => rf.Id == releaseFile.Id);
        savedReleaseFile.PairedIndicators = pairedIndicators;
        await context.SaveChangesAsync();
    }

    private async Task<List<PairedIndicatorEntry>?> GetSavedPairedIndicators(Guid releaseFileId)
    {
        var releaseFile = await fixture
            .GetContentDbContext()
            .ReleaseFiles.AsNoTracking()
            .SingleAsync(rf => rf.Id == releaseFileId);

        return releaseFile.PairedIndicators;
    }

    private static string PairedIndicatorsUrl(Guid releaseVersionId, Guid subjectId) =>
        $"api/data/release/{releaseVersionId}/meta/subject/{subjectId}/paired-indicators";
}
