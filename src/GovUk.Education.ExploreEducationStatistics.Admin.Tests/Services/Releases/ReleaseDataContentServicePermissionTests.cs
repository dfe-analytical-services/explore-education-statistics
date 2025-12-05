using GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Releases;

public class ReleaseDataContentServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task GetReleaseDataContent()
    {
        // Arrange
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion();

        // Act & Assert
        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(
                rv => rv.Id == releaseVersion.Id,
                ContentSecurityPolicies.CanViewSpecificReleaseVersion
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.ReleaseVersions.Add(releaseVersion);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var sut = BuildService(contentDbContext, userService.Object);

                    return await sut.GetReleaseDataContent(
                        releaseVersionId: releaseVersion.Id,
                        cancellationToken: CancellationToken.None
                    );
                }
            });
    }

    private static ReleaseDataContentService BuildService(
        ContentDbContext contentDbContext,
        IUserService userService
    ) => new(contentDbContext: contentDbContext, userService: userService);
}
