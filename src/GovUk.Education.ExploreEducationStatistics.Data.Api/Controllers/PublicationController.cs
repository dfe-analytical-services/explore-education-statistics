using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        private readonly IPublicationService _publicationService;

        public PublicationController(IPublicationService publicationService)
        {
            _publicationService = publicationService;
        }

        [HttpGet("publications/{publicationId}")]
        public async Task<ActionResult<SubjectsAndHighlightsViewModel>> GetLatestPublicationSubjectsAndHighlights(Guid publicationId)
        {
            return await _publicationService
                .GetLatestPublicationSubjectsAndHighlights(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}")]
        public async Task<ActionResult<SubjectsAndHighlightsViewModel>> GetReleaseSubjectsAndHighlights(Guid releaseId)
        {
            return await _publicationService
                .GetReleaseSubjectsAndHighlights(releaseId)
                .HandleFailuresOrOk();
        }
    }
}