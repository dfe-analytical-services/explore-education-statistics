using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Seed.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

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
            _logger.LogInformation("Seeding");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var theme in SamplePublications.Themes)
            {
                foreach (var topic in theme.Topics)
                {
                    foreach (var publication in topic.Publications)
                    {
                        foreach (var release in publication.Releases)
                        {
                            foreach (var subject in release.Subjects)
                            {
                                _logger.LogInformation("Seeding Subject {Subject}", subject.Name);

                                Release r = new Release
                                {
                                    Id = release.Id,
                                    Slug = release.Slug,
                                    Title = release.Title,
                                    ReleaseDate = release.ReleaseDate,
                                    PublicationId = publication.Id,
                                    Publication = new Publication
                                    {
                                        Id = publication.Id,
                                        Slug = publication.Slug,
                                        Title = publication.Title,
                                        TopicId = topic.Id,
                                        Topic = new Topic
                                        {
                                           Id = topic.Id,
                                           Slug = topic.Slug,
                                           Title = topic.Title,
                                           ThemeId = theme.Id,
                                           Theme = new Theme
                                           {
                                              Id = theme.Id,
                                              Slug = theme.Slug,
                                              Title = theme.Title
                                           }
                                        }
                                    },
                                    Subjects = new []
                                    {
                                        new Subject {
                                            Id = subject.Id,
                                            Name = subject.Name,
                                            ReleaseId = release.Id
                                        }
                                    }
                                };

                                var file = SamplePublications.SubjectFiles.GetValueOrDefault(subject.Id);

                                StoreFiles(r, file, subject.Name);
                                Seed(file.ToString() + ".csv", r);
                            }   
                        }
                    } 
                }
            }

            stopWatch.Stop();
            _logger.LogInformation("Seeding completed with duration {duration} ", stopWatch.Elapsed.ToString());
        }

        private void Seed(string dataFileName, Release release)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var pQueue = client.GetQueueReference("imports-pending");
            var aQueue = client.GetQueueReference("imports-available");
            
            pQueue.CreateIfNotExists();
            aQueue.CreateIfNotExists();
            
            var message = BuildMessage(dataFileName, release);
            
            pQueue.AddMessage(message);
        }

        private CloudQueueMessage BuildMessage(string dataFileName, Release release)
        {

            var importMessageRelease = _mapper.Map<Release>(release);
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
            var dataFile = CreateFormFile(file.GetCsvLines(), file.ToString() + ".csv", "file");
            var metaFile = CreateFormFile(file.GetMetaCsvLines(), file.ToString() + ".meta.csv", "metaFile");

            _fileStorageService.UploadFilesAsync(release.Publication.Slug, release.Slug, dataFile, metaFile, subjectName).Wait();
        }

        private IFormFile CreateFormFile(IEnumerable<string> lines, string fileName, string name)
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
    
        
        class ImportMessage
        {
            public string DataFileName { get; set; }
            public Release Release { get; set; }
        
            public int BatchSize { get; set; }
        
            public int BatchNo { get; set; }
        
            public override string ToString()
            {
                return $"{nameof(DataFileName)}: {DataFileName}, {nameof(Release)}: {Release}";
            }
        }
    }
}