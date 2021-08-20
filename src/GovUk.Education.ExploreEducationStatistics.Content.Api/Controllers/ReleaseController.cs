#nullable enable
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ReleaseController : ControllerBase
    {
        private readonly IReleaseService _releaseService;

        public ReleaseController(IReleaseService releaseService)
        {
            _releaseService = releaseService;
        }

        [HttpGet("publications/{publicationSlug}/releases")]
        public async Task<ActionResult<List<ReleaseSummaryViewModel>>> ListReleases(string publicationSlug)
        {
            return await _releaseService.List(publicationSlug)
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationSlug}/releases/latest")]
        public async Task<ActionResult<ReleaseViewModel>> GetLatestRelease(string publicationSlug)
        {
            return await _releaseService.Get(
                    PublicContentPublicationPath(publicationSlug),
                    PublicContentLatestReleasePath(publicationSlug))
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationSlug}/releases/latest/summary")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> GetLatestReleaseSummary(string publicationSlug)
        {
            return await _releaseService.GetSummary(
                    PublicContentPublicationPath(publicationSlug),
                    PublicContentLatestReleasePath(publicationSlug))
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}")]
        public async Task<ActionResult<ReleaseViewModel>> GetRelease(string publicationSlug, string releaseSlug)
        {
            return await _releaseService.Get(
                    PublicContentPublicationPath(publicationSlug),
                    PublicContentReleasePath(publicationSlug, releaseSlug))
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/summary")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> GetReleaseSummary(string publicationSlug,
            string releaseSlug)
        {
            return await _releaseService.GetSummary(
                    PublicContentPublicationPath(publicationSlug),
                    PublicContentReleasePath(publicationSlug, releaseSlug))
                .HandleFailuresOrOk();
        }
    }
}
