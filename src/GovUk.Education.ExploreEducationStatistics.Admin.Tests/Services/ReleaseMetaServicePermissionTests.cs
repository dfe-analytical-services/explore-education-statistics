using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseMetaServicePermissionTests
    {
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public void GetSubjects()
        {
            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseMetaService(userService: userService.Object);
                        return service.GetSubjects(_release.Id);
                    }
                );
        }

        private ReleaseMetaService BuildReleaseMetaService(
            ContentDbContext contentDbContext = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            StatisticsDbContext statisticsDbContext = null,
            IUserService userService = null)
        {
            return new ReleaseMetaService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                userService ?? new Mock<IUserService>().Object
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