#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class PreReleaseSummaryServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task GetPreReleaseSummaryViewModel()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(
                _dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication())
            );

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(
                rv => rv.Id == releaseVersion.Id,
                CanViewSpecificPreReleaseSummary
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    contentDbContext.ReleaseVersions.Add(releaseVersion);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = BuildService(
                        contentDbContext: contentDbContext,
                        userService: userService.Object
                    );

                    return await service.GetPreReleaseSummaryViewModel(
                        releaseVersion.Id,
                        CancellationToken.None
                    );
                }
            });
    }

    private static PreReleaseSummaryService BuildService(
        ContentDbContext? contentDbContext = null,
        IUserService? userService = null
    )
    {
        return new PreReleaseSummaryService(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            userService ?? Mock.Of<IUserService>()
        );
    }
}
