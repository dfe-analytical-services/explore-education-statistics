#nullable enable
#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseAmendmentServicePermissionTests
    {
        private static readonly Publication Publication = new()
        {
            Id = Guid.NewGuid()
        };

        private readonly Release _release = new()
        {
            Id = Guid.NewGuid(),
            PublicationId = Publication.Id,
            Published = DateTime.Now,
            TimePeriodCoverage = TimeIdentifier.April
        };

        [Fact]
        public async Task CreateReleaseAmendment()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanMakeAmendmentOfSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        using var contentDbContext = InMemoryApplicationDbContext("CreateReleaseAmendmentAsync");
                        contentDbContext.Attach(_release);
                        var service = BuildReleaseService(
                            userService.Object,
                            contentDbContext);
                        return service.CreateReleaseAmendment(_release.Id);
                    }
                );
        }

        private ReleaseAmendmentService BuildReleaseService(
            IUserService userService,
            ContentDbContext? context = null,
            IReleaseService? releaseService = null)
        {
            return new ReleaseAmendmentService(
                context ?? Mock.Of<ContentDbContext>(),
                releaseService ?? Mock.Of<IReleaseService>(MockBehavior.Strict),
                DefaultPersistenceHelperMock().Object,
                userService,
                Mock.Of<IFootnoteService>(MockBehavior.Strict),
                Mock.Of<StatisticsDbContext>(MockBehavior.Strict));
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            var mock = MockUtils.MockPersistenceHelper<ContentDbContext, Release>();
            MockUtils.SetupCall(mock, _release.Id, _release);
            MockUtils.SetupCall(mock, Publication.Id, Publication);

            return mock;
        }
    }
}
