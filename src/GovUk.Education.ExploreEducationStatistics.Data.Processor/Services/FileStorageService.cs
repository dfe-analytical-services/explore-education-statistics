using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;

        private const string ContainerName = "releases";

        public FileStorageService(IConfiguration config)
        {
            _storageConnectionString = config.GetConnectionString("CoreStorage");
        }

        public async Task<SubjectData> GetSubjectData(ImportMessage importMessage)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(ContainerName);

            var publication = importMessage.Release.Publication.Slug;
            var release = importMessage.Release.Slug;

            var dataBlob = blobContainer.GetBlockBlobReference(
                $"{publication}/{release}/{importMessage.DataFileName}");

            await dataBlob.FetchAttributesAsync();

            var metaBlob = blobContainer.GetBlockBlobReference(
                $"{publication}/{release}/{GetMetaFileName(dataBlob)}");

            return new SubjectData(dataBlob, metaBlob, GetName(dataBlob));
        }

        private static string GetName(CloudBlob blob)
        {
            return blob.Metadata["name"];
        }

        private static string GetMetaFileName(CloudBlob blob)
        {
            return blob.Metadata["metafile"];
        }
    }
}