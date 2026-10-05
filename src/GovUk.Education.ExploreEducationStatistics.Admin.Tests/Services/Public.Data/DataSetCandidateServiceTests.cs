#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Public.Data;

public abstract class DataSetCandidateServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class ListCandidatesTests : DataSetCandidateServiceTests
    {
        [Fact]
        public async Task NotBauUser_IncompatibleReleaseFileNotReturned()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            var (releaseVersion, compatible, incompatible, unknown) = await AddReleaseFiles(contentDbContextId);

            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

            var service = BuildService(contentDbContext, isBauUser: false);

            var candidates = (await service.ListCandidates(releaseVersion.Id)).AssertRight();

            Assert.Equal(2, candidates.Count);
            Assert.Contains(candidates, c => c.ReleaseFileId == compatible.Id && c.PublicApiCompatible == true);
            Assert.Contains(candidates, c => c.ReleaseFileId == unknown.Id && c.PublicApiCompatible == null);
            Assert.DoesNotContain(candidates, c => c.ReleaseFileId == incompatible.Id);
        }

        [Fact]
        public async Task BauUser_AllReleaseFilesReturned()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            var (releaseVersion, compatible, incompatible, unknown) = await AddReleaseFiles(contentDbContextId);

            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

            var service = BuildService(contentDbContext, isBauUser: true);

            var candidates = (await service.ListCandidates(releaseVersion.Id)).AssertRight();

            Assert.Equal(3, candidates.Count);
            Assert.Contains(candidates, c => c.ReleaseFileId == compatible.Id && c.PublicApiCompatible == true);
            Assert.Contains(candidates, c => c.ReleaseFileId == incompatible.Id && c.PublicApiCompatible == false);
            Assert.Contains(candidates, c => c.ReleaseFileId == unknown.Id && c.PublicApiCompatible == null);
        }
    }

    private async Task<(ReleaseVersion, ReleaseFile, ReleaseFile, ReleaseFile)> AddReleaseFiles(
        string contentDbContextId
    )
    {
        Release release = _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true);
        var releaseVersion = release.Versions.Single();

        var dataImports = _dataFixture.DefaultDataImport().WithStatus(DataImportStatus.COMPLETE).GenerateList(3);

        ReleaseFile compatible = _dataFixture
            .DefaultReleaseFile()
            .WithFile(dataImports[0].File)
            .WithReleaseVersion(releaseVersion)
            .WithApiCompatibility(true);

        ReleaseFile incompatible = _dataFixture
            .DefaultReleaseFile()
            .WithFile(dataImports[1].File)
            .WithReleaseVersion(releaseVersion)
            .WithApiCompatibility(false);

        // Files uploaded before the screener reported API compatibility have no value set.
        ReleaseFile unknown = _dataFixture
            .DefaultReleaseFile()
            .WithFile(dataImports[2].File)
            .WithReleaseVersion(releaseVersion);

        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        contentDbContext.DataImports.AddRange(dataImports);
        contentDbContext.ReleaseFiles.AddRange(compatible, incompatible, unknown);
        await contentDbContext.SaveChangesAsync();

        return (releaseVersion, compatible, incompatible, unknown);
    }

    private static DataSetCandidateService BuildService(ContentDbContext contentDbContext, bool isBauUser)
    {
        var userService = new Mock<IUserService>(MockBehavior.Strict);

        userService.Setup(s => s.MatchesPolicy(SecurityPolicies.CanManagePublicApiDataSets)).ReturnsAsync(true);
        userService.Setup(s => s.MatchesPolicy(SecurityPolicies.IsBauUser)).ReturnsAsync(isBauUser);

        return new DataSetCandidateService(contentDbContext, userService.Object);
    }
}
