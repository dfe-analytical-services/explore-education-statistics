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
     * Temporary controller for migrating blobs to use a new file structure
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

        [HttpPatch("api/files/private/data/migrate-created-fields")]
        public async Task<ActionResult<Unit>> MigratePrivateDataFilesCreatedFields()
        {
            return await _migrateFilesService
                .MigratePrivateFilesCreatedFields()
                .HandleFailuresOrOk();
        }

        [HttpPatch("api/files/private/data/migrate-files")]
        public async Task<ActionResult<Unit>> MigratePrivateDataFiles()
        {
            return await _migrateFilesService
                .MigratePrivateFiles(FileType.Data, Metadata, DataZip)
                .HandleFailuresOrOk();
        }

        [HttpPatch("api/files/public/ancillary/migrate-files")]
        public async Task<ActionResult<Unit>> MigratePublicAncillaryFiles()
        {
            return await _migrateFilesService
                .MigratePublicFiles(Ancillary)
                .HandleFailuresOrOk();
        }

        [HttpPatch("api/files/public/chart/migrate-files")]
        public async Task<ActionResult<Unit>> MigratePublicChartFiles()
        {
            return await _migrateFilesService
                .MigratePublicFiles(Chart)
                .HandleFailuresOrOk();
        }

        [HttpPatch("api/files/public/data/migrate-files")]
        public async Task<ActionResult<Unit>> MigratePublicDataFiles()
        {
            return await _migrateFilesService
                .MigratePublicFiles(FileType.Data)
                .HandleFailuresOrOk();
        }
    }
}
