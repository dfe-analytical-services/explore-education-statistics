using System.IO.Compression;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class DataArchiveService : IDataArchiveService
    {
        private readonly IBlobStorageService _blobStorageService;

        public DataArchiveService(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        public async Task ExtractDataFiles(File zipFile)
        {
            var path = zipFile.Path();
            var releaseId = zipFile.ReleaseId;
            var blob = await _blobStorageService.GetBlob(PrivateFilesContainerName, path);

            await using var zipBlobFileStream = await _blobStorageService.StreamBlob(PrivateFilesContainerName, path);
            using var archive = new ZipArchive(zipBlobFileStream);

            var file1 = archive.Entries[0];
            var file2 = archive.Entries[1];
            var dataFile = file1.Name.Contains(".meta.") ? file2 : file1;
            var metadataFile = file1.Name.Contains(".meta.") ? file1 : file2;

            await using (var rowStream = dataFile.Open())
            await using (var stream = dataFile.Open())
            {
                await _blobStorageService.UploadStream(
                    containerName: PrivateFilesContainerName,
                    path: AdminReleasePath(releaseId, FileType.Data, dataFile.Name.ToLower()),
                    stream: stream,
                    contentType: "text/csv",
                    options: new IBlobStorageService.UploadStreamOptions
                    {
                        MetaValues = GetDataFileMetaValues(
                            name: blob.Name,
                            metaFileName: metadataFile.Name,
                            userName: blob.GetUserName(),
                            numberOfRows: CalculateNumberOfRows(rowStream)
                        )
                    });
            }

            await using (var stream = metadataFile.Open())
            {
                await _blobStorageService.UploadStream(
                    containerName: PrivateFilesContainerName,
                    path: AdminReleasePath(releaseId, Metadata, metadataFile.Name.ToLower()),
                    stream: stream,
                    contentType: "text/csv",
                    options: new IBlobStorageService.UploadStreamOptions
                    {
                        MetaValues = GetMetaDataFileMetaValues(
                            dataFileName: dataFile.Name
                        )
                    });
            }
        }
    }
}