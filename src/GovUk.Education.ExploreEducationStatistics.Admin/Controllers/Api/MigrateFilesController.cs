using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

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

        [HttpPatch("api/files/private/ancillary/migrate-filenames")]
        public async Task<ActionResult<Unit>> MigratePrivateAncillaryFilenames()
        {
            return await _migrateFilesService
                .MigratePrivateFiles(Ancillary)
                .HandleFailuresOrOk();
        }

        [HttpPatch("api/files/public/ancillary/migrate-filenames")]
        public async Task<ActionResult<Unit>> MigratePublicAncillaryFilenames()
        {
            return await _migrateFilesService
                .MigratePublicFiles(Ancillary)
                .HandleFailuresOrOk();
        }

        [HttpPatch("api/files/public/chart/migrate-filenames")]
        public async Task<ActionResult<Unit>> MigratePublicChartFilenames()
        {
            return await _migrateFilesService
                .MigratePublicFiles(Chart, lenient: true)
                .HandleFailuresOrOk();
        }
    }
}