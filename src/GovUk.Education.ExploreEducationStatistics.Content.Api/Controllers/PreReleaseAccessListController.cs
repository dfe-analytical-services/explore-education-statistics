using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    public class PreReleaseAccessListController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public PreReleaseAccessListController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpGet("publications/{publicationSlug}/releases/latest/prerelease-access-list")]
        public async Task<ActionResult<PreReleaseAccessListViewModel>> GetLatest(string publicationSlug)
        {
            return await GetViewModel(
                PublicContentPublicationPath(publicationSlug),
                PublicContentLatestReleasePath(publicationSlug)
            );
        }

        [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/prerelease-access-list")]
        public async Task<ActionResult<PreReleaseAccessListViewModel>> Get(
            string publicationSlug,
            string releaseSlug)
        {
            return await GetViewModel(
                PublicContentPublicationPath(publicationSlug),
                PublicContentReleasePath(publicationSlug, releaseSlug)
            );
        }

        private async Task<ActionResult<PreReleaseAccessListViewModel>> GetViewModel(
            string publicationPath,
            string releasePath)
        {
            var publicationTask = _fileStorageService.GetDeserialized<CachedPublicationViewModel>(publicationPath);
            var releaseTask = _fileStorageService.GetDeserialized<CachedReleaseViewModel>(releasePath);

            await Task.WhenAll(publicationTask, releaseTask);

            if (releaseTask.Result.IsRight && publicationTask.Result.IsRight)
            {
                return new PreReleaseAccessListViewModel(releaseTask.Result.Right, publicationTask.Result.Right);
            }

            return NotFound();
        }
    }
}
