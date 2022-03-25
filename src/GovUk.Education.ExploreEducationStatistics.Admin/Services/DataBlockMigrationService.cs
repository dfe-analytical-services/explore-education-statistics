#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataBlockMigrationService : IDataBlockMigrationService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IUserService _userService;
        private readonly ILogger<DataBlockMigrationService> _logger;

        public DataBlockMigrationService(
            ContentDbContext contentDbContext,
            IUserService userService,
            ILogger<DataBlockMigrationService> logger)
        {
            _contentDbContext = contentDbContext;
            _userService = userService;
            _logger = logger;
        }

        public async Task<Either<ActionResult, Unit>> Migrate()
        {
            return await _userService
                .CheckCanRunMigrations()
                .OnSuccessVoid(() =>
                {
                    // Implement migration here...
                    _logger.LogWarning("Migration not implemented");
                });
        }
    }
}
