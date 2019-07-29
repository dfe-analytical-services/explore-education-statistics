using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Seed.Extensions;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Services
{
    public class SeedService : ISeedService
    {
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly IImporterService _importerService;

        public SeedService(ILogger<SeedService> logger,
            ApplicationDbContext context,
            IImporterService importerService)
        {
            _logger = logger;
            _context = context;
            _importerService = importerService;
        }

        public void Seed()
        {
            _logger.LogInformation("Seeding");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = false;

                foreach (var theme in SamplePublications.Themes)
                {
                    _logger.LogInformation("Updating Theme {Theme}", theme.Title);
                    var updated = _context.Theme.Update(theme).Entity;
                    _context.SaveChanges();

                    var subjects = updated.Topics
                        .SelectMany(topic => topic.Publications)
                        .SelectMany(publication => publication.Releases)
                        .SelectMany(release => release.Subjects);

                    Seed(subjects);
                }
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            stopWatch.Stop();
            _logger.LogInformation("Seeding completed with duration {duration} ", stopWatch.Elapsed.ToString());
        }

        private void Seed(IEnumerable<Subject> subjects)
        {
            foreach (var subject in subjects)
            {
                var file = SamplePublications.SubjectFiles.GetValueOrDefault(subject.Id);

                if (file.Equals(DataCsvFile.clean_data_fe)) {
                    _logger.LogInformation("Seeding Subject {Subject}", subject.Name);

                    var csvLines = file.GetCsvLines();
                    var subjectMeta = _importerService.ImportMeta(file.GetMetaCsvLines().ToList(), subject, false);
                    var batches = csvLines.Skip(1).Batch(10000);
                    var index = 0;
                    
                    foreach (var batch in batches)
                    {
                        var lines = batch.ToList();
                        index++;
                        // Insert the header at the beginning of each file/batch
                        lines.Insert(0, csvLines.First());
                        _logger.LogInformation("Processing batch {0} of {1} batches,", index, batches.Count());
                        _importerService.ImportObservations(lines, subject, subjectMeta);
                    }
                }
            }
        }
    }
}