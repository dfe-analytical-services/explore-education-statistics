using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IBlobStorageService _blobStorageService;

        public FileStorageService(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        public async Task<SubjectData> GetSubjectData(ImportMessage importMessage)
        {
            var releaseId = importMessage.Release.Id.ToString();

            var dataBlob = await _blobStorageService.GetBlob(
                PrivateFilesContainerName,
                AdminReleasePath(releaseId, ReleaseFileTypes.Data, importMessage.DataFileName)
            );

            var metaBlob = await _blobStorageService.GetBlob(
                PrivateFilesContainerName,
                AdminReleasePath(releaseId, ReleaseFileTypes.Metadata, dataBlob.GetMetaFileName())
            );

            return new SubjectData(dataBlob, metaBlob);
        }

        public async Task UploadStream(
            Guid releaseId,
            ReleaseFileTypes fileType,
            string fileName,
            Stream stream,
            string contentType,
            IDictionary<string, string> metaValues)
        {
            await _blobStorageService.UploadStream(
                containerName: PrivateFilesContainerName,
                path: AdminReleasePath(releaseId, fileType, fileName),
                stream: stream,
                contentType: contentType,
                options: new IBlobStorageService.UploadStreamOptions
                {
                    MetaValues = metaValues
                }
            );
        }

        public async Task<Stream> StreamFile(Guid releaseId, ReleaseFileTypes fileType, string fileName)
        {
            return await _blobStorageService.StreamBlob(
                PrivateFilesContainerName,
                AdminReleasePath(releaseId, fileType, fileName)
            );
        }

        public async Task<Stream> StreamBlob(BlobInfo blob)
        {
            return await _blobStorageService.StreamBlob(PrivateFilesContainerName, blob.Path);
        }

        public async Task DeleteBatchFile(string releaseId, string dataFileName)
        {
            await _blobStorageService.DeleteBlob(
                PrivateFilesContainerName,
                AdminReleaseDirectoryPath(releaseId, ReleaseFileTypes.Data) + dataFileName
            );
        }

        public async Task<int> GetNumBatchesRemaining(string releaseId, string origDataFileName)
        {
            return (await GetBatchesRemaining(releaseId, origDataFileName)).Count();
        }

        public async Task<IEnumerable<BlobInfo>> GetBatchesRemaining(string releaseId, string origDataFileName)
        {
            var blobs = await _blobStorageService.ListBlobs(
                PrivateFilesContainerName,
                AdminReleaseDirectoryPath(releaseId, ReleaseFileTypes.Data)
            );

            return blobs
                .Where(blob => IsBatchedFile(blob, releaseId) && blob.Path.Contains(origDataFileName));
        }

        private static bool IsBatchedFile(BlobInfo blob, string releaseId)
        {
            return blob.Path.StartsWith(AdminReleaseBatchesDirectoryPath(releaseId));
        }
    }
}