using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Importer;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class SeedService
    {
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly CsvImporterFactory _csvImporterFactory = new CsvImporterFactory();
        private readonly TidyDataPersisterFactory _tidyDataPersisterFactory;
        private readonly IOptions<SeedConfigurationOptions> _options;

        private const string attributeMetaTableName = "AttributeMeta";
        private const string characteristicMetaTableName = "CharacteristicMeta";

        public SeedService(ILogger<SeedService> logger,
            ApplicationDbContext context,
            TidyDataPersisterFactory tidyDataPersisterFactory,
            IOptions<SeedConfigurationOptions> options)
        {
            _logger = logger;
            _context = context;
            _tidyDataPersisterFactory = tidyDataPersisterFactory;
            _options = options;
        }


        public void DeleteAll()
        {
            _context.Database.ExecuteSqlCommand("delete from AttributeMeta");
            _context.Database.ExecuteSqlCommand("delete from CharacteristicMeta");
            _context.Database.ExecuteSqlCommand("delete from GeographicData");
            _context.Database.ExecuteSqlCommand("delete from CharacteristicDataLa");
            _context.Database.ExecuteSqlCommand("delete from CharacteristicDataNational");
        }

        public void Seed()
        {
            _logger.LogInformation("Seeding");

            foreach (var publication in SamplePublications.Publications.Values)
            {
                Seed(publication);
            }

            _logger.LogInformation("Seeding complete");
        }

        private void Seed(Publication publication)
        {
            _logger.LogInformation("Seeding Publication {Publication}", publication.PublicationId);

            SeedAttributes(publication);
            SeedCharacteristics(publication);

            foreach (var release in publication.Releases)
            {
                SeedRelease(release);
            }
        }

        private void SeedRelease(Release release)
        {
            _logger.LogInformation("Seeding Release for {Publication}, {Release}", release.PublicationId,
                release.Name);

            foreach (var dataCsvFilename in release.Filenames)
            {
                var importer = _csvImporterFactory.Importer(dataCsvFilename);
                var persister = _tidyDataPersisterFactory.Persister(dataCsvFilename);

                if (persister.Count(release.PublicationId) == 0)
                {
                    var data = importer.Data(dataCsvFilename, release.PublicationId, release.ReleaseId,
                        release.ReleaseDate);
                    Persist(data, release);
                }
                else
                {
                    _logger.LogWarning("Not seeding {Release}. Data already exists for publication {Publication}",
                        release.Name, release.PublicationId);
                }
            }
        }
        
        private void SeedAttributes(Publication publication)
        {
            _logger.LogInformation("Seeding {Table}", attributeMetaTableName);

            if (_context.CharacteristicMeta.Count(meta => meta.PublicationId == publication.PublicationId) == 0)
            {
                foreach (var attributeMeta in publication.AttributeMetas)
                {
                    attributeMeta.PublicationId = publication.PublicationId;
                }

                _context.AttributeMeta.AddRange(publication.AttributeMetas);
                _context.SaveChanges();
            }
            else
            {
                _logger.LogWarning("Not seeding {Table}. Table already contains meta for publication {Publication}",
                    attributeMetaTableName, publication.PublicationId);
            }
        }

        private void SeedCharacteristics(Publication publication)
        {
            _logger.LogInformation("Seeding {Table}", characteristicMetaTableName);

            if (_context.CharacteristicMeta.Count(meta => meta.PublicationId == publication.PublicationId) == 0)
            {
                foreach (var characteristicMeta in publication.CharacteristicMetas)
                {
                    characteristicMeta.PublicationId = publication.PublicationId;
                }

                _context.CharacteristicMeta.AddRange(publication.CharacteristicMetas);
                _context.SaveChanges();
            }
            else
            {
                _logger.LogWarning("Not seeding {Table}. Table already contains meta for publication {Publication}",
                    characteristicMetaTableName, publication.PublicationId);
            }
        }

        private void Persist(IEnumerable<ITidyData> tidyData, Release release)
        {
            var index = 1;
            var batches = tidyData.Batch(_options.Value.BatchSize);
            foreach (var batch in batches)
            {
                _logger.LogInformation(
                    "Seeding batch {Index} of {TotalCount} for Publication {Publication}, {Release}", index,
                    batches.Count(), release.PublicationId, release.Name);
                
                _context.Set<ITidyData>().AddRange(batch);
                _context.SaveChanges();
                index++;
            }
        }
    }
}