using System;
using System.Collections.Generic;
using System.IO;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Seed.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Services
{
    public class SeedService : ISeedService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly string _storageConnectionString;
        private readonly IFileStorageService _fileStorageService;
        
        public SeedService(
            ILogger<SeedService> logger,
            IMapper mapper,
            IConfiguration config,
            IFileStorageService fileStorageService)
        {
            _logger = logger;
            _mapper = mapper;
            _storageConnectionString = config.GetConnectionString("CoreStorage");
            _fileStorageService = fileStorageService;
        }

        public void Seed()
        {
            var subjects = SamplePublications.GetSubjects();
            foreach (var subject in subjects)
            {
                _logger.LogInformation($"Processing subject {subject.Id}");
                var file = SamplePublications.SubjectFiles[subject.Id];
                StoreFilesAndSeed(subject.Release, file, subject.Name, subject);
            }
        }
        
        private void StoreFilesAndSeed(Release release, DataCsvFile file, string subjectName, Subject subject)
        {
            var dataFile = CreateFormFile(file.GetCsvLines(), file + ".csv", "file");
            var metaFile = CreateFormFile(file.GetMetaCsvLines(), file + ".meta.csv", "metaFile");

            _logger.LogInformation("Uploading files for \"{subjectName}\"", subjectName);
            var result = _fileStorageService.UploadDataFilesAsync(release.Id, dataFile, metaFile, subjectName, true).Result;
            int rows = FileStorageUtils.CalculateNumberOfRows(dataFile.OpenReadStream());
            Seed(subject.Id,file + ".csv", subject.Release, rows);
        }

        private void Seed(Guid subjectId, string dataFileName, Release release, int rows)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var aQueue = client.GetQueueReference("imports-available");
            aQueue.CreateIfNotExists();

            var pQueue = client.GetQueueReference("imports-pending");

            pQueue.CreateIfNotExists();

            var cloudMessage = BuildCloudMessage(subjectId, dataFileName, release, rows);

            _logger.LogInformation("Adding queue message for file \"{dataFileName}\"", dataFileName);

            pQueue.AddMessage(cloudMessage);
        }

        private CloudQueueMessage BuildCloudMessage(Guid subjectId, string dataFileName, Release release, int rows)
        {
            var importMessageRelease = _mapper.Map<Processor.Model.Release>(release);
            var message = new ImportMessage
            {
                SubjectId = subjectId,
                DataFileName = dataFileName,
                OrigDataFileName = dataFileName,
                Release = importMessageRelease,
                BatchNo = 1,
                Seeding = true
            };

            return new CloudQueueMessage(JsonConvert.SerializeObject(message));
        }

        private static IFormFile CreateFormFile(IEnumerable<string> lines, string fileName, string name)
        {
            var mStream = new MemoryStream();
            var writer = new StreamWriter(mStream);

            foreach (var line in lines)
            {
                writer.WriteLine(line);
                writer.Flush();
            }

            var f = new FormFile(mStream, 0, mStream.Length, name,
                fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/csv"
            };
            return f;
        }
    }
}