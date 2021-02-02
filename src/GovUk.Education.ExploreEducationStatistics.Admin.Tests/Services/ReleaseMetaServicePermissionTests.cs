using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseMetaServicePermissionTests
    {
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public void GetSubjectsMeta()
        {
            PermissionTestUtils.PolicyCheckBuilder<ContentSecurityPolicies>()
                .ExpectResourceCheckToFail(_release, ContentSecurityPolicies.CanViewRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseMetaService(userService: userService.Object);
                        return service.GetSubjectsMeta(_release.Id);
                    }
                );
        }

        private ReleaseMetaService BuildReleaseMetaService(
            ContentDbContext contentDbContext = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            StatisticsDbContext statisticsDbContext = null,
            IUserService userService = null,
            IImportStatusService importStatusService = null)
        {
            return new ReleaseMetaService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                userService ?? new Mock<IUserService>().Object,
                importStatusService ?? new Mock<IImportStatusService>().Object
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