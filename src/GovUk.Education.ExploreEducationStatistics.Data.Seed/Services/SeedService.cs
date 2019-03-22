using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Seed.Models;
using Microsoft.Extensions.Logging;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Seed.Models.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Services
{
    public class SeedService : ISeedService
    {
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly CsvImporterFactory _csvImporterFactory;

        private const string indicatorMetaTableName = "IndicatorMeta";
        private const string characteristicMetaTableName = "CharacteristicMeta";

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

                SeedIndicators();
                SeedCharacteristics();
                _context.SaveChanges();

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

        private void SeedIndicators()
        {
            _logger.LogInformation("Seeding {Table}", indicatorMetaTableName);

            if (!_context.IndicatorMeta.Any())
            {
                _context.IndicatorMeta.AddRange(SamplePublications.IndicatorMetas.Values);
            }
            else
            {
                _logger.LogWarning("Not seeding {Table}. Table already contains values", indicatorMetaTableName);
            }
        }

        private void SeedCharacteristics()
        {
            _logger.LogInformation("Seeding {Table}", characteristicMetaTableName);

            if (!_context.CharacteristicMeta.Any())
            {
                _context.CharacteristicMeta.AddRange(SamplePublications.CharacteristicMetas.Values);
            }
            else
            {
                _logger.LogWarning("Not seeding {Table}. Table already contains values", characteristicMetaTableName);
            }
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

            foreach (var dataSet in release.DataSets)
            {
                SeedDataSet(releaseDb, dataSet);
            }
        }

        private void SeedDataSet(Model.Release release, DataSet dataSet)
        {
            SeedDataSetMeta(release, dataSet);
            _context.SaveChanges();

            var importer = _csvImporterFactory.Importer(dataSet.Filename);
            
            _logger.LogInformation("Importing data from {Filename}.csv", dataSet.Filename);
            importer.Import(GetCsvLines(dataSet.Filename), release);
        }

        private IEnumerable<string> GetCsvLines(DataCsvFilename dataCsvFilename)
        {
            var file = dataCsvFilename + ".csv";
            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Files/" + file));

            _logger.LogInformation("Reading data from: {path}", path);

            return File.ReadAllLines(path);
        }

        private void SeedDataSetMeta(Model.Release releaseDb, DataSet dataSet)
        {
            SeedDataSetIndicators(releaseDb, dataSet);
            SeedDataSetCharacteristics(releaseDb, dataSet);
        }

        private void SeedDataSetIndicators(Model.Release releaseDb, DataSet dataSet)
        {
            _context.AddRange(
                from metaGroup in dataSet.IndicatorMetas
                from indicatorMeta in metaGroup.Meta
                let indicatorMetaDb = LookupIndicatorMeta(indicatorMeta.Name)
                select new ReleaseIndicatorMeta
                {
                    Release = releaseDb,
                    IndicatorMeta = indicatorMetaDb,
                    DataType = dataSet.getDataType().Name,
                    Group = metaGroup.Name
                }
            );
        }

        private void SeedDataSetCharacteristics(Model.Release releaseDb, DataSet dataSet)
        {
            _context.AddRange(dataSet.CharacteristicMetas
                .Select(characteristicMeta => LookupCharacteristicMeta(characteristicMeta.Name))
                .Select(characteristicMetaDb =>
                    new ReleaseCharacteristicMeta
                    {
                        Release = releaseDb,
                        CharacteristicMeta = characteristicMetaDb,
                        DataType = dataSet.getDataType().Name
                    })
                .ToList());
        }

        private Model.Release CreateRelease(Release release)
        {
            var releaseDb = _context.Release.Add(new Model.Release(release.ReleaseDate, release.PublicationId)).Entity;
            _context.SaveChanges();
            return releaseDb;
        }

        private IndicatorMeta LookupIndicatorMeta(string name)
        {
            return _context.IndicatorMeta.FirstOrDefault(meta => meta.Name == name);
        }

        private CharacteristicMeta LookupCharacteristicMeta(string name)
        {
            return _context.CharacteristicMeta.FirstOrDefault(meta => meta.Name == name);
        }
    }
}