using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.ExtensionMethods;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
        public Task<ActionResult<List<Update>>> AddRelatedInformation(CreateReleaseNoteRequest request, Guid releaseId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _releaseNoteService.AddReleaseNoteAsync(releaseId, request),
                result => Created(HttpContext.Request.Path, result));
        }
    }
}