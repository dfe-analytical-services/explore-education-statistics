#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseAmendmentServicePermissionTests
    {
        private readonly ReleaseVersion _releaseVersion = new()
        {
            Id = Guid.NewGuid(),
            Publication = new Publication(),
            Release = new Release()
        };

        [Fact]
        public async Task CreateReleaseAmendment()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanMakeAmendmentOfSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        using var contentDbContext = InMemoryApplicationDbContext();
                        contentDbContext.ReleaseVersions.Add(_releaseVersion);
                        contentDbContext.SaveChanges();

                        var service = BuildService(userService.Object, contentDbContext);
                        return service.CreateReleaseAmendment(_releaseVersion.Id);
                    }
                );
        }

        private ReleaseAmendmentService BuildService(
            IUserService userService,
            ContentDbContext? context = null)
        {
            return new ReleaseAmendmentService(
                context ?? Mock.Of<ContentDbContext>(),
                userService,
                Mock.Of<IFootnoteRepository>(MockBehavior.Strict),
                Mock.Of<StatisticsDbContext>(MockBehavior.Strict));
        }
    }
}
