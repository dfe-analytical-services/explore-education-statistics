using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IBlobStorageService _blobStorageService;

        public FileStorageService(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        public async Task<Either<ActionResult, T>> GetDeserialized<T>(string path)
        {
            var text = "";

            try
            {
                text = await _blobStorageService.DownloadBlobText(
                    BlobContainerNames.PublicContentContainerName,
                    path
                );
            }
            catch (FileNotFoundException)
            {
                return new NotFoundResult();
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new JsonException(
                    $"Found empty file when trying to deserialize JSON for path: {path}");
            }

            return JsonConvert.DeserializeObject<T>(
                text,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }
            );
        }
    }
}