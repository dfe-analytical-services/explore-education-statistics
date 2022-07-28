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
        private readonly Release _release = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task ListSubjects()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(_release, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseMetaService(userService: userService.Object);
                        return service.ListSubjects(_release.Id);
                    }
                );
        }

        private ReleaseService BuildReleaseMetaService(
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            StatisticsDbContext? statisticsDbContext = null,
            IUserService? userService = null,
            IDataGuidanceSubjectService? dataGuidanceSubjectService = null,
            ITimePeriodService? timePeriodService = null)
        {
            return new ReleaseService(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                statisticsDbContext ?? Mock.Of<StatisticsDbContext>(),
                userService ?? Mock.Of<IUserService>(),
                dataGuidanceSubjectService ?? Mock.Of<IDataGuidanceSubjectService>(),
                timePeriodService ?? Mock.Of<ITimePeriodService>()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            var mock = MockUtils.MockPersistenceHelper<ContentDbContext, Release>();
            MockUtils.SetupCall(mock, _release.Id, _release);
            return mock;
        }
    }
}
