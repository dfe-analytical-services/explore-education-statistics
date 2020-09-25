using System;
using System.IO.Compression;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class DataArchiveService : IDataArchiveService
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IFileStorageService _fileStorageService;

        private const string NameKey = "name";

        public DataArchiveService(IBlobStorageService blobStorageService, IFileStorageService fileStorageService)
        {
            _blobStorageService = blobStorageService;
            _fileStorageService = fileStorageService;
        }

        public async Task ExtractDataFiles(Guid releaseId, string zipFileName)
        {
            var path = AdminReleasePath(releaseId, ReleaseFileTypes.DataZip, zipFileName.ToLower());
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
                await _fileStorageService.UploadStream(
                    releaseId: releaseId,
                    stream: stream,
                    fileType: ReleaseFileTypes.Data,
                    fileName: dataFile.Name.ToLower(),
                    contentType: "text/csv",
                    metaValues: GetDataFileMetaValues(
                        name: blob.Name,
                        metaFileName: metadataFile.Name,
                        userName: blob.GetUserName(),
                        numberOfRows: CalculateNumberOfRows(rowStream)
                    )
                );
            }

            await using (var rowStream = metadataFile.Open())
            await using (var stream = metadataFile.Open())
            {
                await _fileStorageService.UploadStream(
                    releaseId: releaseId,
                    stream: stream,
                    fileType: ReleaseFileTypes.Metadata,
                    fileName: metadataFile.Name.ToLower(),
                    contentType: "text/csv",
                    metaValues: GetMetaDataFileMetaValues(
                        dataFileName: dataFile.Name,
                        userName: blob.GetUserName(),
                        numberOfRows: CalculateNumberOfRows(rowStream)
                    )
                );
            }
        }
    }
}