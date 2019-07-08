using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
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
                // TODO DFE-897 DFE-754 Remove restriction on large FE data file
                if (!file.Equals(DataCsvFile.clean_data_fe))
                {
                    _logger.LogInformation("Seeding Subject {Subject}", subject.Name);

                    var lines = file.GetCsvLines();
                    var metaLines = file.GetMetaCsvLines();

                    _importerService.Import(lines, metaLines, subject);
                }
            }
        }
    }
}