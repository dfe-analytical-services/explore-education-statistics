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
    }
}
