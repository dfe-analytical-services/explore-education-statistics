using System;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ReleaseController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorageService;

        public ReleaseController(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
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
            var publicationTask = Task.Run(
                async () =>
                {
                    var text = await _blobStorageService.DownloadBlobText(
                        PublicContentContainerName,
                        publicationPath
                    );
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        throw new ArgumentException();
                    }

                    return JsonConvert.DeserializeObject<CachedPublicationViewModel>(
                        text,
                        new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        }
                    );
                }
            );

            var releaseTask = Task.Run(
                async () =>
                {
                    var text = await _blobStorageService.DownloadBlobText(
                        PublicContentContainerName,
                        releasePath
                    );
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        throw new ArgumentException();
                    }

                    return JsonConvert.DeserializeObject<CachedReleaseViewModel>(
                        text,
                        new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        }
                    );
                }
            );

            var continuation = Task.WhenAll(publicationTask, releaseTask);

            try
            {
                continuation.Wait();
            }
            catch (AggregateException)
            {
            }

            if (continuation.Status == TaskStatus.RanToCompletion)
            {
                return new ReleaseViewModel(releaseTask.Result, publicationTask.Result);
            }

            return NotFound();
        }
    }
}