#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;

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
            .SetupResourceCheckToFail(ReleaseSubject, DataSecurityPolicies.CanViewSubjectData)
            .AssertForbidden(
                async userService =>
                {
                    var query = new ObservationQueryContext
                    {
                        SubjectId = SubjectId
                    };

                    var cancellationToken = new CancellationTokenSource().Token;

                    var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);

                    releaseSubjectRepository.Setup(mock => mock.GetReleaseSubjectForLatestPublishedVersion(SubjectId))
                        .ReturnsAsync(ReleaseSubject);

                    var service = SetupSubjectMetaService(
                        releaseSubjectRepository: releaseSubjectRepository.Object,
                        userService: userService.Object
                    );

                    return await service.FilterSubjectMeta(null, query, cancellationToken);
                }
            );
    }

    private static SubjectMetaService SetupSubjectMetaService(
        StatisticsDbContext? statisticsDbContext = null,
        IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
        IFilterRepository? filterRepository = null,
        IFilterItemRepository? filterItemRepository = null,
        IIndicatorGroupRepository? indicatorGroupRepository = null,
        ILocationRepository? locationRepository = null,
        IObservationService? observationService = null,
        IReleaseSubjectRepository? releaseSubjectRepository = null,
        ITimePeriodService? timePeriodService = null,
        IUserService? userService = null,
        IOptions<LocationsOptions>? options = null)
    {
        return new(
            statisticsPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
            statisticsDbContext ?? Mock.Of<StatisticsDbContext>(Strict),
            filterRepository ?? Mock.Of<IFilterRepository>(Strict),
            filterItemRepository ?? Mock.Of<IFilterItemRepository>(Strict),
            indicatorGroupRepository ?? Mock.Of<IIndicatorGroupRepository>(Strict),
            locationRepository ?? Mock.Of<ILocationRepository>(Strict),
            Mock.Of<ILogger<SubjectMetaService>>(),
            observationService ?? Mock.Of<IObservationService>(Strict),
            releaseSubjectRepository ?? Mock.Of<IReleaseSubjectRepository>(Strict),
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
