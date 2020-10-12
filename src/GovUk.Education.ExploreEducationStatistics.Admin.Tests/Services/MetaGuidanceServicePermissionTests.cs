using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class MetaGuidanceServicePermissionTests
    {
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public void Get()
        {
            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, SecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMetaGuidanceService(userService: userService.Object);
                        return service.Get(_release.Id);
                    }
                );
        }

        [Fact]
        public void UpdateRelease()
        {
            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, SecurityPolicies.CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMetaGuidanceService(userService: userService.Object);
                        return service.UpdateRelease(_release.Id, new MetaGuidanceUpdateReleaseViewModel());
                    }
                );
        }

        [Fact]
        public void UpdateSubject()
        {
            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, SecurityPolicies.CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMetaGuidanceService(userService: userService.Object);
                        return service.UpdateSubject(_release.Id, Guid.NewGuid(),
                            new MetaGuidanceUpdateSubjectViewModel());
                    }
                );
        }

        private MetaGuidanceService SetupMetaGuidanceService(
            ContentDbContext contentDbContext = null,
            StatisticsDbContext statisticsDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IFilterService filterService = null,
            IIndicatorService indicatorService = null,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper = null,
            IUserService userService = null)
        {
            return new MetaGuidanceService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
                filterService ?? new Mock<IFilterService>().Object,
                indicatorService ?? new Mock<IIndicatorService>().Object,
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
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