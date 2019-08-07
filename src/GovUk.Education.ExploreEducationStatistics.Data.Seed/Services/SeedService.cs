using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Seed.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
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

        private readonly ImportMessage[] messages = new ImportMessage[SamplePublications.SubjectFiles.Count - 3];

        private const bool PROCESS_SEQUENTIALLY = true;

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
            _logger.LogInformation("Seeding");

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var count = 0;

            foreach (var subject in SamplePublications.GetSubjects())
            {
                var file = SamplePublications.SubjectFiles[subject.Id];

                _logger.LogInformation("Seeding Subject {Subject}", subject.Name);

                StoreFiles(subject.Release, file, subject.Name);
                Seed(file + ".csv", subject.Release, count++);
            }

            stopWatch.Stop();
            _logger.LogInformation("Seeding completed with duration {duration} ", stopWatch.Elapsed.ToString());
        }

        private void Seed(string dataFileName, Release release, int count)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();

            var aQueue = client.GetQueueReference("imports-available");
            aQueue.CreateIfNotExists();

            if (PROCESS_SEQUENTIALLY)
            {
                var importMessageRelease = _mapper.Map<Processor.Model.Release>(release);
                messages[count] = new ImportMessage
                {
                    DataFileName = dataFileName,
                    Release = importMessageRelease,
                    BatchNo = 1,
                    BatchSize = 1
                };

                if (count == SamplePublications.SubjectFiles.Count - 4)
                {
                    var sQueue = client.GetQueueReference("imports-pending-sequential");
                    sQueue.CreateIfNotExists();
                    sQueue.AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(messages)));
                }
            }
            else
            {
                var pQueue = client.GetQueueReference("imports-pending");

                pQueue.CreateIfNotExists();

                var cloudMessage = BuildCloudMessage(dataFileName, release);

                pQueue.AddMessage(cloudMessage);
            }
        }

        private CloudQueueMessage BuildCloudMessage(string dataFileName, Release release)
        {
            var importMessageRelease = _mapper.Map<Processor.Model.Release>(release);
            var message = new ImportMessage
            {
                DataFileName = dataFileName,
                Release = importMessageRelease,
                BatchNo = 1,
                BatchSize = 1
            };

            return new CloudQueueMessage(JsonConvert.SerializeObject(message));
        }

        private void StoreFiles(Release release, DataCsvFile file, string subjectName)
        {
            var dataFile = CreateFormFile(file.GetCsvLines(), file + ".csv", "file");
            var metaFile = CreateFormFile(file.GetMetaCsvLines(), file + ".meta.csv", "metaFile");

            _fileStorageService
                .UploadFilesAsync(release.Publication.Slug, release.Slug, dataFile, metaFile, subjectName).Wait();
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