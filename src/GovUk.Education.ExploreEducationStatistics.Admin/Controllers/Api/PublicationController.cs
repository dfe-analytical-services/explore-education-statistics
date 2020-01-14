using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        public async Task<List<MyPublicationViewModel>> GetMyPublicationsAsync(
            [Required] [FromQuery(Name = "topicId")]
            Guid topicId)
        {
            return await _publicationService.GetMyPublicationsAndReleasesByTopicAsync(topicId);
        }

        // GET api/publications/{publicationId}
        [HttpGet("api/publications/{publicationId}")]
        public async Task<ActionResult<MyPublicationViewModel>> GetPublicationByIdAsync(
            [Required] Guid publicationId)
        {
            // var userId = new Guid(); // TODO get the Guid from AD
            var result = await _publicationService.GetViewModelAsync(publicationId);

            if (result != null)
            {
                return result;
            }

            return NoContent();
        }

        // POST api/topic/{topicId}/publications
        [HttpPost("api/topic/{topicId}/publications")]
        public async Task<ActionResult<MyPublicationViewModel>> CreatePublicationAsync(
            CreatePublicationViewModel publication, Guid topicId)
        {
            publication.TopicId = topicId;
            
            return await _publicationService
                .CreatePublicationAsync(publication)
                .HandleFailuresOr(Ok);
        }
    }
}