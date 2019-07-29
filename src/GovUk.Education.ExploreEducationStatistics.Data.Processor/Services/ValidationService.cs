using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Publication = GovUk.Education.ExploreEducationStatistics.Data.Model.Publication;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;
using Theme = GovUk.Education.ExploreEducationStatistics.Data.Model.Theme;
using Topic = GovUk.Education.ExploreEducationStatistics.Data.Model.Topic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ValidationService : IValidationService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ValidationService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImporterService _importerService;
        
        public ValidationService(
            IFileStorageService fileStorageService,
            ILogger<ValidationService> logger,
            ApplicationDbContext context,
            IMapper mapper,
            IImporterService importerService)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _importerService = importerService;
        }

        public void Validate(ICollector<ImportMessage> collector, ImportMessage message)
        {
            
            _logger.LogInformation("Validating file size");
            var subjectData = _fileStorageService.GetSubjectData(message).Result;

            CreateUpdateRelease(subjectData, message);
            
            var batchCount = 1;
            var lines = subjectData.GetCsvLines();

            if (lines.Count() > 1001)
            {
                _logger.LogInformation("Splitting file as > 10001 lines");

                List<IFormFile> files = SplitDataFile(message, lines);

                // Upload the split data files & send message to process to imports-available queue
                foreach (var f in files)
                {
                    var result = _fileStorageService.UploadFilesAsync(message.Release.Publication.Slug,
                        message.Release.Slug,
                        f, BlobUtils.GetMetaFileName(subjectData.DataBlob), BlobUtils.GetName(subjectData.DataBlob)).Result;

                    var iMessage = new ImportMessage
                    {
                        DataFileName = f.FileName,
                        Release = message.Release,
                        BatchNo = batchCount,
                        BatchSize = files.Count
                    };

                    batchCount++;
                    collector.Add(iMessage);
                }
            }
            // Else perform any additional validation & pass on file to message queue for import
            else
            {
                collector.Add(message); 
            }
        }

        private void CreateUpdateRelease(SubjectData subjectData, ImportMessage message)
        {
            var release = CreateOrUpdateRelease(message);
            var subject = CreateOrUpdateSubject(subjectData.Name, release);
            var exists = subject.Id > 0; // TODO might need to check if have Observations instead

            _context.SaveChanges();

            _logger.LogInformation("Existing subject : " + exists);

            _logger.LogInformation("Importing meta lines from {0}", 
                BlobUtils.GetMetaFileName(subjectData.DataBlob));
        
            _importerService.ImportMeta(subjectData.GetMetaLines().ToList(), subject, exists);
        }

        private Subject CreateOrUpdateSubject(string name, Release release)
        {
            var subject = _context.Subject
                .FirstOrDefault(s => s.Name.Equals(name) && s.ReleaseId == release.Id);

            if (subject == null)
            {
                subject = _context.Subject.Add(new Subject(name, release)).Entity;
            }

            return subject;
        }
        
        private List<IFormFile> SplitDataFile(ImportMessage message, IEnumerable<string> csvLines)
        {
            var files = new List<IFormFile>();    
            var header = csvLines.First();
            var batches = csvLines.Skip(1).Batch(1000);
            var index = 1;
            
            foreach (var batch in batches)
            {
                var lines = batch.ToList();
                var mStream = new MemoryStream();
                var writer = new StreamWriter(mStream);
                writer.Flush();
                
                // Insert the header at the beginning of each file/batch
                writer.WriteLine(header);
                
                foreach (var line in lines)
                {
                    writer.WriteLine(line);
                    writer.Flush();
                }
                
                var f = new FormFile(mStream, 0, mStream.Length, message.DataFileName,
                    message.DataFileName + "_" + String.Format("{0:000000}", index)) 
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "text/csv"
                };
                files.Add(f);
                index++;
            }
            return files;
        }
        
        private Release CreateOrUpdateRelease(ImportMessage message)
        {
            var release = _context.Release
                .Include(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefault(r => r.Id.Equals(message.Release.Id));

            if (release == null)
            {
                release = new Release
                {
                    Id = message.Release.Id,
                    Title = message.Release.Title,
                    Slug = message.Release.Slug,
                    Publication = CreateOrUpdatePublication(message)
                };
                return _context.Release.Add(release).Entity;
            }

            release = _mapper.Map(message.Release, release);
            return _context.Release.Update(release).Entity;
        }

        private Publication CreateOrUpdatePublication(ImportMessage message)
        {
            var publication = _context.Publication
                .Include(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefault(p => p.Id.Equals(message.Release.Publication.Id));

            if (publication == null)
            {
                publication = new Publication
                {
                    Id = message.Release.Publication.Id,
                    Title = message.Release.Publication.Title,
                    Slug = message.Release.Publication.Slug,
                    Topic = CreateOrUpdateTopic(message)
                };
                return _context.Publication.Add(publication).Entity;
            }

            publication = _mapper.Map(message.Release.Publication, publication);
            return _context.Publication.Update(publication).Entity;
        }
        
        private Topic CreateOrUpdateTopic(ImportMessage message)
        {
            var topic = _context.Topic
                .Include(p => p.Theme)
                .FirstOrDefault(t => t.Id.Equals(message.Release.Publication.Topic.Id));

            if (topic == null)
            {
                topic = new Topic
                {
                    Id = message.Release.Publication.Id,
                    Title = message.Release.Publication.Title,
                    Slug = message.Release.Publication.Slug,
                    Theme = CreateOrUpdateTheme(message)
                };
                return _context.Topic.Add(topic).Entity;
            }

            topic = _mapper.Map(message.Release.Publication.Topic, topic);
            return _context.Topic.Update(topic).Entity;
        }

        private Theme CreateOrUpdateTheme(ImportMessage message)
        {
            var theme = _context.Theme
                .FirstOrDefault(t => t.Id.Equals(message.Release.Publication.Topic.Theme.Id));

            if (theme == null)
            {
                theme = _mapper.Map<Theme>(message.Release.Publication.Topic.Theme);
                return _context.Theme.Add(theme).Entity;
            }

            theme = _mapper.Map(message.Release.Publication.Topic.Theme, theme);
            return _context.Theme.Update(theme).Entity;
        }
    }
}