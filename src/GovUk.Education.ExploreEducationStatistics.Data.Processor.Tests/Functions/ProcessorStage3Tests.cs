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
using Microsoft.Azure.WebJobs;
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

public class ProcessorStage3Tests : IDisposable
{
    private readonly string _contentDbContextId = Guid.NewGuid().ToString();
    private readonly string _statisticsDbContextId = Guid.NewGuid().ToString();

    public ProcessorStage3Tests()
    {
        Environment.SetEnvironmentVariable("RowsPerBatch", "5000");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("RowsPerBatch", null);
    }
    
    [Fact]
    public async Task ProcessStage3()
    {
        var dataFileUnderTest = "small-csv.csv";
        var metaFileUnderTest = dataFileUnderTest.Replace(".csv", ".meta.csv");
    
        var subject = new Subject
        {
            Id = Guid.NewGuid()
        };
    
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
            Status = DataImportStatus.STAGE_3,
            TotalRows = 16,
            ExpectedImportedRows = 16
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
            Mock.Of<ILogger<DataImportService>>());
    
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
            dataImportService,
            Mock.Of<ILogger<ImporterService>>(),
            databaseHelper,
            memoryCache,
            observationBatchImporter);
    
        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            blobStorageService.Object,
            dataImportService,
            importerService);
        
        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            blobStorageService.Object,
            fileImportService,
            importerService,
            dataImportService,
            Mock.Of<IValidatorService>(Strict),
            Mock.Of<IDataArchiveService>(Strict),
            dbContextSupplier);
        
        var function = BuildFunction(
            processorService: processorService, 
            dataImportService: dataImportService);
    
        await function.ProcessUploads(
            new ImportMessage(import.Id),
            new ExecutionContext(),
            Mock.Of<ICollector<ImportMessage>>(Strict));
        
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
        IDataImportService? dataImportService = null)
    {
        return new Processor.Functions.Processor(
            dataImportService ?? Mock.Of<IDataImportService>(Strict),
            processorService ?? Mock.Of<IProcessorService>(Strict),
            Mock.Of<ILogger<Processor.Functions.Processor>>());
    }
}
