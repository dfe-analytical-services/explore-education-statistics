using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.SystemFunctions;
using ReleaseId = System.Guid;
using PublicationId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    // TODO rename to Releases once the current Crud releases controller is removed
    [Route("api2")]
    [ApiController]
    [Authorize]
    public class ReleasesController2 : ControllerBase
    {
        private readonly IReleaseService2 _releaseService2;
        private readonly IPublicationService _publicationService;

        public ReleasesController2(IReleaseService2 releaseService2,
             IPublicationService publicationService)
        {
            _releaseService2 = releaseService2;
            _publicationService = publicationService;
        }

        // POST api/publication/{publicationId}/releases
        [HttpPost("publications/{publicationId}/releases")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAsync(CreateReleaseViewModel release,
            PublicationId publicationId)
        {
            return await CheckPublicationExistsAsync(publicationId, () =>
            {
                release.PublicationId = publicationId;
                return _releaseService2.CreateReleaseAsync(release);
            });
        }
        
        [HttpGet("releases/{releaseId}")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ReleaseViewModel> GetReleaseAsync(ReleaseId releaseId)
        {
            return await _releaseService2.GetReleaseForIdAsync(releaseId);
        }

        [HttpGet("releases/{releaseId}/summary")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<EditReleaseSummaryViewModel>> GetReleaseSummaryAsync(ReleaseId releaseId)
        {
            return Ok(await _releaseService2.GetReleaseSummaryAsync(releaseId));
        }

        [HttpPut("releases/{releaseId}/summary")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<ReleaseViewModel>> EditReleaseSummaryAsync(EditReleaseSummaryViewModel model,
            ReleaseId releaseId)
        {
            return await CheckReleaseExistsAsync(releaseId, () =>
            {
                model.Id = releaseId;
                return _releaseService2.EditReleaseSummaryAsync(model);
            });
        }
        
        [HttpGet("publications/{publicationId}/releases")]
        [AllowAnonymous] // TODO We will need to do Authorisation checks when we know what the permissions model is.
        public async Task<ActionResult<List<ReleaseViewModel>>> GetReleaseForPublicationAsync(
            [Required] PublicationId publicationId)
        {
            return Ok(await _releaseService2.GetReleasesForPublicationAsync(publicationId));
        }
        
        private async Task<ActionResult> CheckReleaseExistsAsync<T>(ReleaseId releaseId,
            Func<Task<Either<ValidationResult, T>>> andThen)
        {
            var release = await _releaseService2.GetAsync(releaseId);
            if (release.IsNull())
            {
                return NotFound();
            }

            var result = await andThen.Invoke();
            if (result.IsLeft)
            {
                ValidationUtils.AddErrors(ModelState, result.Left);
                return ValidationProblem();
            }

            return Ok(result.Right);
        }

        private async Task<ActionResult> CheckPublicationExistsAsync<T>(PublicationId publicationId,
            Func<Task<Either<ValidationResult, T>>> andThen)
        {
            var publication = await _publicationService.GetAsync(publicationId);
            if (publication == null)
            {
                return NotFound();
            }

            var result = await andThen.Invoke();
            if (result.IsLeft)
            {
                ValidationUtils.AddErrors(ModelState, result.Left);
                return ValidationProblem();
            }

            return Ok(result.Right);
        }
    }
}