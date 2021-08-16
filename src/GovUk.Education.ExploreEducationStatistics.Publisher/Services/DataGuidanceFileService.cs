#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class DataGuidanceFileService : IDataGuidanceFileService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IDataGuidanceFileWriter _dataGuidanceFileWriter;
        private readonly IBlobStorageService _blobStorageService;

        public DataGuidanceFileService(
            ContentDbContext contentDbContext,
            IDataGuidanceFileWriter dataGuidanceFileWriter,
            IBlobStorageService blobStorageService)
        {
            _contentDbContext = contentDbContext;
            _dataGuidanceFileWriter = dataGuidanceFileWriter;
            _blobStorageService = blobStorageService;
        }

        public async Task<File> CreateDataGuidanceFile(Guid releaseId)
        {
            var path = Path.GetTempPath() + Guid.NewGuid() + ".txt";

            var release = await _contentDbContext.Releases.FindAsync(releaseId);

            if (release is null)
            {
                throw new ArgumentException($"Could not find release with id: {releaseId}");
            }


            if (release.MetaGuidance.IsNullOrWhitespace())
            {
                throw new InvalidOperationException($"Release {releaseId} must have non-empty data guidance");
            }

            await using var file = System.IO.File.OpenWrite(path);
            await _dataGuidanceFileWriter.WriteToStream(file, release);

            await file.DisposeAsync();

            var releaseFile = new ReleaseFile
            {
                Release = release,
                Name = "Data guidance",
                File = new File
                {
                    Created = DateTime.UtcNow,
                    RootPath = release.Id,
                    Filename = "data-guidance.txt",
                    Type = FileType.DataGuidance
                }
            };

            var created = (await _contentDbContext.ReleaseFiles.AddAsync(releaseFile)).Entity;

            await _blobStorageService.UploadStream(
                containerName: BlobContainers.PrivateReleaseFiles,
                path: created.Path(),
                stream: System.IO.File.OpenRead(path),
                "text/plain"
            );

            await _contentDbContext.SaveChangesAsync();

            return created.File;
        }
    }
}