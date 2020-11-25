using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.ReleaseFileTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    /**
     * Temporary controller for migrating blobs to use their database file reference id as their filename
     */
    [ApiController]
    [Authorize]
    public class MigrateFilesController : ControllerBase
    {
        private readonly IMigrateFilesService _migrateFilesService;

        public MigrateFilesController(IMigrateFilesService migrateFilesService)
        {
            _migrateFilesService = migrateFilesService;
        }

        [HttpPatch("api/files/ancillary/migrate-filenames")]
        public async Task<ActionResult<Unit>> MigrateAncillaryFilenames()
        {
            return await _migrateFilesService
                .MigrateFilenames(Ancillary)
                .HandleFailuresOrOk();
        }
    }
}