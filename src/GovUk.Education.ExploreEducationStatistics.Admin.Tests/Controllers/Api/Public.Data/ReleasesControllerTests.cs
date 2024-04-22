#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public class ReleasesControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/public-data/releases";

    public class GetDataSetCandidatesTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
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
            Assert.All(content, apiDataSetCandidate =>
                releaseFiles.Any(rf => rf.FileId == apiDataSetCandidate.FileId && rf.Name == apiDataSetCandidate.Title));
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

            var client = TestApp
                .SetUser(UserWithClaim(claimType))
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
                .SetUser(UserWithClaim(SecurityClaimTypes.AccessAllReleases))
                .CreateClient();

            var uri = new Uri($"{BaseUrl}/{releaseVersionId}/data-set-candidates", UriKind.Relative);

            return await client.GetAsync(uri);
        }

        private static ClaimsPrincipal UserWithClaim(SecurityClaimTypes? claimType = null)
        {
            var claimsPrincipal = AuthenticatedUser();
            var claimsIdentity = (claimsPrincipal.Identity as ClaimsIdentity)!;

            if (claimType.HasValue)
            {
                claimsIdentity.AddClaim(SecurityClaim(claimType.Value));
            }

            return claimsPrincipal;
        }
    }
}
