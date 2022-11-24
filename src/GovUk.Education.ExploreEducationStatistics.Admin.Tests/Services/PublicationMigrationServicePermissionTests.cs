#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

/// <summary>
/// TODO EES-3882 Remove after migration has been run by EES-3894
/// </summary>
public class PublicationMigrationServicePermissionTests
{
    [Fact]
    public async Task SetLatestPublishedReleases()
    {
        await PermissionTestUtil.PolicyCheckBuilder()
            .SetupCheck(SecurityPolicies.CanRunReleaseMigrations, false)
            .AssertForbidden(
                async userService =>
                {
                    var service = SetupService(
                        userService: userService.Object
                    );

                    return await service.SetLatestPublishedReleases();
                }
            );
    }

    private static PublicationMigrationService SetupService(
        ContentDbContext? contentDbContext = null,
        IUserService? userService = null,
        ILogger<PublicationMigrationService>? logger = null)
    {
        return new PublicationMigrationService(
            contentDbContext ?? DbUtils.InMemoryApplicationDbContext(),
            userService ?? Mock.Of<IUserService>(MockBehavior.Strict),
            logger ?? Mock.Of<ILogger<PublicationMigrationService>>()
        );
    }
}
