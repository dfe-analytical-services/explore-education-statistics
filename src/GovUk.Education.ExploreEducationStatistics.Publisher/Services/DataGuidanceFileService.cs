#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
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
            await using var stream = await _dataGuidanceFileWriter.WriteFile(releaseId, path);

            var releaseFile = new ReleaseFile
            {
                ReleaseId = releaseId,
                Name = "Data guidance",
                File = new File
                {
                    Created = DateTime.UtcNow,
                    RootPath = releaseId,
                    Filename = "data-guidance.txt",
                    Type = FileType.DataGuidance
                }
            };

            var created = (await _contentDbContext.ReleaseFiles.AddAsync(releaseFile)).Entity;

            await _blobStorageService.UploadStream(
                containerName: BlobContainers.PrivateReleaseFiles,
                path: created.Path(),
                stream: stream,
                "text/plain"
            );

            await _contentDbContext.SaveChangesAsync();

            return created.File;
        }
    }
}