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
    private readonly ReleaseSubject _releaseSubject = new()
    {
        ReleaseId = Guid.NewGuid(),
        SubjectId = Guid.NewGuid()
    };

    [Fact]
    public async Task GetSubjectMeta()
    {
        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFail(_releaseSubject, DataSecurityPolicies.CanViewSubjectData)
            .AssertForbidden(
                async userService =>
                {
                    var service = SetupSubjectMetaService(
                        userService: userService.Object
                    );

                    return await service.GetSubjectMeta(_releaseSubject);
                }
            );
    }

    [Fact]
    public async Task FilterSubjectMeta()
    {
        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFail(_releaseSubject, DataSecurityPolicies.CanViewSubjectData)
            .AssertForbidden(
                async userService =>
                {
                    var query = new ObservationQueryContext
                    {
                        SubjectId = _releaseSubject.SubjectId
                    };

                    var cancellationToken = new CancellationTokenSource().Token;

                    var service = SetupSubjectMetaService(
                        userService: userService.Object
                    );

                    return await service.FilterSubjectMeta(_releaseSubject, query, cancellationToken);
                }
            );
    }

    private SubjectMetaService SetupSubjectMetaService(
        StatisticsDbContext? statisticsDbContext = null,
        IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
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

    private Mock<IPersistenceHelper<StatisticsDbContext>> DefaultPersistenceHelperMock()
    {
        return MockUtils.MockPersistenceHelper<StatisticsDbContext, ReleaseSubject>(_releaseSubject);
    }
}
