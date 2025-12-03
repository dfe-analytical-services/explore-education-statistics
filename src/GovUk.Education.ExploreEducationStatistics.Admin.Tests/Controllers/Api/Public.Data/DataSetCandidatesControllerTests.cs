#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.UserAuth;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.WebUtilities;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

// ReSharper disable once ClassNeverInstantiated.Global
public class DataSetCandidatesControllerTestsFixture()
    : OptimisedAdminCollectionFixture(
        capabilities: [AdminIntegrationTestCapability.UserAuth, AdminIntegrationTestCapability.Postgres]
    );

[CollectionDefinition(nameof(DataSetCandidatesControllerTestsFixture))]
public class DataSetCandidatesControllerTestsCollection : ICollectionFixture<DataSetCandidatesControllerTestsFixture>;

[Collection(nameof(DataSetCandidatesControllerTestsFixture))]
public abstract class DataSetCandidatesControllerTests
{
    private static readonly DataFixture DataFixture = new();
    private const string BaseUrl = "api/public-data/data-set-candidates";

    public class ListDataSetCandidatesTests(DataSetCandidatesControllerTestsFixture fixture)
        : DataSetCandidatesControllerTests
    {
        [Fact]
        public async Task Success()
        {
            Release release = DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true);

            var dataImports = DataFixture.DefaultDataImport().WithStatus(DataImportStatus.COMPLETE).GenerateList(3);

            var releaseVersion = release.Versions.Single();

            var releaseFiles = dataImports
                .Select(di =>
                    DataFixture
                        .DefaultReleaseFile()
                        .WithFile(di.File)
                        .WithReleaseVersion(releaseVersion)
                        .WithApiCompatibility(true)
                        .Generate()
                )
                .ToList();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.DataImports.AddRange(dataImports);
                    context.ReleaseFiles.AddRange(releaseFiles);
                });

            var response = await GetDataSetCandidates(releaseVersion.Id);

            var candidates = response.AssertOk<List<DataSetCandidateViewModel>>();

            Assert.NotNull(candidates);
            Assert.Equal(3, candidates.Count);
            Assert.Contains(
                releaseFiles,
                releaseFile =>
                    candidates.Any(candidate =>
                        candidate.ReleaseFileId == releaseFile.Id && candidate.Title == releaseFile.Name
                    )
            );
        }

        [Theory]
        [InlineData(SecurityClaimTypes.UpdateAllReleases)]
        [InlineData(SecurityClaimTypes.CreateAnyRelease)]
        [InlineData(null)]
        public async Task NoPermissionsToViewReleaseVersion_Returns403(SecurityClaimTypes? claimType)
        {
            ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion();

            await fixture.GetContentDbContext().AddTestData(context => context.ReleaseVersions.Add(releaseVersion));

            var authenticatedUser = claimType.HasValue
                ? DataFixture.AuthenticatedUser().WithClaim(claimType.Value.ToString())
                : DataFixture.AuthenticatedUser();

            var response = await GetDataSetCandidates(releaseVersion.Id, user: authenticatedUser);

            response.AssertForbidden();
        }

        [Fact]
        public async Task NoReleaseFileExists_ReturnsEmptyList()
        {
            ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion();

            await fixture.GetContentDbContext().AddTestData(context => context.ReleaseVersions.Add(releaseVersion));

            var response = await GetDataSetCandidates(releaseVersion.Id);

            var candidates = response.AssertOk<List<DataSetCandidateViewModel>>();

            Assert.Empty(candidates);
        }

        [Fact]
        public async Task ReleaseFileIsReplacement_NotReturned()
        {
            Release release = DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true);

            DataImport dataImport = DataFixture
                .DefaultDataImport()
                .WithFile(DataFixture.DefaultFile(FileType.Data).WithReplacingId(Guid.NewGuid()));

            var releaseVersion = release.Versions.Single();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(dataImport.File)
                .WithReleaseVersion(releaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.DataImports.Add(dataImport);
                    context.ReleaseFiles.Add(releaseFile);
                });

            var response = await GetDataSetCandidates(releaseVersion.Id);

            var candidates = response.AssertOk<List<DataSetCandidateViewModel>>();

            Assert.Empty(candidates);
        }

        [Fact]
        public async Task ReleaseFileIsReplaced_NotReturned()
        {
            Release release = DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true);

            DataImport dataImport = DataFixture
                .DefaultDataImport()
                .WithFile(DataFixture.DefaultFile(FileType.Data).WithReplacedById(Guid.NewGuid()));

            var releaseVersion = release.Versions.Single();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(dataImport.File)
                .WithReleaseVersion(releaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.DataImports.Add(dataImport);
                    context.ReleaseFiles.Add(releaseFile);
                });

            var response = await GetDataSetCandidates(releaseVersion.Id);

            var candidates = response.AssertOk<List<DataSetCandidateViewModel>>();

            Assert.Empty(candidates);
        }

        [Fact]
        public async Task ReleaseFileHasAssociatedDataSet_NotReturned()
        {
            Release release = DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true);

            DataImport dataImport = DataFixture.DefaultDataImport().WithFile(DataFixture.DefaultFile(FileType.Data));

            var releaseVersion = release.Versions.Single();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(dataImport.File)
                .WithReleaseVersion(releaseVersion)
                .WithPublicApiDataSetId(Guid.NewGuid());

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.DataImports.Add(dataImport);
                    context.ReleaseFiles.Add(releaseFile);
                });

            var response = await GetDataSetCandidates(releaseVersion.Id);

            var candidates = response.AssertOk<List<DataSetCandidateViewModel>>();

            Assert.Empty(candidates);
        }

        [Fact]
        public async Task ReleaseFileIsIncompatible_NotReturned()
        {
            Release release = DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true);

            DataImport dataImport = DataFixture
                .DefaultDataImport()
                .WithFile(DataFixture.DefaultFile(FileType.Data).WithReplacingId(Guid.NewGuid()));

            var releaseVersion = release.Versions.Single();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(dataImport.File)
                .WithReleaseVersion(releaseVersion)
                .WithApiCompatibility(false);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.DataImports.Add(dataImport);
                    context.ReleaseFiles.Add(releaseFile);
                });

            var response = await GetDataSetCandidates(releaseVersion.Id);

            var candidates = response.AssertOk<List<DataSetCandidateViewModel>>();

            Assert.Empty(candidates);
        }

        [Theory]
        [InlineData(DataImportStatus.QUEUED)]
        [InlineData(DataImportStatus.STAGE_1)]
        [InlineData(DataImportStatus.STAGE_2)]
        [InlineData(DataImportStatus.STAGE_3)]
        [InlineData(DataImportStatus.FAILED)]
        [InlineData(DataImportStatus.NOT_FOUND)]
        [InlineData(DataImportStatus.CANCELLED)]
        [InlineData(DataImportStatus.CANCELLING)]
        public async Task ReleaseFileImportIsNotComplete_NotReturned(DataImportStatus status)
        {
            Release release = DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true);

            DataImport dataImport = DataFixture
                .DefaultDataImport()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithStatus(status);

            var releaseVersion = release.Versions.Single();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(dataImport.File)
                .WithReleaseVersion(releaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.DataImports.Add(dataImport);
                    context.ReleaseFiles.Add(releaseFile);
                });

            var response = await GetDataSetCandidates(releaseVersion.Id);

            var candidates = response.AssertOk<List<DataSetCandidateViewModel>>();

            Assert.Empty(candidates);
        }

        [Fact]
        public async Task ReleaseVersionDoesNotExist_Returns404()
        {
            var response = await GetDataSetCandidates(releaseVersionId: Guid.NewGuid());

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetDataSetCandidates(
            Guid releaseVersionId,
            ClaimsPrincipal? user = null
        )
        {
            var defaultUser = DataFixture
                .AuthenticatedUser()
                .WithClaim(SecurityClaimTypes.AccessAllReleases.ToString());

            fixture.RegisterTestUser(user ?? defaultUser);

            var client = fixture.CreateClient().WithUser(user ?? defaultUser);

            var query = new Dictionary<string, string?> { { "releaseVersionId", releaseVersionId.ToString() } };

            var uri = QueryHelpers.AddQueryString(BaseUrl, query);

            return await client.GetAsync(uri);
        }
    }
}
