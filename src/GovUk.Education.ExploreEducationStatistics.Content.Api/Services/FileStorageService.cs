using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
            try
            {
                return await _blobStorageService.GetDeserializedJson<T>(
                    BlobContainers.PublicContentContainerName,
                    path
                );
            }
            catch (FileNotFoundException)
            {
                return new NotFoundResult();
            }
        }
    }
}
