using System;
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

        public async Task UpdatePublicationTree()
        {
            var tree = _publicationService.GetPublicationsTree();
            await _fileStorageService.UploadFromStreamAsync(PublicContentPublicationsTreePath(), "application/json",
                SerializeObject(tree, _jsonSerializerSettingsCamelCase));
        }

        public async Task UpdateDownloadTree()
        {
            // This is assuming the files have been copied first
            var tree = _downloadService.GetDownloadTree();
            await _fileStorageService.UploadFromStreamAsync(PublicContentDownloadTreePath(), "application/json",
                SerializeObject(tree, _jsonSerializerSettingsCamelCase));
        }

        public async Task UpdateMethodologyTree()
        {
            var tree = _methodologyService.GetTree();
            await _fileStorageService.UploadFromStreamAsync(PublicContentMethodologyTreePath(), "application/json",
                SerializeObject(tree, _jsonSerializerSettingsCamelCase));
        }

        public async Task UpdatePublicationAndRelease(Guid releaseId)
        {
            var release = _releaseService.GetRelease(releaseId);
            var publication = release.Publication;
            await UpdatePublication(publication);
            await UpdateLatestRelease(publication);
            await UpdateRelease(releaseId);
        }

        public async Task UpdatePublicationsAndReleases()
        {
            var publications = _publicationService.ListPublicationsWithPublishedReleases();
            foreach (var publication in publications)
            {
                await UpdatePublication(publication);
                await UpdateLatestRelease(publication);
                foreach (var release in publication.Releases)
                {
                    await UpdateRelease(release.Id);
                }
            }
        }

        public async Task UpdateMethodologies()
        {
            var methodologies = _methodologyService.Get();
            foreach (var methodology in methodologies)
            {
                var methodologyJson = SerializeObject(methodology, _jsonSerializerSettingsCamelCase);
                await _fileStorageService.UploadFromStreamAsync(PublicContentMethodologyPath(methodology.Slug),
                    "application/json",
                    methodologyJson);
            }
        }

        private async Task UpdatePublication(Publication publication)
        {
            var viewModel = BuildPublicationViewModel(publication);
            var json = SerializeObject(viewModel, _jsonSerializerSettingsLowerCase);
            await _fileStorageService.UploadFromStreamAsync(PublicContentPublicationPath(publication.Slug),
                "application/json", json);
        }

        private async Task UpdateLatestRelease(Publication publication)
        {
            var viewModel = _releaseService.GetLatestRelease(publication.Id);
            var json = SerializeObject(viewModel, _jsonSerializerSettingsCamelCase);
            await _fileStorageService.UploadFromStreamAsync(
                PublicContentLatestReleasePath(publication.Slug), "application/json", json);
        }

        private async Task UpdateRelease(Guid releaseId)
        {
            var viewModel = _releaseService.GetRelease(releaseId);
            var json = SerializeObject(viewModel, _jsonSerializerSettingsLowerCase);
            await _fileStorageService.UploadFromStreamAsync(
                PublicContentReleasePath(viewModel.Publication.Slug, viewModel.Slug), "application/json", json);
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

        private static string SerializeObject(object value, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(value, null, settings);
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