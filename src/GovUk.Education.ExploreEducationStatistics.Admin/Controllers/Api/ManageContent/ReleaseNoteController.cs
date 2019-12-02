using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.ExtensionMethods;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ReleaseNoteController : ControllerBase
    {
        private readonly IReleaseNoteService _releaseNoteService;

        public ReleaseNoteController(IReleaseNoteService releaseNoteService)
        {
            _releaseNoteService = releaseNoteService;
        }

        [HttpPost("release/{releaseId}/content/release-note")]
        public Task<ActionResult<List<ReleaseNoteViewModel>>> AddReleaseNote(CreateOrUpdateReleaseNoteRequest request,
            Guid releaseId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _releaseNoteService.AddReleaseNoteAsync(releaseId, request),
                result => Created(HttpContext.Request.Path, result));
        }

        [HttpPut("release/{releaseId}/content/release-note/{releaseNoteId}")]
        public Task<ActionResult<List<ReleaseNoteViewModel>>> UpdateReleaseNote(
            CreateOrUpdateReleaseNoteRequest request, Guid releaseId, Guid releaseNoteId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _releaseNoteService.UpdateReleaseNoteAsync(releaseId, releaseNoteId, request), Ok);
        }

        [HttpDelete("release/{releaseId}/content/release-note/{releaseNoteId}")]
        public Task<ActionResult<List<ReleaseNoteViewModel>>> DeleteReleaseNote(Guid releaseId, Guid releaseNoteId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _releaseNoteService.DeleteReleaseNoteAsync(releaseId, releaseNoteId), Ok);
        }
    }
}