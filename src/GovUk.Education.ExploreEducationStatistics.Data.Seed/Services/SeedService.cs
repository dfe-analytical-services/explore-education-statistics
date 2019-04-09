using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Seed.Models;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Services
{
    public class SeedService : ISeedService
    {
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly CsvImporterFactory _csvImporterFactory;

        public SeedService(ILogger<SeedService> logger,
            ApplicationDbContext context,
            CsvImporterFactory csvImporterFactory)
        {
            _logger = logger;
            _context = context;
            _csvImporterFactory = csvImporterFactory;
        }

        public void Seed()
        {
            _logger.LogInformation("Seeding");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = false;

                foreach (var publication in SamplePublications.Publications.Values)
                {
                    Seed(publication);
                }
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            stopWatch.Stop();
            _logger.LogInformation("Seeding completed with duration {duration} ", stopWatch.Elapsed.ToString());
        }

        private void Seed(Publication publication)
        {
            _logger.LogInformation("Seeding Publication {Publication}", publication.PublicationId);

            foreach (var release in publication.Releases)
            {
                SeedRelease(release);
            }
        }

        private void SeedRelease(Release release)
        {
            _logger.LogInformation("Seeding Release for {Publication}, {Release}", release.PublicationId,
                release.Name);

            var releaseDb = CreateRelease(release);

            foreach (var subject in release.Subjects)
            {
                SeedSubject(releaseDb, subject);
            }
        }

        private void SeedSubject(Model.Release release, Subject subject)
        {
            _logger.LogInformation("Seeding Subject for {Publication}, {Subject}", release.PublicationId,
                subject.Name);

            var subjectDb = CreateSubject(release, subject);

            SeedMetaData(subjectDb, subject);
            _context.SaveChanges();

            var importer = _csvImporterFactory.Importer(subject.GetImportFileType());

            var lines = GetCsvLines(subject.Filename);
            importer.Import(lines, subjectDb);
        }

        private IEnumerable<string> GetCsvLines(DataCsvFilename dataCsvFilename)
        {
            var file = dataCsvFilename + ".csv";
            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Files/" + file));

            _logger.LogInformation("Reading data from: {path}", path);

            return File.ReadAllLines(path);
        }

        private void SeedMetaData(Model.Subject subjectDb, Subject subject)
        {
            SeedMetaDataIndicators(subjectDb, subject);
            SeedMetaDataCharacteristics(subjectDb, subject);
        }

        private void SeedMetaDataIndicators(Model.Subject subjectDb, Subject subject)
        {
            _context.IndicatorMeta.AddRange(subject.IndicatorMetas.SelectMany(group =>
            {
                return group.Meta.Select(meta => new IndicatorMeta
                {
                    Name = meta.Name,
                    Group = group.Name,
                    Label = meta.Label,
                    Unit = meta.Unit,
                    KeyIndicator = meta.KeyIndicator,
                    Subject = subjectDb
                });
            }));
        }

        private void SeedMetaDataCharacteristics(Model.Subject subjectDb, Subject subject)
        {
            _context.CharacteristicMeta.AddRange(subject.CharacteristicMetas.Select(meta => new CharacteristicMeta
            {
                Name = meta.Name,
                Group = meta.Group,
                Label = meta.Label,
                Subject = subjectDb
            }));
        }

        private Model.Release CreateRelease(Release release)
        {
            var releaseDb = _context.Release.Add(new Model.Release(release.ReleaseDate, release.PublicationId)).Entity;
            _context.SaveChanges();
            return releaseDb;
        }

        private Model.Subject CreateSubject(Model.Release release, Subject subject)
        {
            var subjectDb = _context.Subject.Add(new Model.Subject(subject.Name, release)).Entity;
            _context.SaveChanges();
            return subjectDb;
        }
    }
}