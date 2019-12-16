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

        public async Task UpdatePublicationsAndReleases()
        {
            var publications = _publicationService.ListPublicationsWithPublishedReleases();

            foreach (var publication in publications)
            {
                var publicationViewModel = BuildPublicationViewModel(publication);
                var latestRelease = _releaseService.GetLatestRelease(publication.Id);

                var publicationJson = SerializeObject(publicationViewModel, _jsonSerializerSettingsLowerCase);
                var latestReleaseJson = SerializeObject(latestRelease, _jsonSerializerSettingsCamelCase);

                await _fileStorageService.UploadFromStreamAsync(PublicContentPublicationPath(publication.Slug),
                    "application/json",
                    publicationJson);

                await _fileStorageService.UploadFromStreamAsync(
                    PublicContentLatestReleasePath(latestRelease.Publication.Slug), "application/json",
                    latestReleaseJson);

                await _fileStorageService.UploadFromStreamAsync(
                    PublicContentReleasePath(latestRelease.Publication.Slug, latestRelease.Slug), "application/json",
                    latestReleaseJson);

                foreach (var r in publication.Releases)
                {
                    var release = _releaseService.GetRelease(r.Id);
                    var releaseJson = SerializeObject(release, _jsonSerializerSettingsLowerCase);

                    await _fileStorageService.UploadFromStreamAsync(
                        PublicContentReleasePath(publication.Slug, release.Slug), "application/json", releaseJson);
                }
            }
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