using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    // TODO rename to Publications once the current Crud publication controller is removed
    [ApiController]
    [Authorize]
    public class PublicationController : ControllerBase
    {
        private readonly IPublicationService _publicationService;

        public PublicationController(IPublicationService publicationService)
        {
            _publicationService = publicationService;
        }

        // GET api/me/publications?topicId={guid}
        [HttpGet("/me/publications")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public ActionResult<List<Publication>> GetPublications(
            [Required] [FromQuery(Name = "topicId")]
            Guid topicId)
        {
            var userId = new Guid(); // TODO get the Guid from AD
            var result = _publicationService.GetByTopicAndUser(topicId, userId);

            if (result.Any())
            {
                return result;
            }

            return NoContent();
        }
    }
}