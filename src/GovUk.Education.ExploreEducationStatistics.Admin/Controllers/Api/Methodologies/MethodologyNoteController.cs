#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies
{
    [Route("api")]
    [Authorize]
    [ApiController]
    public class MethodologyNoteController : ControllerBase
    {
        private readonly IMethodologyNoteService _methodologyNoteService;

        public MethodologyNoteController(IMethodologyNoteService methodologyNoteService)
        {
            _methodologyNoteService = methodologyNoteService;
        }

        [HttpPost("methodologies/{methodologyVersionId:guid}/notes")]
        public async Task<ActionResult<MethodologyNoteViewModel>> AddNote(
            Guid methodologyVersionId,
            MethodologyNoteAddRequest request)
        {
            return await _methodologyNoteService
                .AddNote(methodologyVersionId, request)
                .HandleFailuresOr(result => Created(HttpContext.Request.Path, result));
        }

        [HttpDelete("methodologies/{methodologyVersionId:guid}/notes/{methodologyNoteId:guid}")]
        public async Task<ActionResult> DeleteNote(
            Guid methodologyVersionId,
            Guid methodologyNoteId)
        {
            return await _methodologyNoteService
                .DeleteNote(methodologyVersionId, methodologyNoteId)
                .HandleFailuresOrNoContent();
        }

        [HttpPut("methodologies/{methodologyVersionId:guid}/notes/{methodologyNoteId:guid}")]
        public async Task<ActionResult<MethodologyNoteViewModel>> UpdateNote(
            Guid methodologyVersionId, Guid methodologyNoteId, MethodologyNoteUpdateRequest request)
        {
            return await _methodologyNoteService
                .UpdateNote(methodologyVersionId, methodologyNoteId, request)
                .HandleFailuresOrOk();
        }
    }
}
