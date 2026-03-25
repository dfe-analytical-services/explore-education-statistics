#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleasePublishingStatusServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task GetReleaseStatus()
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

                    return await sut.GetReleaseStatus(releaseVersion.Id);
                }
            });
    }

    private static ReleasePublishingStatusService BuildService(
        ContentDbContext contentDbContext,
        IUserService userService,
        IPublisherTableStorageService? publisherTableStorageService = null
    ) =>
        new(
            contentDbContext,
            publisherTableStorageService ?? new Mock<IPublisherTableStorageService>().Object,
            userService
        );
}
