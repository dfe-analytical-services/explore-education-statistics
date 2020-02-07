using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    // TODO rename to Publications once the current Crud publication controller is removed
    [Authorize]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        private readonly IPublicationService _publicationService;

        public PublicationController(IPublicationService publicationService)
        {
            _publicationService = publicationService;
        }

        // GET api/me/publications?topicId={guid}
        [HttpGet("api/me/publications")]
        public async Task<ActionResult<List<MyPublicationViewModel>>> GetMyPublicationsAsync(
            [Required] [FromQuery(Name = "topicId")]
            Guid topicId)
        {
            return await _publicationService
                .GetMyPublicationsAndReleasesByTopicAsync(topicId)
                .HandleFailuresOr(Ok);
        }

        // GET api/publications/{publicationId}
        [HttpGet("api/publications/{publicationId}")]
        public async Task<ActionResult<PublicationViewModel>> GetPublicationByIdAsync(
            [Required] Guid publicationId)
        {
            return await _publicationService
                .GetViewModelAsync(publicationId)
                .HandleFailuresOr(Ok);
        }
        
        // PUT api/publications/{publicationId}/methodology
        [HttpPut("api/publications/{publicationId}/methodology")]
        public async Task<ActionResult<PublicationViewModel>> UpdatePublicationMethodology(
            UpdatePublicationMethodologyViewModel model, Guid publicationId)
        {
            return Ok();
        }
        
        // POST api/topic/{topicId}/publications
        [HttpPost("api/topic/{topicId}/publications")]
        public async Task<ActionResult<PublicationViewModel>> CreatePublicationAsync(
            CreatePublicationViewModel publication, Guid topicId)
        {
            publication.TopicId = topicId;
            
            return await _publicationService
                .CreatePublicationAsync(publication)
                .HandleFailuresOr(Ok);
        }
    }
}