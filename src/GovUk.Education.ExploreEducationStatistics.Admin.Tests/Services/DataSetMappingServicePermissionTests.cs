#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetMappingServicePermissionTests
{
    [Fact]
    public async Task UpdateFilterMappings()
    {
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        await using var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

        contentDbContext.ReleaseVersions.Add(releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PermissionTestUtils
            .PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(releaseVersion, SecurityPolicies.CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupDataSetMappingService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    userService: userService.Object
                );
                return service.UpdateFilterMappings(
                    releaseVersion.Id,
                    new FilterMappingUpdatesRequest(),
                    CancellationToken.None
                );
            });
    }

    [Fact]
    public async Task UpdateIndicatorMapping()
    {
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        await using var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

        contentDbContext.ReleaseVersions.Add(releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PermissionTestUtils
            .PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(releaseVersion, SecurityPolicies.CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupDataSetMappingService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
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
        await using var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

        contentDbContext.ReleaseVersions.Add(releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PermissionTestUtils
            .PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(releaseVersion, SecurityPolicies.CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupDataSetMappingService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
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
        StatisticsDbContext statisticsDbContext,
        IUserService? userService = null
    )
    {
        return new DataSetMappingService(
            contentDbContext,
            statisticsDbContext,
            new LocationRepository(statisticsDbContext),
            userService ?? Mock.Of<IUserService>(MockBehavior.Strict)
        );
    }
}
