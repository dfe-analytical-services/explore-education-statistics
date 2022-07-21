#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class FileMigrationServicePermissionTests
{
    [Fact]
    public async Task MigrateAll()
    {
        await PolicyCheckBuilder()
            .SetupCheck(CanRunReleaseMigrations, false)
            .AssertForbidden(
                async userService =>
                {
                    await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext();

                    var service = SetupService(
                        contentDbContext: contentDbContext,
                        userService: userService.Object
                    );

                    return await service.MigrateAll();
                }
            );
    }

    private static FileMigrationService SetupService(
        ContentDbContext contentDbContext,
        IStorageQueueService? storageQueueService = null,
        IUserService? userService = null)
    {
        return new FileMigrationService(
            contentDbContext,
            storageQueueService ?? Mock.Of<IStorageQueueService>(Strict),
            userService ?? Mock.Of<IUserService>(Strict),
            Mock.Of<ILogger<FileMigrationService>>()
        );
    }
}
