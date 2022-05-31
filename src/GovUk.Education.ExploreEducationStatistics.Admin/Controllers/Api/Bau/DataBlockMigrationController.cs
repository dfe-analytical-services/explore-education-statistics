#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.DataBlockMigrationService;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.DataBlockMigrationService.DataBlockMapMigrationPlan;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau
{
    /**
     * Controller used to migrate data blocks. Intended to be adapted as needed when migrations are required.
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
        
        [HttpPatch("releases/migrate-all-maps")]
        public async Task<ActionResult<List<MapMigrationResult>>> MigrateAllMaps(
            [FromQuery] bool dryRun = true)
        {
            return await _dataBlockMigrationService
                .MigrateMaps(dryRun)
                .HandleFailuresOrOk();
        }
    }
}
