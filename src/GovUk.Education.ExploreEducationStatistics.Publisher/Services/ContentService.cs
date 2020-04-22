using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ContentService : IContentService
    {
        private readonly IReleaseService _releaseService;
        private readonly IPublicationService _publicationService;
        private readonly IDownloadService _downloadService;
        private readonly IMethodologyService _methodologyService;
        private readonly IFileStorageService _fileStorageService;

        private readonly JsonSerializerSettings _jsonSerializerSettingsCamelCase =
            GetJsonSerializerSettings(new CamelCaseNamingStrategy());

        public ContentService(IDownloadService downloadService,
            IFileStorageService fileStorageService,
            IMethodologyService methodologyService,
            IReleaseService releaseService,
            IPublicationService publicationService)
        {
            _fileStorageService = fileStorageService;
            _releaseService = releaseService;
            _publicationService = publicationService;
            _downloadService = downloadService;
            _methodologyService = methodologyService;
        }

        /**
         * Intended to be used as a Development / BAU Function to perform a full content refresh
         */
        public async Task UpdateAllContentAsync()
        {
            // Content is not staged when performing a full refresh and there are no additional releases included
            var includedReleaseIds = Enumerable.Empty<Guid>();
            const bool staging = false;

            await _fileStorageService.DeleteAllContentAsyncExcludingStaging();
            await UpdateTreesAsync(includedReleaseIds, staging);

            var publications = _publicationService.ListPublicationsWithPublishedReleases().ToList();
            var methodologyIds = publications.Where(publication => publication.MethodologyId.HasValue)
                .Select(publication => publication.MethodologyId.Value).Distinct();

            foreach (var publication in publications)
            {
                var releases = publication.Releases.Where(release => release.Live);
                await UpdatePublicationAsync(publication, includedReleaseIds, staging);
                await UpdateLatestReleaseAsync(publication, includedReleaseIds, staging);
                foreach (var release in releases)
                {
                    await UpdateReleaseAsync(release, staging);
                }
            }

            foreach (var methodologyId in methodologyIds)
            {
                await UpdateMethodologyAsync(methodologyId, staging);
            }
        }

        public async Task UpdateContentAsync(IEnumerable<Guid> releaseIds, bool staging = true)
        {
            await UpdateTreesAsync(releaseIds, staging);

            var releases = await _releaseService.GetAsync(releaseIds);
            var publications = releases.Select(release => release.Publication).ToList();
            var methodologyIds = publications.Where(publication => publication.MethodologyId.HasValue)
                .Select(publication => publication.MethodologyId.Value).Distinct();

            foreach (var publication in publications)
            {
                await UpdatePublicationAsync(publication, releaseIds, staging);
                await UpdateLatestReleaseAsync(publication, releaseIds, staging);
                foreach (var release in releases)
                {
                    await UpdateReleaseAsync(release, staging);
                }
            }

            foreach (var methodologyId in methodologyIds)
            {
                await UpdateMethodologyAsync(methodologyId, staging);
            }
        }

        private async Task UpdateDownloadTreeAsync(IEnumerable<Guid> includedReleaseIds, bool staging)
        {
            // This assumes the files have been copied first
            var tree = _downloadService.GetTree(includedReleaseIds);
            await UploadAsync(PublicContentDownloadTreePath, staging, tree, _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdateMethodologyTreeAsync(IEnumerable<Guid> includedReleaseIds, bool staging)
        {
            var tree = _methodologyService.GetTree(includedReleaseIds);
            await UploadAsync(PublicContentMethodologyTreePath, staging, tree, _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdatePublicationTreeAsync(IEnumerable<Guid> includedReleaseIds, bool staging)
        {
            var tree = _publicationService.GetTree(includedReleaseIds);
            await UploadAsync(PublicContentPublicationsTreePath, staging, tree, _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdateLatestReleaseAsync(Publication publication, IEnumerable<Guid> includedReleaseIds,
            bool staging = true)
        {
            var viewModel = _releaseService.GetLatestReleaseViewModel(publication.Id, includedReleaseIds);
            await UploadAsync(prefix => PublicContentLatestReleasePath(publication.Slug, prefix), staging, viewModel,
                _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdateMethodologyAsync(Guid methodologyId, bool staging)
        {
            var viewModel = await _methodologyService.GetViewModelAsync(methodologyId);
            await UploadAsync(prefix => PublicContentMethodologyPath(viewModel.Slug, prefix), staging, viewModel,
                _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdatePublicationAsync(Publication publication, IEnumerable<Guid> includedReleaseIds,
            bool staging)
        {
            var viewModel = await _publicationService.GetViewModelAsync(publication.Id, includedReleaseIds);
            await UploadAsync(prefix => PublicContentPublicationPath(publication.Slug, prefix), staging, viewModel,
                _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdateReleaseAsync(Release release, bool staging)
        {
            var viewModel = _releaseService.GetReleaseViewModel(release.Id);
            await UploadAsync(prefix => PublicContentReleasePath(release.Publication.Slug, release.Slug, prefix),
                staging, viewModel, _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdateTreesAsync(IEnumerable<Guid> includedReleaseIds, bool staging)
        {
            await UpdateDownloadTreeAsync(includedReleaseIds, staging);
            await UpdateMethodologyTreeAsync(includedReleaseIds, staging);
            await UpdatePublicationTreeAsync(includedReleaseIds, staging);
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

        private async Task UploadAsync(Func<string, string> pathFunction, bool staging, object value,
            JsonSerializerSettings settings)
        {
            var pathPrefix = staging ? PublicContentStagingPath() : null;
            var blobName = pathFunction.Invoke(pathPrefix);
            var json = JsonConvert.SerializeObject(value, null, settings);
            await _fileStorageService.UploadFromStreamAsync(blobName, MediaTypeNames.Application.Json, json);
        }
    }
}