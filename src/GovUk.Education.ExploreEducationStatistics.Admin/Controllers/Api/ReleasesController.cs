using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ReleasesController : ControllerBase
    {
        private readonly IReleaseService _releaseService;
        private readonly IReleaseFileService _releaseFileService;
        private readonly IReleaseDataFileService _releaseDataFileService;
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly IReleaseChecklistService _releaseChecklistService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDataBlockService _dataBlockService;
        private readonly IImportService _importService;

        public ReleasesController(
            IReleaseService releaseService,
            IReleaseFileService releaseFileService,
            IReleaseDataFileService releaseDataFileService,
            IReleaseStatusService releaseStatusService,
            IReleaseChecklistService releaseChecklistService,
            UserManager<ApplicationUser> userManager,
            IDataBlockService dataBlockService,
            IImportService importService)
        {
            _releaseService = releaseService;
            _releaseDataFileService = releaseDataFileService;
            _releaseFileService = releaseFileService;
            _releaseStatusService = releaseStatusService;
            _releaseChecklistService = releaseChecklistService;
            _userManager = userManager;
            _dataBlockService = dataBlockService;
            _importService = importService;
        }

        [HttpGet("release/{releaseId}/file/{id}")]
        public async Task<ActionResult> GetFile(Guid releaseId, Guid id)
        {
            return await _releaseFileService
                .Stream(releaseId, id)
                .HandleFailures();
        }

        [HttpPost("publications/{publicationId}/releases")]
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAsync(ReleaseCreateViewModel release,
            Guid publicationId)
        {
            release.PublicationId = publicationId;

            return await _releaseService
                .CreateReleaseAsync(release)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId}")]
        public async Task<ActionResult<ReleaseViewModel>> DeleteRelease(Guid releaseId)
        {
            return await _releaseService
                .DeleteRelease(releaseId)
                .HandleFailuresOrNoContent();
        }

        [HttpPost("release/{releaseId}/amendment")]
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAmendmentAsync(Guid releaseId)
        {
            return await _releaseService
                .CreateReleaseAmendmentAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/data/{fileId}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DataFileInfo>> GetDataFileInfo(Guid releaseId, Guid fileId)
        {
            return await _releaseDataFileService
                .GetInfo(releaseId, fileId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<DataFileInfo>>> GetDataFileInfo(Guid releaseId)
        {
            return await _releaseDataFileService
                .ListAll(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/ancillary")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetAncillaryFilesAsync(Guid releaseId)
        {
            return await _releaseFileService
                .ListAll(releaseId, Ancillary)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}/ancillary")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<FileInfo>> AddAncillaryFileAsync(Guid releaseId,
            [FromQuery(Name = "name"), Required] string name, IFormFile file)
        {
            return await _releaseFileService
                .UploadAncillary(releaseId, file, name)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}/chart")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<FileInfo>> AddChartFileAsync(Guid releaseId, IFormFile file)
        {
            return await _releaseFileService
                .UploadChart(releaseId, file)
                .HandleFailuresOrOk();
        }

        [HttpPut("release/{releaseId}/chart/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<FileInfo>> UpdateChartFileAsync(Guid releaseId, Guid id, IFormFile file)
        {
            return await _releaseFileService
                .UploadChart(releaseId, file, replacingId: id)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<DataFileInfo>> AddDataFilesAsync(Guid releaseId,
            [FromQuery(Name = "replacingFileId")] Guid? replacingFileId,
            [FromQuery(Name = "name")] string subjectName,
            IFormFile file,
            IFormFile metaFile)
        {
            var user = await _userManager.GetUserAsync(User);

            return await _releaseDataFileService
                .Upload(releaseId: releaseId,
                    dataFormFile: file,
                    metaFormFile: metaFile,
                    userName: user.Email,
                    replacingFileId: replacingFileId,
                    subjectName: subjectName)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}/zip-data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<DataFileInfo>> AddDataZipFileAsync(Guid releaseId,
            [FromQuery(Name = "replacingFileId")] Guid? replacingFileId,
            [FromQuery(Name = "name")] string subjectName,
            IFormFile zipFile)
        {
            var user = await _userManager.GetUserAsync(User);

            return await _releaseDataFileService
                .UploadAsZip(releaseId: releaseId,
                    zipFormFile: zipFile,
                    userName: user.Email,
                    replacingFileId: replacingFileId,
                    subjectName: subjectName)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}")]
        public async Task<ActionResult<ReleaseViewModel>> GetRelease(Guid releaseId)
        {
            return await _releaseService
                .GetRelease(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}/publication-status")]
        public async Task<ActionResult<ReleasePublicationStatusViewModel>> GetReleasePublicationStatusAsync(
            Guid releaseId)
        {
            return await _releaseService
                .GetReleasePublicationStatusAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPut("releases/{releaseId}")]
        public async Task<ActionResult<ReleaseViewModel>> UpdateRelease(ReleaseUpdateViewModel request,
            Guid releaseId)
        {
            return await _releaseService
                .UpdateRelease(releaseId, request)
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationId}/releases/template")]
        public async Task<ActionResult<TitleAndIdViewModel?>> GetTemplateReleaseAsync(
            [Required] Guid publicationId)
        {
            return await _releaseService
                .GetLatestReleaseAsync(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/draft")]
        public async Task<ActionResult<List<MyReleaseViewModel>>> GetDraftReleasesAsync()
        {
            return await _releaseService
                .GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Draft, ReleaseStatus.HigherLevelReview)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/scheduled")]
        public async Task<ActionResult<List<MyReleaseViewModel>>> GetScheduledReleasesAsync()
        {
            return await _releaseService
                .GetMyScheduledReleasesAsync()
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/data/{fileName}/import/status")]
        public Task<ActionResult<ImportStatus>> GetDataUploadStatus(Guid releaseId, string fileName)
        {
            return _releaseService
                .GetDataFileImportStatus(releaseId, fileName)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/data/{fileId}/delete-plan")]
        public async Task<ActionResult<DeleteDataFilePlan>> GetDeleteDataFilePlan(Guid releaseId, Guid fileId)
        {
            return await _releaseService
                .GetDeleteDataFilePlan(releaseId, fileId)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId}/data/{fileId}")]
        public async Task<ActionResult> DeleteDataFiles(Guid releaseId,
            Guid fileId)
        {
            return await _releaseService
                .RemoveDataFiles(releaseId, fileId)
                .HandleFailuresOrNoContent();
        }

        [HttpPost("release/{releaseId}/data/{dataFileName}/import/cancel")]
        public async Task<IActionResult> CancelFileImport([FromRoute] ReleaseFileImportInfo file)
        {
            return await _importService
                .CancelImport(file)
                .HandleFailuresOr(result => new AcceptedResult());
        }

        [HttpDelete("release/{releaseId}/ancillary/{fileId}")]
        public async Task<ActionResult> DeleteAncillaryFile(
            Guid releaseId, Guid fileId)
        {
            return await _releaseFileService
                .Delete(releaseId, fileId)
                .HandleFailuresOrNoContent();
        }

        [HttpDelete("release/{releaseId}/chart/{id}")]
        public async Task<ActionResult> DeleteChartFile(
            Guid releaseId, Guid id)
        {
            return await _dataBlockService.RemoveChartFile(releaseId, id)
                .HandleFailuresOrNoContent();
        }

        [HttpGet("releases/{releaseId}/status")]
        public async Task<ActionResult<ReleaseStatusViewModel>> GetReleaseStatusesAsync(Guid releaseId)
        {
            return await _releaseStatusService
                .GetReleaseStatusAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}/checklist")]
        public async Task<ActionResult<ReleaseChecklistViewModel>> GetChecklist(Guid releaseId)
        {
            return await _releaseChecklistService
                .GetChecklist(releaseId)
                .HandleFailuresOrOk();
        }
    }
}