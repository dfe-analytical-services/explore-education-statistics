#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseAmendmentServicePermissionTests
    {
        private readonly Release _release = new()
        {
            Id = Guid.NewGuid(),
            Publication = new Publication(),
            ReleaseParent = new ReleaseParent()
        };

        [Fact]
        public async Task CreateReleaseAmendment()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanMakeAmendmentOfSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        using var contentDbContext = InMemoryApplicationDbContext();
                        contentDbContext.Releases.Add(_release);
                        contentDbContext.SaveChanges();

                        var service = BuildService(userService.Object, contentDbContext);
                        return service.CreateReleaseAmendment(_release.Id);
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
