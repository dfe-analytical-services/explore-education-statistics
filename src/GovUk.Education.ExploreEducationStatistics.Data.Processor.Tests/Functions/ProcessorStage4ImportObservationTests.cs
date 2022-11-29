#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Functions;

public class ProcessorStage4ImportObservationTests
{
    private readonly string _contentDbContextId = Guid.NewGuid().ToString();
    private readonly string _statisticsDbContextId = Guid.NewGuid().ToString();

    [Fact]
    public async Task ProcessStage4_ImportObservation_NoBatchingWasRequired()
    {
        var dataFileUnderTest = "small-csv.csv";
        var metaFileUnderTest = dataFileUnderTest.Replace(".csv", ".meta.csv");

        var subject = new Subject
        {
            Id = Guid.NewGuid()
        };

        // The total rows to attempt to import is the same as the RowsPerBatch, and therefore the file did not require
        // batching at Stage 3.
        var import = new DataImport
        {
            Id = Guid.NewGuid(),
            SubjectId = subject.Id,
            File = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest,
                Type = FileType.Data
            },
            MetaFile = new File
            {
                Id = Guid.NewGuid(),
                Filename = metaFileUnderTest,
                Type = FileType.Metadata
            },
            Status = DataImportStatus.STAGE_4,
            RowsPerBatch = 16,
            TotalRows = 16,
            ImportedRows = 16,
            NumBatches = 1
        };

        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        var filter = new Filter("", "Filter one", "filter_one", subject.Id);
        var filterGroup = new FilterGroup(filter.Id, "Default");
        filterGroup.FilterItems.Add(new FilterItem("Total", filterGroup.Id));
        filter.FilterGroups.Add(filterGroup);

        var indicatorGroup = new IndicatorGroup("Default", subject.Id);
        indicatorGroup.Indicators.Add(new Indicator
        {
            Label = "Indicator one",
            Name = "ind_one"
        });
        
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await statisticsDbContext.Subject.AddAsync(subject);
            await statisticsDbContext.Filter.AddAsync(filter);
            await statisticsDbContext.IndicatorGroup.AddAsync(indicatorGroup);
            await statisticsDbContext.Location.AddRangeAsync(ListOf(
                new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Country = new Country("E92000001", "England"),
                    LocalAuthority = new LocalAuthority("E08000025", "330", "Birmingham")
                },
                new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Country = new Country("E92000001", "England"),
                    LocalAuthority = new LocalAuthority("E08000016", "370", "Barnsley")
                },
                new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Country = new Country("E92000001", "England"),
                    LocalAuthority = new LocalAuthority("E09000011", "203", "Greenwich")
                },
                new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Country = new Country("E92000001", "England"),
                    LocalAuthority = new LocalAuthority("E09000007", "202", "Camden")
                }));
            await statisticsDbContext.SaveChangesAsync();
        }

        // There should be no interactions with BlobStorage if no batching is required.
        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + dataFileUnderTest);

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + metaFileUnderTest);
        
        blobStorageService.SetupStreamBlob(
            PrivateReleaseFiles, 
            import.File.Path(), 
            dataFilePath);
        
        blobStorageService.SetupStreamBlob(
            PrivateReleaseFiles, 
            import.MetaFile.Path(), 
            metaFilePath);
        
        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId: _contentDbContextId,
            statisticsDbContextId: _statisticsDbContextId);

        var databaseHelper = new InMemoryDatabaseHelper(dbContextSupplier);
        
        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>(),
            databaseHelper);

        var memoryCache = new ImporterFilterCache();

        var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            // Fill the ImporterLocationCache with all existing Locations on "startup" of the Importer.
            // Note that this occurs in Startup.cs.
            importerLocationCache.LoadLocations(statisticsDbContext);
        }

        var guidGenerator = new SequentialGuidGenerator();

        var importerMetaService = new ImporterMetaService(
            guidGenerator, 
            databaseHelper);

        var observationBatchImporter = new TestObservationBatchImporter();
        
        var importerService = new ImporterService(
            guidGenerator,
            new ImporterFilterService(memoryCache),
            new ImporterLocationService(
                guidGenerator, 
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            importerMetaService,
            Mock.Of<IDataImportService>(Strict),
            Mock.Of<ILogger<ImporterService>>(),
            databaseHelper,
            memoryCache,
            observationBatchImporter);

        var batchService = new BatchService(blobStorageService.Object);
        
        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            batchService,
            blobStorageService.Object,
            dataImportService,
            importerService,
            databaseHelper);
        
        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            blobStorageService.Object,
            fileImportService,
            Mock.Of<ISplitFileService>(Strict),
            importerService,
            dataImportService,
            Mock.Of<IValidatorService>(Strict),
            Mock.Of<IDataArchiveService>(Strict),
            dbContextSupplier);
        
        var function = BuildFunction(
            processorService: processorService, 
            dataImportService: dataImportService,
            fileImportService: fileImportService,
            dbContextSupplier: dbContextSupplier);

        await function.ImportObservations(new ImportObservationsMessage
        {
            BatchNo = 1,
            Id = import.Id,
            ObservationsFilePath = import.File.Path()
        });
        
        VerifyAllMocks(blobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status is COMPLETE now that the only data file to import has been imported,
            // and imported successfully.
            Assert.Equal(DataImportStatus.COMPLETE, dataImport.Status);
        }
    }
    
    [Fact]
    public async Task ProcessStage4_ImportObservation_BatchingWasRequired_MoreBatchedRemain()
    {
        var dataFileUnderTest = "small-csv.csv";
        var metaFileUnderTest = dataFileUnderTest.Replace(".csv", ".meta.csv");

        var subject = new Subject
        {
            Id = Guid.NewGuid()
        };

        // The total rows to attempt to import is the same as the RowsPerBatch, and therefore the file did not require
        // batching at Stage 3.
        var import = new DataImport
        {
            Id = Guid.NewGuid(),
            SubjectId = subject.Id,
            File = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest,
                Type = FileType.Data
            },
            MetaFile = new File
            {
                Id = Guid.NewGuid(),
                Filename = metaFileUnderTest,
                Type = FileType.Metadata
            },
            Status = DataImportStatus.STAGE_4,
            RowsPerBatch = 10,
            TotalRows = 16,
            ImportedRows = 16,
            NumBatches = 2
        };

        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        var filter = new Filter("", "Filter one", "filter_one", subject.Id);
        var filterGroup = new FilterGroup(filter.Id, "Default");
        filterGroup.FilterItems.Add(new FilterItem("Total", filterGroup.Id));
        filter.FilterGroups.Add(filterGroup);

        var indicatorGroup = new IndicatorGroup("Default", subject.Id);
        indicatorGroup.Indicators.Add(new Indicator
        {
            Label = "Indicator one",
            Name = "ind_one"
        });
        
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await statisticsDbContext.Subject.AddAsync(subject);
            await statisticsDbContext.Filter.AddAsync(filter);
            await statisticsDbContext.IndicatorGroup.AddAsync(indicatorGroup);
            await statisticsDbContext.Location.AddRangeAsync(ListOf(
                new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Country = new Country("E92000001", "England"),
                    LocalAuthority = new LocalAuthority("E08000025", "330", "Birmingham")
                },
                new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Country = new Country("E92000001", "England"),
                    LocalAuthority = new LocalAuthority("E08000016", "370", "Barnsley")
                },
                new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Country = new Country("E92000001", "England"),
                    LocalAuthority = new LocalAuthority("E09000011", "203", "Greenwich")
                },
                new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Country = new Country("E92000001", "England"),
                    LocalAuthority = new LocalAuthority("E09000007", "202", "Camden")
                }));
            await statisticsDbContext.SaveChangesAsync();
        }
        
        var importObservationsMessage = new ImportObservationsMessage
        {
            BatchNo = 1,
            Id = import.Id,
            ObservationsFilePath = import.File.Path()
        };

        // There should be no interactions with BlobStorage if no batching is required.
        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + dataFileUnderTest);

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + metaFileUnderTest);

        blobStorageService.SetupStreamBlob(
            PrivateReleaseFiles, 
            import.File.Path(), 
            dataFilePath);
        
        blobStorageService.SetupStreamBlob(
            PrivateReleaseFiles, 
            import.MetaFile.Path(), 
            metaFilePath);
        
        blobStorageService
            .Setup(s => s.DeleteBlob(PrivateReleaseFiles, importObservationsMessage.ObservationsFilePath))
            .Returns(Task.CompletedTask);
        
        // There is one additional batch file remaining.
        blobStorageService.SetupListBlobs(
            PrivateReleaseFiles, 
            import.File.BatchesPath(), 
            new BlobInfo($"{import.File.BatchesPath()}{import.File.Id}_000002", "text/csv", 0));

        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId: _contentDbContextId,
            statisticsDbContextId: _statisticsDbContextId);

        var databaseHelper = new InMemoryDatabaseHelper(dbContextSupplier);
        
        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>(),
            databaseHelper);

        var memoryCache = new ImporterFilterCache();

        var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            // Fill the ImporterLocationCache with all existing Locations on "startup" of the Importer.
            // Note that this occurs in Startup.cs.
            importerLocationCache.LoadLocations(statisticsDbContext);
        }
        
        var guidGenerator = new SequentialGuidGenerator();

        var importerMetaService = new ImporterMetaService(
            guidGenerator, 
            databaseHelper);

        var observationBatchImporter = new TestObservationBatchImporter();
        
        var importerService = new ImporterService(
            guidGenerator,
            new ImporterFilterService(memoryCache),
            new ImporterLocationService(
                guidGenerator, 
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            importerMetaService,
            Mock.Of<IDataImportService>(Strict),
            Mock.Of<ILogger<ImporterService>>(),
            databaseHelper,
            memoryCache,
            observationBatchImporter);

        var batchService = new BatchService(blobStorageService.Object);
        
        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            batchService,
            blobStorageService.Object,
            dataImportService,
            importerService,
            databaseHelper);
        
        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            blobStorageService.Object,
            fileImportService,
            Mock.Of<ISplitFileService>(Strict),
            importerService,
            dataImportService,
            Mock.Of<IValidatorService>(Strict),
            Mock.Of<IDataArchiveService>(Strict),
            dbContextSupplier);
        
        var function = BuildFunction(
            processorService: processorService, 
            dataImportService: dataImportService,
            fileImportService: fileImportService,
            dbContextSupplier: dbContextSupplier);

        await function.ImportObservations(importObservationsMessage);
        
        VerifyAllMocks(blobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status is still at Stage 4, as there are still more batch files to process for
            // this Subject.
            Assert.Equal(DataImportStatus.STAGE_4, dataImport.Status);
        }
    }
    
    [Fact]
    public async Task ProcessStage4_ImportObservation_BatchingWasRequired_LastBatchImported()
    {
        var dataFileUnderTest = "small-csv.csv";
        var metaFileUnderTest = dataFileUnderTest.Replace(".csv", ".meta.csv");

        var subject = new Subject
        {
            Id = Guid.NewGuid()
        };

        // The total rows to attempt to import is the same as the RowsPerBatch, and therefore the file did not require
        // batching at Stage 3.
        var import = new DataImport
        {
            Id = Guid.NewGuid(),
            SubjectId = subject.Id,
            File = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest,
                Type = FileType.Data
            },
            MetaFile = new File
            {
                Id = Guid.NewGuid(),
                Filename = metaFileUnderTest,
                Type = FileType.Metadata
            },
            Status = DataImportStatus.STAGE_4,
            RowsPerBatch = 10,
            TotalRows = 16,
            ImportedRows = 16,
            NumBatches = 2
        };

        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        var filter = new Filter("", "Filter one", "filter_one", subject.Id);
        var filterGroup = new FilterGroup(filter.Id, "Default");
        filterGroup.FilterItems.Add(new FilterItem("Total", filterGroup.Id));
        filter.FilterGroups.Add(filterGroup);

        var indicatorGroup = new IndicatorGroup("Default", subject.Id);
        indicatorGroup.Indicators.Add(new Indicator
        {
            Label = "Indicator one",
            Name = "ind_one"
        });
        
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await statisticsDbContext.Subject.AddAsync(subject);
            await statisticsDbContext.Filter.AddAsync(filter);
            await statisticsDbContext.IndicatorGroup.AddAsync(indicatorGroup);
            await statisticsDbContext.Location.AddRangeAsync(ListOf(
                new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Country = new Country("E92000001", "England"),
                    LocalAuthority = new LocalAuthority("E08000025", "330", "Birmingham")
                },
                new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Country = new Country("E92000001", "England"),
                    LocalAuthority = new LocalAuthority("E08000016", "370", "Barnsley")
                },
                new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Country = new Country("E92000001", "England"),
                    LocalAuthority = new LocalAuthority("E09000011", "203", "Greenwich")
                },
                new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Country = new Country("E92000001", "England"),
                    LocalAuthority = new LocalAuthority("E09000007", "202", "Camden")
                }));
            await statisticsDbContext.SaveChangesAsync();
        }
        
        var importObservationsMessage = new ImportObservationsMessage
        {
            BatchNo = 1,
            Id = import.Id,
            ObservationsFilePath = import.File.Path()
        };

        // There should be no interactions with BlobStorage if no batching is required.
        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + dataFileUnderTest);

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + metaFileUnderTest);

        blobStorageService.SetupStreamBlob(
            PrivateReleaseFiles, 
            import.File.Path(), 
            dataFilePath);
        
        blobStorageService.SetupStreamBlob(
            PrivateReleaseFiles, 
            import.MetaFile.Path(), 
            metaFilePath);
        
        blobStorageService
            .Setup(s => s.DeleteBlob(PrivateReleaseFiles, importObservationsMessage.ObservationsFilePath))
            .Returns(Task.CompletedTask);
        
        blobStorageService.SetupListBlobs(PrivateReleaseFiles, import.File.BatchesPath());

        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId: _contentDbContextId,
            statisticsDbContextId: _statisticsDbContextId);

        var databaseHelper = new InMemoryDatabaseHelper(dbContextSupplier);
        
        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>(),
            databaseHelper);

        var memoryCache = new ImporterFilterCache();

        var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            // Fill the ImporterLocationCache with all existing Locations on "startup" of the Importer.
            // Note that this occurs in Startup.cs.
            importerLocationCache.LoadLocations(statisticsDbContext);
        }
        
        var guidGenerator = new SequentialGuidGenerator();

        var importerMetaService = new ImporterMetaService(
            guidGenerator, 
            databaseHelper);

        var observationBatchImporter = new TestObservationBatchImporter();
        
        var importerService = new ImporterService(
            guidGenerator,
            new ImporterFilterService(memoryCache),
            new ImporterLocationService(
                guidGenerator, 
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            importerMetaService,
            Mock.Of<IDataImportService>(Strict),
            Mock.Of<ILogger<ImporterService>>(),
            databaseHelper,
            memoryCache,
            observationBatchImporter);

        var batchService = new BatchService(blobStorageService.Object);
        
        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            batchService,
            blobStorageService.Object,
            dataImportService,
            importerService,
            databaseHelper);
        
        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            blobStorageService.Object,
            fileImportService,
            Mock.Of<ISplitFileService>(Strict),
            importerService,
            dataImportService,
            Mock.Of<IValidatorService>(Strict),
            Mock.Of<IDataArchiveService>(Strict),
            dbContextSupplier);
        
        var function = BuildFunction(
            processorService: processorService, 
            dataImportService: dataImportService,
            fileImportService: fileImportService,
            dbContextSupplier: dbContextSupplier);

        await function.ImportObservations(importObservationsMessage);
        
        VerifyAllMocks(blobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status is now COMPLETE, as this was the last batch file to process.
            Assert.Equal(DataImportStatus.COMPLETE, dataImport.Status);
        }
    }
    
    private class TestObservationBatchImporter : IObservationBatchImporter
    {
        public async Task ImportObservationBatch(StatisticsDbContext context, IEnumerable<Observation> observations)
        {
            await context.Observation.AddRangeAsync(observations);
            await context.SaveChangesAsync();
        }
    }
    
    private static Processor.Functions.Processor BuildFunction(
        IProcessorService? processorService = null,
        IDataImportService? dataImportService = null,
        IFileImportService? fileImportService = null,
        IDbContextSupplier? dbContextSupplier = null)
    {
        return new Processor.Functions.Processor(
            fileImportService ?? Mock.Of<IFileImportService>(Strict),
            dataImportService ?? Mock.Of<IDataImportService>(Strict),
            processorService ?? Mock.Of<IProcessorService>(Strict),
            dbContextSupplier ?? Mock.Of<IDbContextSupplier?>(Strict),
            Mock.Of<ILogger<Processor.Functions.Processor>>());
    }
}