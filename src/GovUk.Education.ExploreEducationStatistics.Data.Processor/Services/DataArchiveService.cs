#nullable enable
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class DataArchiveService : IDataArchiveService
    {
        private readonly IPrivateBlobStorageService _privateBlobStorageService;

        public DataArchiveService(IPrivateBlobStorageService privateBlobStorageService)
        {
            _privateBlobStorageService = privateBlobStorageService;
        }

        public async Task ExtractDataFiles(DataImport import)
        {
            var path = import.ZipFile!.Path();

            await using var zipBlobFileStream = await _privateBlobStorageService
                .StreamBlob(PrivateReleaseFiles, path);
            using var archive = new ZipArchive(zipBlobFileStream);

            var dataFile = archive.GetEntry(import.File.Filename) ??
                           throw new FileNotFoundException($"Data file {import.File.Filename} not found in archive");
            var metaFile = archive.GetEntry(import.MetaFile.Filename) ??
                           throw new FileNotFoundException($"Meta file {import.MetaFile.Filename} not found in archive");

            await using (var stream = dataFile.Open()) // we should have validated file's existence previously
            {
                await _privateBlobStorageService.UploadStream(
                    containerName: PrivateReleaseFiles,
                    path: import.File.Path(),
                    stream: stream,
                    contentType: ContentTypes.Csv);
            }

            await using (var stream = metaFile.Open()) // we should have validated file's existence previously
            {
                await _privateBlobStorageService.UploadStream(
                    containerName: PrivateReleaseFiles,
                    path: import.MetaFile.Path(),
                    stream: stream,
                    contentType: ContentTypes.Csv);
            }
        }
    }
}
