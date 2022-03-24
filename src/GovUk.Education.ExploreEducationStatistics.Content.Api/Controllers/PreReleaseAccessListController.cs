#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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

            return await _publicationService.Get(publicationSlug)
                .OnSuccessCombineWith(_ => _releaseService.GetCachedRelease(publicationSlug, releaseSlug))
                .OnSuccess(publicationAndRelease =>
                {
                    var (publication, release) = publicationAndRelease;
                    return new PreReleaseAccessListViewModel(release!, publication);
                })
                .HandleFailuresOrOk();
        }
    }
}
