#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPatch("releases/migrate-data-blocks")]
        public async Task<ActionResult<Unit>> MigrateDataBlocks()
        {
            return await _dataBlockMigrationService
                .Migrate()
                .HandleFailuresOrOk();
        }
        
        [AllowAnonymous]
        [HttpGet("releases/migrate-all-maps")]
        public async Task<ActionResult<Dictionary<MigrationType, List<DataBlockMapMigrationPlan>>>> MigrateAllMaps()
        {
            return await _dataBlockMigrationService
                .GetMigrateMapPlans()
                .OnSuccess(plans =>
                {
                    return plans
                        .GroupBy(plan => plan.Type)
                        .ToDictionary(
                            group => group.Key, 
                            group => group.ToList());
                })
                .HandleFailuresOrOk();
        }
    }
}
