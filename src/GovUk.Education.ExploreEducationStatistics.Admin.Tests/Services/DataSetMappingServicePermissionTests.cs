#nullable enable
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Utils.StorageDataSetTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetMappingServicePermissionTests
{
    private static readonly DataFixture DataFixture = new();

    [Fact]
    public async Task UpdateFilterMappings()
    {
        var releaseVersion = CreateReleaseVersion();

        await PermissionTestUtils
            .PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher(
                ReleaseVersionWithReleaseLoaded(releaseVersion),
                SecurityPolicies.CanUpdateSpecificReleaseVersion
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = await SaveReleaseVersion(releaseVersion);

                await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
                await using var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

                var service = SetupDataSetMappingService(contentDbContext, statisticsDbContext, userService.Object);

                return await service.UpdateFilterMappings(
                    releaseVersion.Id,
                    new FilterMappingUpdatesRequest(),
                    CancellationToken.None
                );
            });
    }

    [Fact]
    public async Task UpdateIndicatorMappings()
    {
        var releaseVersion = CreateReleaseVersion();

        await PermissionTestUtils
            .PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher(
                ReleaseVersionWithReleaseLoaded(releaseVersion),
                SecurityPolicies.CanUpdateSpecificReleaseVersion
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = await SaveReleaseVersion(releaseVersion);

                await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
                await using var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

                var service = SetupDataSetMappingService(contentDbContext, statisticsDbContext, userService.Object);

                return await service.UpdateIndicatorMappings(
                    releaseVersion.Id,
                    new IndicatorMappingUpdatesRequest(),
                    CancellationToken.None
                );
            });
    }

    [Fact]
    public async Task UpdateLocationMappings()
    {
        var releaseVersion = CreateReleaseVersion();

        await PermissionTestUtils
            .PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher(
                ReleaseVersionWithReleaseLoaded(releaseVersion),
                SecurityPolicies.CanUpdateSpecificReleaseVersion
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = await SaveReleaseVersion(releaseVersion);

                await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
                await using var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

                var service = SetupDataSetMappingService(contentDbContext, statisticsDbContext, userService.Object);

                return await service.UpdateLocationMappings(
                    releaseVersion.Id,
                    new LocationMappingUpdatesRequest(),
                    CancellationToken.None
                );
            });
    }

    // The UpdateSpecificReleaseVersion authorization handler reads ReleaseVersion.Release.PublicationId for users
    // without the UpdateAllReleases claim (e.g. analysts), so the Release must be loaded before the policy check.
    private static Expression<Func<ReleaseVersion, bool>> ReleaseVersionWithReleaseLoaded(
        ReleaseVersion releaseVersion
    ) =>
        rv =>
            rv.Id == releaseVersion.Id
            && rv.Release != null
            && rv.Release.PublicationId == releaseVersion.Release.PublicationId;

    private static ReleaseVersion CreateReleaseVersion() =>
        DataFixture
            .DefaultReleaseVersion()
            .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()));

    /// <summary>
    /// Saves the release version in its own context, so the service reads it back from a fresh context and
    /// EF can't populate Release from entities it's already tracking.
    /// </summary>
    private static async Task<string> SaveReleaseVersion(ReleaseVersion releaseVersion)
    {
        var contentDbContextId = Guid.NewGuid().ToString();

        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        contentDbContext.ReleaseVersions.Add(releaseVersion);
        await contentDbContext.SaveChangesAsync();

        return contentDbContextId;
    }

    private static DataSetMappingService SetupDataSetMappingService(
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext,
        IUserService? userService = null
    )
    {
        return new DataSetMappingService(
            contentDbContext,
            BuildStorageDataSetResolver(contentDbContext, statisticsDbContext),
            userService ?? Mock.Of<IUserService>(MockBehavior.Strict)
        );
    }
}
