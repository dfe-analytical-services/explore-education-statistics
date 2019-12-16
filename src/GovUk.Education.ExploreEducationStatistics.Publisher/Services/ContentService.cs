using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
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

        private const string ContainerName = "cache";

        private readonly string _publicStorageConnectionString =
            ConnectionUtils.GetAzureStorageConnectionString("PublicStorage");

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
            await _fileStorageService.UploadFromStreamAsync(_publicStorageConnectionString, ContainerName,
                PublicContentPublicationsTreePath(), null, SerializeObject(tree, _jsonSerializerSettingsCamelCase));
        }

        // TODO: Get this to work
        public async Task UpdateDownloadTree()
        {
            // This is assuming the files have been copied first
            var tree = _downloadService.GetDownloadTree();
            await _fileStorageService.UploadFromStreamAsync(_publicStorageConnectionString, ContainerName,
                PublicContentDownloadTreePath(), null, SerializeObject(tree, _jsonSerializerSettingsCamelCase));
        }

        public async Task UpdateMethodologyTree()
        {
            var tree = _methodologyService.GetTree();
            await _fileStorageService.UploadFromStreamAsync(_publicStorageConnectionString, ContainerName,
                PublicContentMethodologyTreePath(), null, SerializeObject(tree, _jsonSerializerSettingsCamelCase));
        }


        public async Task UpdateMethodologies()
        {
            var methodologies = _methodologyService.Get();
            foreach (var methodology in methodologies)
            {
                var methodologyJson = SerializeObject(methodology, _jsonSerializerSettingsCamelCase);
                await _fileStorageService.UploadFromStreamAsync(_publicStorageConnectionString, ContainerName,
                    PublicContentMethodologyPath(methodology.Slug), null, methodologyJson);
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

                await _fileStorageService.UploadFromStreamAsync(_publicStorageConnectionString, ContainerName,
                    PublicContentPublicationPath(publication.Slug), null, publicationJson);

                await _fileStorageService.UploadFromStreamAsync(_publicStorageConnectionString, ContainerName,
                    PublicContentLatestReleasePath(latestRelease.Publication.Slug), null, latestReleaseJson);

                await _fileStorageService.UploadFromStreamAsync(_publicStorageConnectionString, ContainerName,
                    PublicContentReleasePath(latestRelease.Publication.Slug, latestRelease.Slug), null,
                    latestReleaseJson);

                foreach (var r in publication.Releases)
                {
                    var release = _releaseService.GetRelease(r.Id);
                    var releaseJson = SerializeObject(release, _jsonSerializerSettingsLowerCase);

                    await _fileStorageService.UploadFromStreamAsync(_publicStorageConnectionString, ContainerName,
                        PublicContentReleasePath(publication.Slug, release.Slug), null, releaseJson);
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
            return new JsonSerializerSettings()
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