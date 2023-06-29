#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

/// <summary>
/// TODO EES-4372 Remove after the EES-4364 filter migration is complete
/// </summary>
public class FilterMigrationServicePermissionTests
{
    [Fact]
    public async Task MigrateGroupCsvColumns()
    {
        await PolicyCheckBuilder()
            .SetupCheck(SecurityPolicies.IsBauUser, false)
            .AssertForbidden(
                async userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return await service.MigrateGroupCsvColumns();
                }
            );
    }

    private static FilterMigrationService SetupService(
        ContentDbContext? contentDbContext = null,
        StatisticsDbContext? statisticsDbContext = null,
        IPrivateBlobStorageService? privateBlobStorageService = null,
        IUserService? userService = null,
        ILogger<FilterMigrationService>? logger = null)
    {
        return new FilterMigrationService(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            statisticsDbContext ?? Mock.Of<StatisticsDbContext>(),
            privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(),
            userService ?? Mock.Of<IUserService>(),
            logger ?? Mock.Of<ILogger<FilterMigrationService>>()
        );
    }
}
