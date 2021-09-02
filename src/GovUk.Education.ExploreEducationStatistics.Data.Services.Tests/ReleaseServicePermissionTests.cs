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
        private readonly Release _release = new Release
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
            IMetaGuidanceSubjectService? metaGuidanceSubjectService = null)
        {
            return new ReleaseService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                userService ?? new Mock<IUserService>().Object,
                metaGuidanceSubjectService ?? new Mock<IMetaGuidanceSubjectService>().Object
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