#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    public class PreReleaseAccessListController : ControllerBase
    {
        private readonly IPublicationService _publicationService;
        private readonly IReleaseService _releaseService;

        public PreReleaseAccessListController(
            IPublicationService publicationService,
            IReleaseService releaseService)
        {
            _publicationService = publicationService;
            _releaseService = releaseService;
        }

        [HttpGet("publications/{publicationSlug}/releases/latest/prerelease-access-list")]
        public async Task<ActionResult<PreReleaseAccessListViewModel>> GetLatest(string publicationSlug)
        {
            return await GetViewModel(
                publicationSlug
            );
        }

        [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/prerelease-access-list")]
        public async Task<ActionResult<PreReleaseAccessListViewModel>> Get(
            string publicationSlug,
            string releaseSlug)
        {
            return await GetViewModel(
                publicationSlug,
                releaseSlug
            );
        }

        private async Task<ActionResult<PreReleaseAccessListViewModel>> GetViewModel(
            string publicationSlug,
            string? releaseSlug = null)
        {
            var publicationTask = _publicationService.Get(publicationSlug);
            var releaseTask = _releaseService.FetchCachedRelease(publicationSlug, releaseSlug);

            await Task.WhenAll(publicationTask, releaseTask);

            if (releaseTask.Result.IsRight && publicationTask.Result.IsRight)
            {
                return new PreReleaseAccessListViewModel(releaseTask.Result.Right!, publicationTask.Result.Right);
            }

            return NotFound();
        }
    }
}
