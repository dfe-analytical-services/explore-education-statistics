using System;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
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

        [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}")]
        public async Task<ActionResult<ReleaseViewModel>> GetRelease(string publicationSlug, string releaseSlug)
        {
            return await GetReleaseViewModel(
                PublicContentPublicationPath(publicationSlug),
                PublicContentReleasePath(publicationSlug, releaseSlug)
            );
        }

        private async Task<ActionResult<ReleaseViewModel>> GetReleaseViewModel(
            string publicationPath,
            string releasePath)
        {
            var publicationTask = _fileStorageService.GetDeserialized<CachedPublicationViewModel>(publicationPath);
            var releaseTask = _fileStorageService.GetDeserialized<CachedReleaseViewModel>(releasePath);

            var continuation = Task.WhenAll(publicationTask, releaseTask);

            try
            {
                continuation.Wait();
            }
            catch (AggregateException)
            {
            }

            if (continuation.Status == TaskStatus.RanToCompletion
                && releaseTask.Result.IsRight
                && publicationTask.Result.IsRight)
            {
                return new ReleaseViewModel(releaseTask.Result.Right, publicationTask.Result.Right);
            }

            return NotFound();
        }
    }
}