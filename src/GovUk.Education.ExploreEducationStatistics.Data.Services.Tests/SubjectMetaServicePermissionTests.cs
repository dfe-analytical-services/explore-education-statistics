#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;
using ContentRelease = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class SubjectMetaServicePermissionTests
{
    private static readonly Guid ReleaseId = Guid.NewGuid();
    private static readonly Guid SubjectId = Guid.NewGuid();

    private static readonly ReleaseSubject ReleaseSubject = new()
    {
        ReleaseId = ReleaseId,
        SubjectId = SubjectId
    };

    [Fact]
    public async Task GetSubjectMeta()
    {
        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFail(ReleaseSubject, DataSecurityPolicies.CanViewSubjectData)
            .AssertForbidden(
                async userService =>
                {
                    var service = SetupSubjectMetaService(
                        userService: userService.Object
                    );

                    return await service.GetSubjectMeta(releaseId: ReleaseId, subjectId: SubjectId);
                }
            );
    }

    [Fact]
    public async Task GetSubjectMeta_WithReleaseSubject()
    {
        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFail(ReleaseSubject, DataSecurityPolicies.CanViewSubjectData)
            .AssertForbidden(
                async userService =>
                {
                    var service = SetupSubjectMetaService(
                        userService: userService.Object
                    );

                    return await service.GetSubjectMeta(ReleaseSubject);
                }
            );
    }

    [Fact]
    public async Task FilterSubjectMeta()
    {
        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFail(ReleaseSubject, DataSecurityPolicies.CanViewSubjectData)
            .AssertForbidden(
                async userService =>
                {
                    var query = new ObservationQueryContext
                    {
                        SubjectId = SubjectId
                    };

                    var cancellationToken = new CancellationTokenSource().Token;

                    var service = SetupSubjectMetaService(
                        userService: userService.Object
                    );

                    return await service.FilterSubjectMeta(ReleaseId, query, cancellationToken);
                }
            );
    }

    [Fact]
    public async Task FilterSubjectMeta_LatestRelease()
    {
        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseSubject>(rs => 
                rs.ReleaseId == ReleaseSubject.ReleaseId && rs.SubjectId == ReleaseSubject.SubjectId, 
                DataSecurityPolicies.CanViewSubjectData)
            .AssertForbidden(
                async userService =>
                {
                    var query = new ObservationQueryContext
                    {
                        SubjectId = SubjectId
                    };

                    var release = new ContentRelease
                    {
                        Id = ReleaseId,
                        Published = DateTime.UtcNow.AddDays(-1)
                    };

                    await using var contextDbContext = InMemoryContentDbContext();
                    await contextDbContext.Releases.AddAsync(release);
                    await contextDbContext.SaveChangesAsync();

                    await using var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();
                    await statisticsDbContext.ReleaseSubject.AddAsync(ReleaseSubject);
                    await statisticsDbContext.SaveChangesAsync();
                    
                    var service = SetupSubjectMetaService(
                        userService: userService.Object, 
                        statisticsDbContext: statisticsDbContext, 
                        contentDbContext: contextDbContext);

                    var cancellationToken = new CancellationTokenSource().Token;

                    return await service.FilterSubjectMeta(null, query, cancellationToken);
                }
            );
    }

    [Fact]
    public async Task GetReleaseSubjectForLatestPublishedVersion()
    {
        // Note that there are no explicit security policy checks enforced for this method.
        // This is because this method is very specifically returning only ReleaseSubjects for currently-live Releases
        // which are definitely publicly visible.
        await PolicyCheckBuilder<DataSecurityPolicies>()
            .AssertSuccess(async userService =>
            {
                var release = new ContentRelease
                {
                    Id = ReleaseId,
                    Published = DateTime.UtcNow.AddDays(-1)
                };

                await using var contextDbContext = InMemoryContentDbContext();
                await contextDbContext.Releases.AddAsync(release);
                await contextDbContext.SaveChangesAsync();

                await using var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();
                await statisticsDbContext.ReleaseSubject.AddAsync(ReleaseSubject);
                await statisticsDbContext.SaveChangesAsync();
                    
                var service = SetupSubjectMetaService(
                    userService: userService.Object, 
                    statisticsDbContext: statisticsDbContext, 
                    contentDbContext: contextDbContext);
                
                return await service.GetReleaseSubjectForLatestPublishedVersion(subjectId: SubjectId);
            });
    }

    private static SubjectMetaService SetupSubjectMetaService(
        StatisticsDbContext? statisticsDbContext = null,
        ContentDbContext? contentDbContext = null,
        IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
        IBlobCacheService? cacheService = null,
        IFilterRepository? filterRepository = null,
        IFilterItemRepository? filterItemRepository = null,
        IIndicatorGroupRepository? indicatorGroupRepository = null,
        ILocationRepository? locationRepository = null,
        IObservationService? observationService = null,
        ITimePeriodService? timePeriodService = null,
        IUserService? userService = null,
        IOptions<LocationsOptions>? options = null)
    {
        return new(
            statisticsPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
            statisticsDbContext ?? Mock.Of<StatisticsDbContext>(Strict),
            contentDbContext ?? InMemoryContentDbContext(),
            cacheService ?? Mock.Of<IBlobCacheService>(Strict), 
            filterRepository ?? Mock.Of<IFilterRepository>(Strict),
            filterItemRepository ?? Mock.Of<IFilterItemRepository>(Strict),
            indicatorGroupRepository ?? Mock.Of<IIndicatorGroupRepository>(Strict),
            locationRepository ?? Mock.Of<ILocationRepository>(Strict),
            Mock.Of<ILogger<SubjectMetaService>>(),
            observationService ?? Mock.Of<IObservationService>(Strict),
            timePeriodService ?? Mock.Of<ITimePeriodService>(Strict),
            userService ?? Mock.Of<IUserService>(),
            options ?? DefaultLocationOptions()
        );
    }

    private static IOptions<LocationsOptions> DefaultLocationOptions()
    {
        return Options.Create(new LocationsOptions());
    }

    private static Mock<IPersistenceHelper<StatisticsDbContext>> DefaultPersistenceHelperMock()
    {
        return MockUtils.MockPersistenceHelper<StatisticsDbContext, ReleaseSubject>(ReleaseSubject);
    }
}
