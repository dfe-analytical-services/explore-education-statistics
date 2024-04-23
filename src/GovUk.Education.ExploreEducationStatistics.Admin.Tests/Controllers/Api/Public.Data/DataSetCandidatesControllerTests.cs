#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.WebUtilities;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public class DataSetCandidatesControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/public-data/data-set-candidates";

    public class ListApiDataSetCandidatesTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            Release release = DataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true);

            var files = DataFixture
                .DefaultFile()
                .GenerateList(3);

            var releaseVersion = release.Versions.Single();

            var releaseFiles = DataFixture
                .DefaultReleaseFile()
                .ForIndex(0, rf => rf.SetFile(files[0]))
                .ForIndex(1, rf => rf.SetFile(files[1]))
                .ForIndex(2, rf => rf.SetFile(files[2]))
                .WithReleaseVersion(releaseVersion)
                .GenerateList(3);

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseFiles.AddRange(releaseFiles));

            var response = await GetDataSetCandidates(releaseVersion.Id);

            var content = response.AssertOk<List<ApiDataSetCandidateViewModel>>();

            Assert.NotNull(content);
            Assert.NotEmpty(content);
            Assert.Equal(3, content.Count);
            Assert.All(releaseFiles, releaseFile =>
                content.Any(apiDataSetCandidate => apiDataSetCandidate.FileId == releaseFile.FileId && apiDataSetCandidate.Title == releaseFile.Name));
        }

        [Theory]
        [InlineData(SecurityClaimTypes.UpdateAllReleases)]
        [InlineData(SecurityClaimTypes.CreateAnyRelease)]
        [InlineData(null)]
        public async Task NoPermissionsToViewReleaseVersion_Returns403(SecurityClaimTypes? claimType)
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseVersions.Add(releaseVersion));

            var authenticatedUser =
                claimType.HasValue
                ? AuthenticatedUser(SecurityClaim(claimType.Value))
                : AuthenticatedUser();

            var client = TestApp
                .SetUser(authenticatedUser)
                .CreateClient();

            var response = await GetDataSetCandidates(releaseVersion.Id, client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task NoReleaseFileExists_ReturnsEmptyList()
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseVersions.Add(releaseVersion));

            var response = await GetDataSetCandidates(releaseVersion.Id);

            var content = response.AssertOk<List<ApiDataSetCandidateViewModel>>();

            Assert.NotNull(content);
            Assert.Empty(content);
        }


        [Fact]
        public async Task ReleaseFileIsReplacement_NotReturned()
        {
            Release release = DataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true);

            File file = DataFixture
                .DefaultFile()
                .WithReplacingId(Guid.NewGuid());

            var releaseVersion = release.Versions.Single();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(file)
                .WithReleaseVersion(releaseVersion);

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            var response = await GetDataSetCandidates(releaseVersion.Id);

            var content = response.AssertOk<List<ApiDataSetCandidateViewModel>>();

            Assert.NotNull(content);
            Assert.Empty(content);
        }

        [Fact]
        public async Task ReleaseFileIsReplaced_NotReturned()
        {
            Release release = DataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true);

            File file = DataFixture
                .DefaultFile()
                .WithReplacedById(Guid.NewGuid());

            var releaseVersion = release.Versions.Single();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(file)
                .WithReleaseVersion(releaseVersion);

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            var response = await GetDataSetCandidates(releaseVersion.Id);

            var content = response.AssertOk<List<ApiDataSetCandidateViewModel>>();

            Assert.NotNull(content);
            Assert.Empty(content);
        }

        [Fact]
        public async Task ReleaseFileHasAssociatedApiDataSet_NotReturned()
        {
            Release release = DataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true);

            File file = DataFixture
                .DefaultFile()
                .WithPublicDataSetVersionId(Guid.NewGuid());

            var releaseVersion = release.Versions.Single();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(file)
                .WithReleaseVersion(releaseVersion);

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            var response = await GetDataSetCandidates(releaseVersion.Id);

            var content = response.AssertOk<List<ApiDataSetCandidateViewModel>>();

            Assert.NotNull(content);
            Assert.Empty(content);
        }

        [Fact]
        public async Task ReleaseVersionDoesNotExist_Returns404()
        {
            var response = await GetDataSetCandidates(
                releaseVersionId: Guid.NewGuid());

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetDataSetCandidates(
            Guid releaseVersionId,
            HttpClient? client = null)
        {
            client ??= TestApp
                .SetUser(AuthenticatedUser(SecurityClaim(SecurityClaimTypes.AccessAllReleases)))
                .CreateClient();

            var query = new Dictionary<string, string?>
            {
                { "releaseVersionId", releaseVersionId.ToString() },
            };

            var uri = QueryHelpers.AddQueryString(BaseUrl, query);

            return await client.GetAsync(uri);
        }
    }
}
