#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetMappingServicePermissionTests
{
    // NOTE: GetOrCreateMapping is used by other services that do permission checks, so no need for one in it

    [Fact]
    public async Task UpdateIndicatorMapping()
    {
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

        contentDbContext.ReleaseVersions.Add(releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PermissionTestUtils
            .PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(releaseVersion, SecurityPolicies.CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupDataSetMappingService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.UpdateIndicatorMappings(
                    releaseVersion.Id,
                    new IndicatorMappingUpdatesRequest(),
                    CancellationToken.None
                );
            });
    }

    [Fact]
    public async Task UpdateLocationMappings()
    {
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

        contentDbContext.ReleaseVersions.Add(releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PermissionTestUtils
            .PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(releaseVersion, SecurityPolicies.CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupDataSetMappingService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.UpdateLocationMappings(
                    releaseVersion.Id,
                    new LocationMappingUpdatesRequest(),
                    CancellationToken.None
                );
            });
    }

    private static DataSetMappingService SetupDataSetMappingService(
        ContentDbContext contentDbContext,
        StatisticsDbContext? statisticsDbContext = null,
        IUserService? userService = null
    )
    {
        return new DataSetMappingService(
            contentDbContext,
            statisticsDbContext ?? Mock.Of<StatisticsDbContext>(MockBehavior.Strict),
            userService ?? Mock.Of<IUserService>(MockBehavior.Strict)
        );
    }
}
