#nullable enable
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

            await using var zipBlobFileStream = await _privateBlobStorageService.StreamBlob(PrivateReleaseFiles, path);
            using var archive = new ZipArchive(zipBlobFileStream);

            var dataFileName = import.File.Filename;
            var metaFileName = import.MetaFile.Filename;

            var dataFile = archive.GetEntry(dataFileName);
            var metaFile = archive.GetEntry(metaFileName);

            // @MarkFix extra validation here? Could skip if we assume validation occurred elsewhere

            await using (var stream = dataFile.Open())
            {
                await _privateBlobStorageService.UploadStream(
                    containerName: PrivateReleaseFiles,
                    path: import.File.Path(),
                    stream: stream,
                    contentType: ContentTypes.Csv);
            }

            await using (var stream = metaFile.Open())
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
