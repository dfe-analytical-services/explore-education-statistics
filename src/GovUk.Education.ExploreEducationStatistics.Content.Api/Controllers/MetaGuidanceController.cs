using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    public class MetaGuidanceController
    {
        private readonly IMetaGuidanceService _metaGuidanceService;

        public MetaGuidanceController(IMetaGuidanceService metaGuidanceService)
        {
            _metaGuidanceService = metaGuidanceService;
        }

        [HttpGet("publications/{publicationSlug}/releases/latest/meta-guidance")]
        public async Task<ActionResult<MetaGuidanceViewModel>> GetLatest(string publicationSlug)
        {
            return await _metaGuidanceService.Get(
                    publicationPath: PublicContentPublicationPath(publicationSlug),
                    releasePath: PublicContentLatestReleasePath(publicationSlug)
                )
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/meta-guidance")]
        public async Task<ActionResult<MetaGuidanceViewModel>> Get(
            string publicationSlug,
            string releaseSlug)
        {
            return await _metaGuidanceService.Get(
                    publicationPath: PublicContentPublicationPath(publicationSlug),
                    releasePath: PublicContentReleasePath(publicationSlug, releaseSlug)
                )
                .HandleFailuresOrOk();
        }
    }
}