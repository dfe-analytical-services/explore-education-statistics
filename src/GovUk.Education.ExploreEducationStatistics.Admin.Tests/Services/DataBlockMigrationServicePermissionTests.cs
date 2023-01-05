#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataBlockMigrationServicePermissionTests
    {
        [Fact]
        public async Task Migrate()
        {
            await PolicyCheckBuilder()
                .SetupCheck(SecurityPolicies.IsBauUser, false)
                .AssertForbidden(
                    async userService =>
                    {
                        await using var contentDbContext = InMemoryContentDbContext();
                        await using var statisticsDbContext = InMemoryStatisticsDbContext();

                        var service = SetupService(
                            contentDbContext,
                            statisticsDbContext,
                            userService.Object
                        );

                        return await service.MigrateMaps();
                    }
                );
        }

        private static DataBlockMigrationService SetupService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IUserService? userService = null)
        {
            return new DataBlockMigrationService(
                contentDbContext,
                statisticsDbContext,
                userService ?? Mock.Of<IUserService>(Strict),
                Mock.Of<ILogger<DataBlockMigrationService>>()
            );
        }
    }
}
