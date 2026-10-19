#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Options;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class SubjectMetaServicePermissionTests
{
    private static readonly Guid ReleaseVersionId = Guid.NewGuid();
    private static readonly Guid SubjectId = Guid.NewGuid();

    private static readonly ReleaseFile ReleaseFile = new()
    {
        ReleaseVersionId = ReleaseVersionId,
        File = new File { SubjectId = SubjectId },
    };

    [Fact]
    public async Task GetSubjectMeta()
    {
        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFail(ReleaseFile, DataSecurityPolicies.CanViewSubjectData)
            .AssertForbidden(async userService =>
            {
                var releaseSubjectService = new Mock<IReleaseSubjectService>(Strict);
                releaseSubjectService.Setup(s => s.Find(SubjectId, ReleaseVersionId)).ReturnsAsync(ReleaseFile);

                var service = SetupService(
                    userService: userService.Object,
                    releaseSubjectService: releaseSubjectService.Object
                );

                return await service.GetSubjectMeta(releaseVersionId: ReleaseVersionId, subjectId: SubjectId);
            });
    }

    [Fact]
    public async Task GetSubjectMeta_WithReleaseFile()
    {
        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFail(ReleaseFile, DataSecurityPolicies.CanViewSubjectData)
            .AssertForbidden(async userService =>
            {
                var releaseSubjectService = new Mock<IReleaseSubjectService>(Strict);

                releaseSubjectService.Setup(s => s.Find(SubjectId, ReleaseVersionId)).ReturnsAsync(ReleaseFile);

                var service = SetupService(
                    userService: userService.Object,
                    releaseSubjectService: releaseSubjectService.Object
                );

                return await service.GetSubjectMeta(ReleaseFile);
            });
    }

    [Fact]
    public async Task FilterSubjectMeta()
    {
        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFail(ReleaseFile, DataSecurityPolicies.CanViewSubjectData)
            .AssertForbidden(async userService =>
            {
                var releaseSubjectService = new Mock<IReleaseSubjectService>(Strict);
                releaseSubjectService.Setup(s => s.Find(SubjectId, ReleaseVersionId)).ReturnsAsync(ReleaseFile);

                var service = SetupService(
                    userService: userService.Object,
                    releaseSubjectService: releaseSubjectService.Object
                );

                var request = new LocationsOrTimePeriodsQueryRequest { SubjectId = SubjectId };

                return await service.FilterSubjectMeta(ReleaseVersionId, request, new CancellationTokenSource().Token);
            });
    }

    [Fact]
    public async Task FilterSubjectMeta_LatestRelease()
    {
        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseFile>(
                rf => rf.ReleaseVersionId == ReleaseVersionId && rf.File.SubjectId == SubjectId,
                DataSecurityPolicies.CanViewSubjectData
            )
            .AssertForbidden(async userService =>
            {
                var releaseSubjectService = new Mock<IReleaseSubjectService>(Strict);

                releaseSubjectService.Setup(s => s.Find(SubjectId, null)).ReturnsAsync(ReleaseFile);

                var service = SetupService(
                    userService: userService.Object,
                    releaseSubjectService: releaseSubjectService.Object
                );

                var request = new LocationsOrTimePeriodsQueryRequest { SubjectId = SubjectId };

                return await service.FilterSubjectMeta(null, request, new CancellationTokenSource().Token);
            });
    }

    private static SubjectMetaService SetupService(
        StatisticsDbContext? statisticsDbContext = null,
        ContentDbContext? contentDbContext = null,
        IBlobCacheService? cacheService = null,
        IReleaseSubjectService? releaseSubjectService = null,
        IFilterRepository? filterRepository = null,
        IFilterItemRepository? filterItemRepository = null,
        IIndicatorGroupRepository? indicatorGroupRepository = null,
        ILocationRepository? locationRepository = null,
        IObservationService? observationService = null,
        ITimePeriodService? timePeriodService = null,
        IUserService? userService = null,
        IOptions<LocationsOptions>? options = null
    )
    {
        return new(
            statisticsDbContext ?? InMemoryStatisticsDbContext(),
            contentDbContext ?? InMemoryContentDbContext(),
            cacheService ?? Mock.Of<IBlobCacheService>(Strict),
            releaseSubjectService ?? Mock.Of<IReleaseSubjectService>(Strict),
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
        return new LocationsOptions().ToOptionsWrapper();
    }
}
