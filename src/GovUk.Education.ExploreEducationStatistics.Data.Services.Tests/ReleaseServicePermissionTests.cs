#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class ReleaseServicePermissionTests
    {
        private readonly ReleaseVersion _releaseVersion = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task ListSubjects()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseMetaService(userService: userService.Object);
                        return service.ListSubjects(_releaseVersion.Id);
                    }
                );
        }

        private ReleaseService BuildReleaseMetaService(
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            StatisticsDbContext? statisticsDbContext = null,
            IUserService? userService = null,
            IDataGuidanceDataSetService? dataGuidanceDataSetService = null,
            ITimePeriodService? timePeriodService = null)
        {
            return new ReleaseService(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                statisticsDbContext ?? Mock.Of<StatisticsDbContext>(),
                userService ?? Mock.Of<IUserService>(),
                dataGuidanceDataSetService ?? Mock.Of<IDataGuidanceDataSetService>(),
                timePeriodService ?? Mock.Of<ITimePeriodService>()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            var mock = MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseVersion>();
            MockUtils.SetupCall(mock, _releaseVersion.Id, _releaseVersion);
            return mock;
        }
    }
}
