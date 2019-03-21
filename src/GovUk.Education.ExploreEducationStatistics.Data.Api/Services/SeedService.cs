using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EFCore.BulkExtensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Importer;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Configuration;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Api.Importer.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class SeedService : ISeedService
    {
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly CsvImporterFactory _csvImporterFactory = new CsvImporterFactory();
        private readonly IOptions<SeedConfigurationOptions> _options;

        private const string indicatorMetaTableName = "IndicatorMeta";
        private const string characteristicMetaTableName = "CharacteristicMeta";

        public SeedService(ILogger<SeedService> logger,
            ApplicationDbContext context,
            IOptions<SeedConfigurationOptions> options)
        {
            _logger = logger;
            _context = context;
            _options = options;
        }

        public void DeleteAll()
        {
            _context.Database.ExecuteSqlCommand("truncate table ReleaseIndicatorMeta");
            _context.Database.ExecuteSqlCommand("truncate table ReleaseCharacteristicMeta");


            _context.Database.ExecuteSqlCommand(
                "alter table ReleaseIndicatorMeta drop constraint FK_ReleaseIndicatorMeta_IndicatorMeta_IndicatorMetaId");
            _context.Database.ExecuteSqlCommand("truncate table IndicatorMeta");
            _context.Database.ExecuteSqlCommand(
                "alter table ReleaseIndicatorMeta add constraint FK_ReleaseIndicatorMeta_IndicatorMeta_IndicatorMetaId foreign key (IndicatorMetaId) references IndicatorMeta (Id)");


            _context.Database.ExecuteSqlCommand(
                "alter table ReleaseCharacteristicMeta drop constraint FK_ReleaseCharacteristicMeta_CharacteristicMeta_CharacteristicMetaId");
            _context.Database.ExecuteSqlCommand("truncate table CharacteristicMeta");
            _context.Database.ExecuteSqlCommand(
                "alter table ReleaseCharacteristicMeta add constraint FK_ReleaseCharacteristicMeta_CharacteristicMeta_CharacteristicMetaId foreign key (CharacteristicMetaId) references CharacteristicMeta (Id)");


            _context.Database.ExecuteSqlCommand(
                "alter table ReleaseIndicatorMeta drop constraint FK_ReleaseIndicatorMeta_Release_ReleaseId");
            _context.Database.ExecuteSqlCommand(
                "alter table ReleaseCharacteristicMeta drop constraint FK_ReleaseCharacteristicMeta_Release_ReleaseId");
            _context.Database.ExecuteSqlCommand(
                "alter table GeographicData drop constraint FK_GeographicData_Release_ReleaseId");
            _context.Database.ExecuteSqlCommand(
                "alter table CharacteristicDataNational drop constraint FK_CharacteristicDataNational_Release_ReleaseId");
            _context.Database.ExecuteSqlCommand(
                "alter table CharacteristicDataLa drop constraint FK_CharacteristicDataLa_Release_ReleaseId");


            _context.Database.ExecuteSqlCommand("truncate table GeographicData");
            _context.Database.ExecuteSqlCommand("truncate table CharacteristicDataLa");
            _context.Database.ExecuteSqlCommand("truncate table CharacteristicDataNational");
            _context.Database.ExecuteSqlCommand("truncate table Release");


            _context.Database.ExecuteSqlCommand(
                "alter table ReleaseIndicatorMeta add constraint FK_ReleaseIndicatorMeta_Release_ReleaseId foreign key (ReleaseId) references Release (Id)");
            _context.Database.ExecuteSqlCommand(
                "alter table ReleaseCharacteristicMeta add constraint FK_ReleaseCharacteristicMeta_Release_ReleaseId foreign key (ReleaseId) references Release (Id)");
            _context.Database.ExecuteSqlCommand(
                "alter table GeographicData add constraint FK_GeographicData_Release_ReleaseId foreign key (ReleaseId) references Release (Id)");
            _context.Database.ExecuteSqlCommand(
                "alter table CharacteristicDataNational add constraint FK_CharacteristicDataNational_Release_ReleaseId foreign key (ReleaseId) references Release (Id)");
            _context.Database.ExecuteSqlCommand(
                "alter table CharacteristicDataLa add constraint FK_CharacteristicDataLa_Release_ReleaseId foreign key (ReleaseId) references Release (Id)");
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

        private void SeedDataSet(Models.Release release, DataSet dataSet)
        {
            SeedDataSetMeta(release, dataSet);
            _context.SaveChanges();

            var importer = _csvImporterFactory.Importer(dataSet.Filename);
            var data = importer.Data(dataSet.Filename, release);

            _logger.LogInformation("Seeding {Filename}.csv", dataSet.Filename);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            if (dataSet.getDataType() == typeof(GeographicData))
            {
                SeedGeographicData(data.OfType<GeographicData>().ToList());
            }

            if (dataSet.getDataType() == typeof(CharacteristicDataNational))
            {
                SeedCharacteristicDataNational(data.OfType<CharacteristicDataNational>().ToList());
            }

            if (dataSet.getDataType() == typeof(CharacteristicDataLa))
            {
                SeedCharacteristicDataLa(data.OfType<CharacteristicDataLa>().ToList());
            }

            _logger.LogInformation("Seeded {Count} records in duration {Duration}. {TimerPerRecord}ms per record",
                data.Count(), stopWatch.Elapsed.ToString(), stopWatch.Elapsed.TotalMilliseconds / data.Count());

            stopWatch.Stop();
        }

        private void SeedGeographicData(IList<GeographicData> geographicData)
        {
            _context.BulkInsert(geographicData, CreateBulkConfigForSeed());
        }

        private void SeedCharacteristicDataNational(IList<CharacteristicDataNational> characteristicDataNational)
        {
            _context.BulkInsert(characteristicDataNational, CreateBulkConfigForSeed());
        }

        private void SeedCharacteristicDataLa(IList<CharacteristicDataLa> characteristicDataLa)
        {
            _context.BulkInsert(characteristicDataLa, CreateBulkConfigForSeed());
        }

        private void SeedDataSetMeta(Models.Release releaseDb, DataSet dataSet)
        {
            SeedDataSetIndicators(releaseDb, dataSet);
            SeedDataSetCharacteristics(releaseDb, dataSet);
        }

        private void SeedDataSetIndicators(Models.Release releaseDb, DataSet dataSet)
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

        private void SeedDataSetCharacteristics(Models.Release releaseDb, DataSet dataSet)
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

        private static BulkConfig CreateBulkConfigForSeed()
        {
            return new BulkConfig {PreserveInsertOrder = true};
        }

        private Models.Release CreateRelease(Release release)
        {
            return _context.Release.Add(new Models.Release(release.ReleaseDate, release.PublicationId)).Entity;
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