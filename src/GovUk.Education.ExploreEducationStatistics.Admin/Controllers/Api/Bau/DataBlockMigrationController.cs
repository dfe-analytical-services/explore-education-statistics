#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau
{
    /**
     * Temporary controller used to migrate data blocks for EES-3167.
     */
    [Route("api")]
    [ApiController]
    [Authorize(Roles = RoleNames.BauUser)]
    public class DataBlockMigrationController : ControllerBase
    {
        private readonly IDataBlockMigrationService _dataBlockMigrationService;

        public DataBlockMigrationController(IDataBlockMigrationService dataBlockMigrationService)
        {
            _dataBlockMigrationService = dataBlockMigrationService;
        }

        [HttpPatch("releases/migrate-all-data-blocks")]
        public async Task<ActionResult<Unit>> MigrateAllDataBlocks()
        {
            return await _dataBlockMigrationService
                .MigrateAll()
                .HandleFailuresOrOk();
        }
    }
}
