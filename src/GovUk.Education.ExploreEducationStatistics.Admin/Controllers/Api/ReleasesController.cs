using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ReleasesController : ControllerBase
    {
        private readonly IReleaseService _releaseService;
        private readonly IReleaseFilesService _releaseFilesService;
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly IReplacementService _replacementService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDataBlockService _dataBlockService;

        public ReleasesController(
            IReleaseService releaseService,
            IReleaseFilesService releaseFilesService,
            IReleaseStatusService releaseStatusService,
            IReplacementService replacementService,
            UserManager<ApplicationUser> userManager,
            IDataBlockService dataBlockService
            )
        {
            _releaseService = releaseService;
            _releaseFilesService = releaseFilesService;
            _releaseStatusService = releaseStatusService;
            _replacementService = replacementService;
            _userManager = userManager;
            _dataBlockService = dataBlockService;
        }

        [HttpGet("release/{releaseId}/chart/{id}")]
        public async Task<ActionResult> GetChartFile(Guid releaseId, Guid id)
        {
            return await _releaseFilesService
                .StreamFile(releaseId, ReleaseFileTypes.Chart, id)
                .HandleFailures();
        }

        [HttpGet("release/{releaseId}/data/{filename}")]
        public async Task<ActionResult> GetDataFile(Guid releaseId, string filename)
        {
            return await _releaseFilesService
                .StreamFile(releaseId, ReleaseFileTypes.Data, filename)
                .HandleFailures();
        }

        [HttpGet("release/{releaseId}/meta/{filename}")]
        public async Task<ActionResult> GetMetaFile(Guid releaseId, string filename)
        {
            return await _releaseFilesService
                .StreamFile(releaseId, ReleaseFileTypes.Metadata, filename)
                .HandleFailures();
        }

        [HttpGet("release/{releaseId}/ancillary/{filename}")]
        public async Task<ActionResult> GetAncillaryFile(Guid releaseId, string filename)
        {
            return await _releaseFilesService
                .StreamFile(releaseId, ReleaseFileTypes.Ancillary, filename)
                .HandleFailures();
        }

        [HttpPost("publications/{publicationId}/releases")]
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAsync(CreateReleaseViewModel release,
            Guid publicationId)
        {
            release.PublicationId = publicationId;

            return await _releaseService
                .CreateReleaseAsync(release)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId}")]
        public async Task<ActionResult<ReleaseViewModel>> DeleteReleaseAsync(Guid releaseId)
        {
            return await _releaseService
                .DeleteReleaseAsync(releaseId)
                .HandleFailuresOr(_ => NoContent());
        }

        [HttpPost("release/{releaseId}/amendment")]
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAmendmentAsync(Guid releaseId)
        {
            return await _releaseService
                .CreateReleaseAmendmentAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<DataFileInfo>>> GetDataFilesAsync(Guid releaseId)
        {
            return await _releaseFilesService
                .ListDataFilesAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/ancillary")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetAncillaryFilesAsync(Guid releaseId)
        {
            return await _releaseFilesService
                .ListFilesAsync(releaseId, ReleaseFileTypes.Ancillary)
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
            return await _releaseFilesService
                .UploadFileAsync(releaseId, file, name, ReleaseFileTypes.Ancillary, false)
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
            return await _releaseFilesService
                .UploadChartFileAsync(releaseId, file)
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
            return await _releaseFilesService
                .UploadChartFileAsync(releaseId, file, id)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<DataFileInfo>> AddDataFilesAsync(Guid releaseId,
            [FromQuery(Name = "name"), Required] string name, IFormFile file, IFormFile metaFile)
        {
            var user = await _userManager.GetUserAsync(User);

            return await _releaseFilesService
                .UploadDataFilesAsync(releaseId, file, metaFile, name, user.Email)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}/zip-data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<DataFileInfo>> AddDataZipFileAsync(Guid releaseId,
            [FromQuery(Name = "name"), Required] string name, IFormFile zipFile)
        {
            var user = await _userManager.GetUserAsync(User);

            return await _releaseFilesService
                .UploadDataFilesAsZipAsync(releaseId, zipFile, name, user.Email)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}")]
        public async Task<ActionResult<ReleaseViewModel>> GetReleaseAsync(Guid releaseId)
        {
            return await _releaseService
                .GetReleaseForIdAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}/summary")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> GetReleaseSummaryAsync(Guid releaseId)
        {
            return await _releaseService
                .GetReleaseSummaryAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}/publication-status")]
        public async Task<ActionResult<ReleasePublicationStatusViewModel>> GetReleasePublicationStatusAsync(Guid releaseId)
        {
            return await _releaseService
                .GetReleasePublicationStatusAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPut("releases/{releaseId}")]
        public async Task<ActionResult<ReleaseViewModel>> UpdateRelease(UpdateReleaseViewModel request,
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

        [HttpGet("release/{releaseId}/data/{fileName}/delete-plan")]
        public async Task<ActionResult<DeleteDataFilePlan>> GetDeleteDataFilePlan(Guid releaseId,
            string fileName,
            [FromQuery(Name = "name"), Required] string subjectTitle)
        {
            return await _releaseService
                .GetDeleteDataFilePlan(releaseId, fileName, subjectTitle)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/data/replacement-plan")]
        public async Task<ActionResult<ReplacementPlanViewModel>> GetReplacementPlan(ReplacementPlanRequest request)
        {
            return await _replacementService
                .GetReplacementPlan(request.OriginalSubjectId.Value, request.ReplacementSubjectId.Value)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/data/replace")]
        public async Task<ActionResult<Unit>> Replace(ReplacementPlanRequest request)
        {
            return await _replacementService
                .Replace(request.OriginalSubjectId.Value, request.ReplacementSubjectId.Value)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId}/data/{fileName}")]
        public async Task<ActionResult> DeleteDataFiles(Guid releaseId,
            string fileName,
            [FromQuery(Name = "name"), Required] string subjectTitle)
        {
            return await _releaseService
                .RemoveDataFilesAsync(releaseId, fileName, subjectTitle)
                .HandleFailuresOrNoContent();
        }

        [HttpDelete("release/{releaseId}/ancillary/{fileName}")]
        public async Task<ActionResult> DeleteAncillaryFile(
            Guid releaseId, string fileName)
        {
            return await _releaseFilesService
                .DeleteNonDataFileAsync(releaseId, ReleaseFileTypes.Ancillary, fileName)
                .HandleFailuresOrNoContent();
        }

        [HttpDelete("release/{releaseId}/chart/{id}")]
        public async Task<ActionResult> DeleteChartFile(
            Guid releaseId, string subjectName, Guid id)
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
    }
}