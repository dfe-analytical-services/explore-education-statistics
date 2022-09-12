#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    public class PreReleaseAccessListController : ControllerBase
    {
        private readonly IPublicationCacheService _publicationCacheService;
        private readonly IReleaseCacheService _releaseCacheService;

        public PreReleaseAccessListController(IPublicationCacheService publicationCacheService,
            IReleaseCacheService releaseCacheService)
        {
            _publicationCacheService = publicationCacheService;
            _releaseCacheService = releaseCacheService;
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
            return await _publicationCacheService.GetPublication(publicationSlug)
                .OnSuccessCombineWith(_ => _releaseCacheService.GetRelease(publicationSlug, releaseSlug))
                .OnSuccess(publicationAndRelease =>
                {
                    var (publication, release) = publicationAndRelease;
                    return new PreReleaseAccessListViewModel(release, publication);
                })
                .HandleFailuresOrOk();
        }
    }
}
