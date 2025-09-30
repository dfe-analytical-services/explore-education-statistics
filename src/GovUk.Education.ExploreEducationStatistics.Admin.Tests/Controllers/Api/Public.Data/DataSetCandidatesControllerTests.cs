#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.WebUtilities;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public abstract class DataSetCandidatesControllerTests(TestApplicationFactory testApp)
    : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/public-data/data-set-candidates";

    public class ListDataSetCandidatesTests(TestApplicationFactory testApp)
        : DataSetsControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            Release release = DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true);

            var dataImports = DataFixture
                .DefaultDataImport()
                .WithStatus(DataImportStatus.COMPLETE)
                .GenerateList(3);

            var releaseVersion = release.Versions.Single();

            var releaseFiles = dataImports
                .Select(di =>
                    DataFixture
                        .DefaultReleaseFile()
                        .WithFile(di.File)
                        .WithReleaseVersion(releaseVersion)
                        .Generate()
                )
                .ToList();

            await TestApp.AddTestData<ContentDbContext>(context =>
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
                        candidate.ReleaseFileId == releaseFile.Id
                        && candidate.Title == releaseFile.Name
                    )
            );
        }

        [Theory]
        [InlineData(SecurityClaimTypes.UpdateAllReleases)]
        [InlineData(SecurityClaimTypes.CreateAnyRelease)]
        [InlineData(null)]
        public async Task NoPermissionsToViewReleaseVersion_Returns403(
            SecurityClaimTypes? claimType
        )
        {
            ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion();

            await TestApp.AddTestData<ContentDbContext>(context =>
                context.ReleaseVersions.Add(releaseVersion)
            );

            var authenticatedUser = claimType.HasValue
                ? DataFixture.AuthenticatedUser().WithClaim(claimType.Value.ToString())
                : DataFixture.AuthenticatedUser();

            var client = TestApp.SetUser(authenticatedUser).CreateClient();

            var response = await GetDataSetCandidates(releaseVersion.Id, client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task NoReleaseFileExists_ReturnsEmptyList()
        {
            ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion();

            await TestApp.AddTestData<ContentDbContext>(context =>
                context.ReleaseVersions.Add(releaseVersion)
            );

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

            await TestApp.AddTestData<ContentDbContext>(context =>
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

            await TestApp.AddTestData<ContentDbContext>(context =>
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

            DataImport dataImport = DataFixture
                .DefaultDataImport()
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            var releaseVersion = release.Versions.Single();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(dataImport.File)
                .WithReleaseVersion(releaseVersion)
                .WithPublicApiDataSetId(Guid.NewGuid());

            await TestApp.AddTestData<ContentDbContext>(context =>
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

            await TestApp.AddTestData<ContentDbContext>(context =>
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
            HttpClient? client = null
        )
        {
            var user = DataFixture
                .AuthenticatedUser()
                .WithClaim(SecurityClaimTypes.AccessAllReleases.ToString());

            client ??= TestApp.SetUser(user).CreateClient();

            var query = new Dictionary<string, string?>
            {
                { "releaseVersionId", releaseVersionId.ToString() },
            };

            var uri = QueryHelpers.AddQueryString(BaseUrl, query);

            return await client.GetAsync(uri);
        }
    }
}
