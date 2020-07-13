using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class PublishingService : IPublishingService
    {
        private readonly IFastTrackService _fastTrackService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IReleaseService _releaseService;

        public PublishingService(IFastTrackService fastTrackService,
            IFileStorageService fileStorageService,
            IReleaseService releaseService)
        {
            _fastTrackService = fastTrackService;
            _fileStorageService = fileStorageService;
            _releaseService = releaseService;
        }

        public async Task PublishStagedReleaseContentAsync(Guid releaseId)
        {
            await _fileStorageService.MoveStagedContentAsync();
            await _releaseService.SetPublishedDatesAsync(releaseId, DateTime.UtcNow);
        }

        public async Task PublishReleaseFilesAsync(Guid releaseId)
        {
            var release = await _releaseService.GetAsync(releaseId);
            var copyReleaseCommand = new CopyReleaseFilesCommand
            {
                ReleaseId = releaseId,
                PublicationSlug = release.Publication.Slug,
                PublishScheduled = release.PublishScheduled.Value,
                ReleaseSlug = release.Slug,
                ReleaseFileReferences = _releaseService.GetReleaseFileReferences(
                    releaseId, ReleaseFileTypes.Ancillary, ReleaseFileTypes.Chart, ReleaseFileTypes.Data),
                AdditionalDeleteDirectoryPaths = GetAdditionalDeletePaths(release)
            };
            await _fileStorageService.CopyReleaseFilesToPublicContainer(copyReleaseCommand);
        }

        private static List<string> GetAdditionalDeletePaths(Release release)
        {
            var result = new List<string>();
            
            // Slug may have changed for the amendment so also remove the previous contents if it has
            if (release.Slug != release.PreviousVersion.Slug)
            {
                result.Add(GetPreviousVersionReleaseDirectoryPath(release));
            }

            return result;
        }

        private static string GetPreviousVersionReleaseDirectoryPath(Release release)
        {
            return PublicReleaseDirectoryPath(release.Publication.Slug, release.PreviousVersion.Slug);
        }
    }
}