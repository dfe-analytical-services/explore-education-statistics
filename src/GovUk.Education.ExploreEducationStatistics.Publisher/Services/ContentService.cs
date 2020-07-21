using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ContentService : IContentService
    {
        private readonly IFastTrackService _fastTrackService;
        private readonly IReleaseService _releaseService;
        private readonly IPublicationService _publicationService;
        private readonly IDownloadService _downloadService;
        private readonly IMethodologyService _methodologyService;
        private readonly IFileStorageService _fileStorageService;

        private readonly JsonSerializerSettings _jsonSerializerSettingsCamelCase =
            GetJsonSerializerSettings(new CamelCaseNamingStrategy());

        public ContentService(IFastTrackService fastTrackService,
            IDownloadService downloadService,
            IFileStorageService fileStorageService,
            IMethodologyService methodologyService,
            IReleaseService releaseService,
            IPublicationService publicationService)
        {
            _fastTrackService = fastTrackService;
            _fileStorageService = fileStorageService;
            _releaseService = releaseService;
            _publicationService = publicationService;
            _downloadService = downloadService;
            _methodologyService = methodologyService;
        }


        public async Task DeletePreviousVersionsContent(IEnumerable<Guid> releaseIds)
        {
            var releases = await _releaseService.GetAmendedReleases(releaseIds);

            foreach (var release in releases)
            {
                await _fastTrackService.DeleteAllFastTracksByRelease(release.PreviousVersionId);

                // Delete content which hasn't been overwritten because the Slug has changed
                if (release.Slug != release.PreviousVersion.Slug)
                {
                    await _fileStorageService.DeletePublicBlob(
                        PublicContentReleasePath(release.Publication.Slug, release.PreviousVersion.Slug));
                }
            }
        }

        /**
         * Intended to be used as a Development / BAU Function to perform a full content refresh
         */
        public async Task UpdateAllContentAsync()
        {
            // Content is not staged when performing a full refresh and there are no additional releases included
            var includedReleaseIds = Enumerable.Empty<Guid>();
            var context = new PublishContext(DateTime.UtcNow, false);

            await DeleteAllContent();
            await UpdateTrees(includedReleaseIds, context);

            var publications = _publicationService.ListPublicationsWithPublishedReleases().ToList();
            var methodologyIds = publications.Where(publication => publication.MethodologyId.HasValue)
                .Select(publication => publication.MethodologyId.Value).Distinct();

            foreach (var publication in publications)
            {
                var releases = publication.Releases.Where(release => release.Live);
                await UpdatePublication(publication, includedReleaseIds, context);
                await UpdateLatestRelease(publication, includedReleaseIds, context);
                foreach (var release in releases)
                {
                    await UpdateFastTracks(release, context);
                    await UpdateRelease(release, context);
                }
            }

            foreach (var methodologyId in methodologyIds)
            {
                await UpdateMethodology(methodologyId, context);
            }
        }

        public async Task UpdateContentAsync(IEnumerable<Guid> releaseIds, PublishContext context)
        {
            await UpdateTrees(releaseIds, context);

            var releases = await _releaseService.GetAsync(releaseIds);
            var publications = releases.Select(release => release.Publication).ToList();
            var methodologyIds = publications.Where(publication => publication.MethodologyId.HasValue)
                .Select(publication => publication.MethodologyId.Value).Distinct();

            foreach (var publication in publications)
            {
                await UpdatePublication(publication, releaseIds, context);
                await UpdateLatestRelease(publication, releaseIds, context);
                foreach (var release in releases)
                {
                    await UpdateFastTracks(release, context);
                    await UpdateRelease(release, context);
                }
            }

            foreach (var methodologyId in methodologyIds)
            {
                await UpdateMethodology(methodologyId, context);
            }
        }

        private async Task DeleteAllContent()
        {
            await _fastTrackService.DeleteAllReleaseFastTracks();
            await _fileStorageService.DeleteAllContentAsyncExcludingStaging();
        }
        
        private async Task UpdateDownloadTreeAsync(IEnumerable<Guid> includedReleaseIds, PublishContext context)
        {
            // This assumes the files have been copied first
            var tree = _downloadService.GetTree(includedReleaseIds);
            await Upload(PublicContentDownloadTreePath, context, tree, _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdateFastTracks(Release release, PublishContext context)
        {
            await _fastTrackService.CreateAllByRelease(release.Id, context);
        }
        
        private async Task UpdateMethodologyTree(IEnumerable<Guid> includedReleaseIds, PublishContext context)
        {
            var tree = _methodologyService.GetTree(includedReleaseIds);
            await Upload(PublicContentMethodologyTreePath, context, tree, _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdatePublicationTree(IEnumerable<Guid> includedReleaseIds, PublishContext context)
        {
            var tree = _publicationService.GetTree(includedReleaseIds);
            await Upload(PublicContentPublicationsTreePath, context, tree, _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdateLatestRelease(Publication publication, IEnumerable<Guid> includedReleaseIds,
            PublishContext context)
        {
            var viewModel = _releaseService.GetLatestReleaseViewModel(publication.Id, includedReleaseIds, context);
            await Upload(prefix => PublicContentLatestReleasePath(publication.Slug, prefix), context, viewModel, _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdateMethodology(Guid methodologyId, PublishContext context)
        {
            var viewModel = await _methodologyService.GetViewModelAsync(methodologyId, context);
            await Upload(prefix => PublicContentMethodologyPath(viewModel.Slug, prefix), context, viewModel, _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdatePublication(Publication publication, IEnumerable<Guid> includedReleaseIds, PublishContext context)
        {
            var viewModel = await _publicationService.GetViewModelAsync(publication.Id, includedReleaseIds);
            await Upload(prefix => PublicContentPublicationPath(publication.Slug, prefix), context, viewModel, _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdateRelease(Release release, PublishContext context)
        {
            var viewModel = _releaseService.GetReleaseViewModel(release.Id, context);
            await Upload(prefix => PublicContentReleasePath(release.Publication.Slug, release.Slug, prefix), context, viewModel, _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdateTrees(IEnumerable<Guid> includedReleaseIds, PublishContext context)
        {
            await UpdateDownloadTreeAsync(includedReleaseIds, context);
            await UpdateMethodologyTree(includedReleaseIds, context);
            await UpdatePublicationTree(includedReleaseIds, context);
        }

        private static JsonSerializerSettings GetJsonSerializerSettings(NamingStrategy namingStrategy)
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = namingStrategy
                },
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        private async Task Upload(Func<string, string> pathFunction, PublishContext context, object value,
            JsonSerializerSettings settings)
        {
            var pathPrefix = context.Staging ? PublicContentStagingPath() : null;
            var blobName = pathFunction.Invoke(pathPrefix);
            await _fileStorageService.UploadAsJson(blobName, value, settings);
        }
    }
}