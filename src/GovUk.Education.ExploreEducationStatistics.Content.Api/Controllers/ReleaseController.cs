using System;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ReleaseController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public ReleaseController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpGet("publications/{publicationSlug}/releases/latest")]
        public async Task<ActionResult<ReleaseViewModel>> GetLatestRelease(string publicationSlug)
        {
            return await GetReleaseViewModel(
                PublicContentPublicationPath(publicationSlug),
                PublicContentLatestReleasePath(publicationSlug)
            );
        }

        [HttpGet("publications/{publicationSlug}/releases/latest/summary")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> GetLatestReleaseSummary(string publicationSlug)
        {
            return await GetReleaseSummaryViewModel(
                PublicContentPublicationPath(publicationSlug),
                PublicContentLatestReleasePath(publicationSlug)
            );
        }

        [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}")]
        public async Task<ActionResult<ReleaseViewModel>> GetRelease(string publicationSlug, string releaseSlug)
        {
            return await GetReleaseViewModel(
                PublicContentPublicationPath(publicationSlug),
                PublicContentReleasePath(publicationSlug, releaseSlug)
            );
        }

        [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/summary")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> GetReleaseSummary(string publicationSlug, string releaseSlug)
        {
            return await GetReleaseSummaryViewModel(
                PublicContentPublicationPath(publicationSlug),
                PublicContentReleasePath(publicationSlug, releaseSlug)
            );
        }

        private Task<ActionResult<ReleaseViewModel>> GetReleaseViewModel(
            string publicationPath,
            string releasePath)
        {
            return CreateFromCachedPublicationAndRelease(publicationPath, releasePath, 
                (publication, release) => new ReleaseViewModel(release, publication));
        }
        
        private Task<ActionResult<ReleaseSummaryViewModel>> GetReleaseSummaryViewModel(
            string publicationPath,
            string releasePath)
        {
            return CreateFromCachedPublicationAndRelease(publicationPath, releasePath, 
                (publication, release) => new ReleaseSummaryViewModel(release, publication));
        }
        
        private async Task<ActionResult<T>> CreateFromCachedPublicationAndRelease<T>(
            string publicationPath,
            string releasePath,
            Func<CachedPublicationViewModel, CachedReleaseViewModel, T> func)
        {
            var publicationTask = _fileStorageService.GetDeserialized<CachedPublicationViewModel>(publicationPath);
            var releaseTask = _fileStorageService.GetDeserialized<CachedReleaseViewModel>(releasePath);

            await Task.WhenAll(publicationTask, releaseTask);

            if (releaseTask.Result.IsRight && publicationTask.Result.IsRight)
            {
                return func.Invoke(publicationTask.Result.Right, releaseTask.Result.Right);
            }

            return NotFound();
        }
    }
}
