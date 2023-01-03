#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

/// <summary>
/// TODO EES-3755 Remove after Permalink snapshot migration work is complete
/// </summary>
public class PermalinkMigrationServicePermissionTests
{
    [Fact]
    public async Task MigrateAll()
    {
        await PolicyCheckBuilder()
            .SetupCheck(SecurityPolicies.IsBauUser, false)
            .AssertForbidden(
                async userService =>
                {
                    var service = SetupService(
                        userService: userService.Object
                    );

                    return await service.MigrateAll();
                }
            );
    }

    private static PermalinkMigrationService SetupService(
        IStorageQueueService? storageQueueService = null,
        IUserService? userService = null)
    {
        return new PermalinkMigrationService(
            storageQueueService ?? Mock.Of<IStorageQueueService>(Strict),
            userService ?? Mock.Of<IUserService>(Strict)
        );
    }
}
