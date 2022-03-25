#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataBlockMigrationServiceTests
    {
        [Fact]
        public async Task Migrate()
        {
            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext();

            var service = SetupService(contentDbContext);

            var result = await service.Migrate();

            result.AssertRight();
        }

        private static DataBlockMigrationService SetupService(
            ContentDbContext contentDbContext,
            IUserService? userService = null)
        {
            return new DataBlockMigrationService(
                contentDbContext,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                Mock.Of<ILogger<DataBlockMigrationService>>()
            );
        }
    }
}
