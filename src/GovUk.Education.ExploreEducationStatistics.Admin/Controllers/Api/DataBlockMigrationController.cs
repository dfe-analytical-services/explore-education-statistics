using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    /**
     * Temporary controller used to migrate datablocks for EES-17.
     */
    [Route("api")]
    [ApiController]
    [Authorize]
    public class DataBlockMigrationController : ControllerBase
    {
        private readonly IDataBlockMigrationService _dataBlockMigrationService;

        public DataBlockMigrationController(IDataBlockMigrationService dataBlockMigrationService)
        {
            _dataBlockMigrationService = dataBlockMigrationService;
        }

        [HttpPost("releases/migrate-all-data-blocks")]
        public async Task<ActionResult<bool>> MigrateAllDataBlocks()
        {
            return await _dataBlockMigrationService
                .MigrateAll()
                .HandleFailuresOrOk();
        }
    }
}