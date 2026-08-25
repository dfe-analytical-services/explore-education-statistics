#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.WebUtilities;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

// ReSharper disable once ClassNeverInstantiated.Global
public class DataSetCandidatesControllerTestsFixture()
    : OptimisedAdminCollectionFixture(
        capabilities: [AdminIntegrationTestCapability.UserAuth, AdminIntegrationTestCapability.Postgres]
    );

[CollectionDefinition(nameof(DataSetCandidatesControllerTestsFixture))]
public class DataSetCandidatesControllerTestsCollection : ICollectionFixture<DataSetCandidatesControllerTestsFixture>;

[Collection(nameof(DataSetCandidatesControllerTestsFixture))]
public abstract class DataSetCandidatesControllerTests(DataSetCandidatesControllerTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private static readonly DataFixture DataFixture = new();
    private const string BaseUrl = "api/public-data/data-set-candidates";

    public class ListDataSetCandidatesTests(DataSetCandidatesControllerTestsFixture fixture)
        : DataSetCandidatesControllerTests(fixture)
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

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithRelease(DataFixture.DefaultRelease());

            await fixture.GetContentDbContext().AddTestData(context => context.ReleaseVersions.Add(releaseVersion));

            var response = await GetDataSetCandidates(releaseVersion.Id, user: DataFixture.AuthenticatedUser());

            response.AssertForbidden();
        }

        [Theory]
        [InlineData(PublicationRole.Approver)]
        [InlineData(PublicationRole.Drafter)]
        public async Task UserOnPublicationTeam_Returns403(PublicationRole publicationRole)
        {
            // CanManagePublicApiDataSets is currently BAU-only - having a role on the release version's
            // publication does not (yet) satisfy this policy. This test should start failing once
            // publication-role support is added to ManagePublicApiDataSetsAuthorizationHandler, at which
            // point it should be updated to reflect the new expected behaviour.
            Release release = DataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true)
                .WithPublication(DataFixture.DefaultPublication());
            var releaseVersion = release.Versions.Single();

            ClaimsPrincipal identityUser = DataFixture.StandardUser();
            User user = DataFixture.DefaultUser().WithId(identityUser.GetUserId());
            UserPublicationRole userPublicationRole = DataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(release.Publication)
                .WithRole(publicationRole);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseVersions.Add(releaseVersion);
                    context.UserPublicationRoles.Add(userPublicationRole);
                });

            var response = await GetDataSetCandidates(releaseVersion.Id, user: identityUser);

            response.AssertForbidden();
        }

        [Fact]
        public async Task NoReleaseFileExists_ReturnsEmptyList()
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithRelease(DataFixture.DefaultRelease());

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
            var client = fixture.CreateClient(user: user ?? DataFixture.BauUser());

            var query = new Dictionary<string, string?> { { "releaseVersionId", releaseVersionId.ToString() } };

            var uri = QueryHelpers.AddQueryString(BaseUrl, query);

            return await client.GetAsync(uri);
        }
    }
}
