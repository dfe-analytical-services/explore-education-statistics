using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    /**
     * Temporary controller for migrating table storage Imports to the database
     */
    [ApiController]
    [Authorize]
    public class MigrateImportsController : ControllerBase
    {
        private readonly IMigrateImportsService _migrateImportsService;

        public MigrateImportsController(IMigrateImportsService migrateImportsService)
        {
            _migrateImportsService = migrateImportsService;
        }

        [HttpPatch("api/files/migrate-imports")]
        public async Task<ActionResult<Unit>> MigrateImports()
        {
            return await _migrateImportsService
                .MigrateImports()
                .HandleFailuresOrOk();
        }
    }
}