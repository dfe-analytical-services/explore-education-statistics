using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
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

        private readonly JsonSerializerSettings _jsonSerializerSettingsLowerCase =
            GetJsonSerializerSettings(new LowerCaseNamingStrategy());

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

        public async Task UpdateAllContentAsync()
        {
            await _fileStorageService.DeleteAllContentAsync();
            await UpdateTreesAsync();

            var publications = _publicationService.ListPublicationsWithPublishedReleases().ToList();
            var methodologyIds = publications.Where(publication => publication.MethodologyId.HasValue)
                .Select(publication => publication.MethodologyId.Value).Distinct();

            foreach (var publication in publications)
            {
                await UpdatePublicationAsync(publication);
                await UpdateLatestReleaseAsync(publication);
                foreach (var release in publication.Releases)
                {
                    await UpdateReleaseAsync(release.Id);
                }
            }

            foreach (var methodologyId in methodologyIds)
            {
                await UpdateMethodologyAsync(methodologyId);
            }
        }

        public async Task UpdateContentAsync(Guid releaseId)
        {
            await UpdateTreesAsync();

            var release = _releaseService.GetRelease(releaseId);
            var publication = release.Publication;
            await UpdatePublicationAsync(publication);
            await UpdateLatestReleaseAsync(publication);
            await UpdateReleaseAsync(releaseId);
            if (publication.MethodologyId.HasValue)
            {
                await UpdateMethodologyAsync(publication.MethodologyId.Value);
            }
        }

        private async Task UpdateDownloadTreeAsync()
        {
            // This is assuming the files have been copied first
            var tree = _downloadService.GetDownloadTree();
            await UploadAsync(PublicContentDownloadTreePath(), tree, _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdateMethodologyTreeAsync()
        {
            var tree = _methodologyService.GetTree();
            await UploadAsync(PublicContentMethodologyTreePath(), tree, _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdatePublicationTreeAsync()
        {
            var tree = _publicationService.GetPublicationsTree();
            await UploadAsync(PublicContentPublicationsTreePath(), tree, _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdateLatestReleaseAsync(Publication publication)
        {
            var viewModel = _releaseService.GetLatestRelease(publication.Id);
            await UploadAsync(PublicContentLatestReleasePath(publication.Slug), viewModel,
                _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdateMethodologyAsync(Guid methodologyId)
        {
            var methodology = await _methodologyService.GetAsync(methodologyId);
            await UploadAsync(PublicContentMethodologyPath(methodology.Slug), methodology,
                _jsonSerializerSettingsCamelCase);
        }

        private async Task UpdatePublicationAsync(Publication publication)
        {
            var viewModel = BuildPublicationViewModel(publication);
            await UploadAsync(PublicContentPublicationPath(publication.Slug), viewModel,
                _jsonSerializerSettingsLowerCase);
        }

        private async Task UpdateReleaseAsync(Guid releaseId)
        {
            var viewModel = _releaseService.GetRelease(releaseId);
            await UploadAsync(PublicContentReleasePath(viewModel.Publication.Slug, viewModel.Slug), viewModel,
                _jsonSerializerSettingsLowerCase);
        }

        private async Task UpdateTreesAsync()
        {
            await UpdateDownloadTreeAsync();
            await UpdateMethodologyTreeAsync();
            await UpdatePublicationTreeAsync();
        }

        private static PublicationViewModel BuildPublicationViewModel(Publication publication)
        {
            return new PublicationViewModel
            {
                Id = publication.Id,
                Title = publication.Title
            };
        }

        private static JsonSerializerSettings GetJsonSerializerSettings(NamingStrategy namingStrategy)
        {
            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = namingStrategy
                }
            };
        }

        private async Task UploadAsync(string blobName, object value, JsonSerializerSettings settings)
        {
            await _fileStorageService.UploadFromStreamAsync(blobName, MediaTypeNames.Application.Json,
                JsonConvert.SerializeObject(value, null, settings));
        }
    }

    internal class LowerCaseNamingStrategy : CamelCaseNamingStrategy
    {
        protected override string ResolvePropertyName(string name)
        {
            return name.ToLower();
        }
    }
}