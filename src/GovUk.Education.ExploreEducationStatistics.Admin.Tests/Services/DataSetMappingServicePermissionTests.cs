#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetMappingServicePermissionTests
{
    private static readonly DataFixture DataFixture = new();

    [Fact]
    public async Task UpdateFilterMappings()
    {
        ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion().WithRelease(DataFixture.DefaultRelease());

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
        ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion().WithRelease(DataFixture.DefaultRelease());

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
        ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion().WithRelease(DataFixture.DefaultRelease());

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

    // The UpdateSpecificReleaseVersion authorization handler reads ReleaseVersion.Release.PublicationId for users
    // without the UpdateAllReleases claim (e.g. analysts), so the Release must be loaded before the policy check.
    [Fact]
    public async Task UpdateFilterMappings_ReleaseLoadedForPolicyCheck()
    {
        await AssertReleaseLoadedForPolicyCheck(
            (service, releaseVersionId) =>
                service.UpdateFilterMappings(
                    releaseVersionId,
                    new FilterMappingUpdatesRequest(),
                    CancellationToken.None
                )
        );
    }

    [Fact]
    public async Task UpdateIndicatorMappings_ReleaseLoadedForPolicyCheck()
    {
        await AssertReleaseLoadedForPolicyCheck(
            (service, releaseVersionId) =>
                service.UpdateIndicatorMappings(
                    releaseVersionId,
                    new IndicatorMappingUpdatesRequest(),
                    CancellationToken.None
                )
        );
    }

    [Fact]
    public async Task UpdateLocationMappings_ReleaseLoadedForPolicyCheck()
    {
        await AssertReleaseLoadedForPolicyCheck(
            (service, releaseVersionId) =>
                service.UpdateLocationMappings(
                    releaseVersionId,
                    new LocationMappingUpdatesRequest(),
                    CancellationToken.None
                )
        );
    }

    private static async Task AssertReleaseLoadedForPolicyCheck<TResult>(
        Func<DataSetMappingService, Guid, Task<Either<ActionResult, TResult>>> updateMappings
    )
    {
        ReleaseVersion releaseVersion = DataFixture
            .DefaultReleaseVersion()
            .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()));

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userService = new Mock<IUserService>(MockBehavior.Strict);
        userService
            .Setup(s =>
                s.MatchesPolicy(
                    It.Is<ReleaseVersion>(rv =>
                        rv.Id == releaseVersion.Id
                        && rv.Release != null
                        && rv.Release.PublicationId == releaseVersion.Release.PublicationId
                    ),
                    SecurityPolicies.CanUpdateSpecificReleaseVersion
                )
            )
            .ReturnsAsync(true);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext())
        {
            var service = SetupDataSetMappingService(contentDbContext, statisticsDbContext, userService.Object);

            // No DataSetMapping exists, so the request fails after passing the policy check.
            var result = await updateMappings(service, releaseVersion.Id);

            result.AssertNotFound();
        }

        MockUtils.VerifyAllMocks(userService);
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
