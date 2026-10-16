#nullable enable
using System.Net.Http.Json;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics;

// ReSharper disable once ClassNeverInstantiated.Global
public class TableBuilderMetaControllerTestsFixture()
    : OptimisedAdminCollectionFixture(capabilities: [AdminIntegrationTestCapability.UserAuth])
{
    public Mock<IPrivateBlobCacheService> PrivateBlobCacheServiceMock = null!;

    protected override void ConfigureServicesAndConfiguration(OptimisedServiceAndConfigModifications modifications)
    {
        base.ConfigureServicesAndConfiguration(modifications);
        modifications.ReplaceServiceWithMock<IPrivateBlobCacheService>();
    }

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);
        PrivateBlobCacheServiceMock = lookups.GetMockService<IPrivateBlobCacheService>();
    }

    public override async Task BeforeEachTest()
    {
        await base.BeforeEachTest();
        PrivateBlobCacheServiceMock.Reset();
    }
}

[CollectionDefinition(nameof(TableBuilderMetaControllerTestsFixture))]
public class TableBuilderMetaControllerTestsCollection : ICollectionFixture<TableBuilderMetaControllerTestsFixture>;

[Collection(nameof(TableBuilderMetaControllerTestsFixture))]
public abstract class TableBuilderMetaControllerTests(TableBuilderMetaControllerTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private static readonly DataFixture DataFixture = new();

    private async Task<ReleaseFile> SetUpReleaseFile(bool published = false)
    {
        Release release = published
            ? DataFixture.DefaultRelease(publishedVersions: 1)
            : DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true);

        // Subjects have no filters or indicator groups in the statistics database, so an empty
        // update request passes validation
        ReleaseFile releaseFile = DataFixture
            .DefaultReleaseFile()
            .WithReleaseVersion(release.Versions.Single())
            .WithFile(DataFixture.DefaultFile(FileType.Data).WithSubjectId(Guid.NewGuid()));

        await fixture.GetContentDbContext().AddTestData(context => context.ReleaseFiles.Add(releaseFile));

        return releaseFile;
    }

    private async Task<ReleaseFile> GetReleaseFile(Guid releaseFileId) =>
        await fixture.GetContentDbContext().ReleaseFiles.AsNoTracking().SingleAsync(rf => rf.Id == releaseFileId);

    private void SetUpCacheInvalidation(ReleaseFile releaseFile)
    {
        fixture
            .PrivateBlobCacheServiceMock.Setup(s =>
                s.DeleteItemAsync(
                    ItIs.DeepEqualTo(
                        new PrivateSubjectMetaCacheKey(releaseFile.ReleaseVersionId, releaseFile.File.SubjectId!.Value)
                    )
                )
            )
            .Returns(Task.CompletedTask);
    }

    public class UpdateFiltersTests(TableBuilderMetaControllerTestsFixture fixture)
        : TableBuilderMetaControllerTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var releaseFile = await SetUpReleaseFile();
            SetUpCacheInvalidation(releaseFile);

            var response = await UpdateFilters(releaseFile, OptimisedTestUsers.Bau);

            response.AssertOk();
            VerifyAllMocks(fixture.PrivateBlobCacheServiceMock);
            Assert.Empty((await GetReleaseFile(releaseFile.Id)).FilterSequence!);
        }

        [Fact]
        public async Task AuthenticatedUser_Forbidden()
        {
            var releaseFile = await SetUpReleaseFile();

            var response = await UpdateFilters(releaseFile, OptimisedTestUsers.Authenticated);

            response.AssertForbidden();
            Assert.Null((await GetReleaseFile(releaseFile.Id)).FilterSequence);
        }

        [Fact]
        public async Task PublishedReleaseVersion_Forbidden()
        {
            var releaseFile = await SetUpReleaseFile(published: true);

            var response = await UpdateFilters(releaseFile, OptimisedTestUsers.Bau);

            response.AssertForbidden();
            Assert.Null((await GetReleaseFile(releaseFile.Id)).FilterSequence);
        }

        private async Task<HttpResponseMessage> UpdateFilters(ReleaseFile releaseFile, ClaimsPrincipal user)
        {
            var client = fixture.CreateClient(user: user);

            return await client.PatchAsync(
                $"api/data/release/{releaseFile.ReleaseVersionId}/meta/subject/{releaseFile.File.SubjectId}/filters",
                JsonContent.Create(new List<FilterUpdateViewModel>())
            );
        }
    }

    public class UpdateIndicatorsTests(TableBuilderMetaControllerTestsFixture fixture)
        : TableBuilderMetaControllerTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var releaseFile = await SetUpReleaseFile();
            SetUpCacheInvalidation(releaseFile);

            var response = await UpdateIndicators(releaseFile, OptimisedTestUsers.Bau);

            response.AssertOk();
            VerifyAllMocks(fixture.PrivateBlobCacheServiceMock);
            Assert.Empty((await GetReleaseFile(releaseFile.Id)).IndicatorSequence!);
        }

        [Fact]
        public async Task AuthenticatedUser_Forbidden()
        {
            var releaseFile = await SetUpReleaseFile();

            var response = await UpdateIndicators(releaseFile, OptimisedTestUsers.Authenticated);

            response.AssertForbidden();
            Assert.Null((await GetReleaseFile(releaseFile.Id)).IndicatorSequence);
        }

        [Fact]
        public async Task PublishedReleaseVersion_Forbidden()
        {
            var releaseFile = await SetUpReleaseFile(published: true);

            var response = await UpdateIndicators(releaseFile, OptimisedTestUsers.Bau);

            response.AssertForbidden();
            Assert.Null((await GetReleaseFile(releaseFile.Id)).IndicatorSequence);
        }

        private async Task<HttpResponseMessage> UpdateIndicators(ReleaseFile releaseFile, ClaimsPrincipal user)
        {
            var client = fixture.CreateClient(user: user);

            return await client.PatchAsync(
                $"api/data/release/{releaseFile.ReleaseVersionId}/meta/subject/{releaseFile.File.SubjectId}/indicators",
                JsonContent.Create(new List<IndicatorGroupUpdateViewModel>())
            );
        }
    }
}
