#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

/// <summary>
/// TODO EES-4661 Remove after the EES-4660 data guidance migration is successful
/// </summary>
public class DataGuidanceMigrationServicePermissionTests
{
    [Fact]
    public async Task MigrateDataGuidance()
    {
        await PolicyCheckBuilder()
            .SetupCheck(SecurityPolicies.IsBauUser, false)
            .AssertForbidden(
                async userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return await service.MigrateDataGuidance();
                }
            );
    }

    private static DataGuidanceMigrationService SetupService(IUserService userService)
    {
        return new DataGuidanceMigrationService(
            Mock.Of<ContentDbContext>(),
            Mock.Of<StatisticsDbContext>(),
            userService
        );
    }
}
