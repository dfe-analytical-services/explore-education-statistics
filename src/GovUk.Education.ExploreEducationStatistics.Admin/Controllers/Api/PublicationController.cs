using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
        public async Task<ActionResult<List<MyPublicationViewModel>>> GetMyPublications(
            [FromQuery(Name = "topicId"), Required] Guid topicId)
        {
            return await _publicationService
                .GetMyPublicationsAndReleasesByTopic(topicId)
                .HandleFailuresOr(Ok);
        }

        // GET api/publications/{publicationId}
        [HttpGet("api/publications/{publicationId}")]
        public async Task<ActionResult<PublicationViewModel>> GetPublicationById(
            [Required] Guid publicationId)
        {
            return await _publicationService
                .GetViewModel(publicationId)
                .HandleFailuresOr(Ok);
        }

        // PUT api/publications/{publicationId}/methodology
        [HttpPut("api/publications/{publicationId}/methodology")]
        public async Task<ActionResult> UpdatePublicationMethodology(
            UpdatePublicationMethodologyViewModel model, Guid publicationId)
        {
            return await _publicationService
                .UpdatePublicationMethodology(publicationId, model)
                .HandleFailuresOr(result => Ok());
        }
        
        // POST api/topic/{topicId}/publications
        [HttpPost("api/topic/{topicId}/publications")]
        public async Task<ActionResult<PublicationViewModel>> CreatePublication(
            CreatePublicationViewModel publication, Guid topicId)
        {
            publication.TopicId = topicId;
            
            return await _publicationService
                .CreatePublication(publication)
                .HandleFailuresOr(Ok);
        }
    }
}