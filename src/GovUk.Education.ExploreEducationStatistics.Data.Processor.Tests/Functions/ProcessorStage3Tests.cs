#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Functions;

public class ProcessorStage3Tests : IDisposable
{
    private readonly string _contentDbContextId = Guid.NewGuid().ToString();
    private readonly string _statisticsDbContextId = Guid.NewGuid().ToString();
    
    private readonly DataFixture _fixture = new();

    private readonly List<Location> _locations;
    private readonly Subject _subject;

    public ProcessorStage3Tests()
    {
        Environment.SetEnvironmentVariable("RowsPerBatch", "5000");

        var country = new Country("E92000001", "England");
        
        _locations = _fixture
            .DefaultLocation()
            .WithGeographicLevel(GeographicLevel.LocalAuthority)
            .WithCountry(country)
            .ForIndex(0, s => s.SetLocalAuthority(new LocalAuthority("E08000025", "330", "Birmingham")))
            .ForIndex(1, s => s.SetLocalAuthority(new LocalAuthority("E09000011", "203", "Greenwich")))
            .ForIndex(2, s => s.SetLocalAuthority(new LocalAuthority("E08000016", "370", "Barnsley")))
            .ForIndex(3, s => s.SetLocalAuthority(new LocalAuthority("E09000007", "202", "Camden")))
            .GenerateList();
        
        _subject = _fixture
            .DefaultSubject()
            .WithFilters(_fixture
                .DefaultFilter()
                .WithLabel("Filter one")
                .WithFilterGroups(_fixture
                    .DefaultFilterGroup()
                    .WithFilterItems(_fixture
                        .DefaultFilterItem()
                        .WithLabel("Total")
                        .Generate(1))
                    .Generate(1))
                .Generate(1))
            .WithIndicatorGroups(_fixture
                .DefaultIndicatorGroup()
                .WithIndicators(_fixture
                    .DefaultIndicator()
                    .WithLabel("Indicator one")
                    .Generate(1))
                .Generate(1))
            .Generate();
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("RowsPerBatch", null);
    }
    
    [Fact]
    public async Task ProcessStage3()
    {
        var import = _fixture
            .DefaultDataImport()
            .WithSubjectId(_subject.Id)
            .WithDefaultFiles("small-csv")
            .WithStatus(DataImportStatus.STAGE_3)
            .WithRowCounts(
                totalRows: 16,
                expectedImportedRows: 16
            )
            .Generate();
    
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
            
            await statisticsDbContext.Subject.AddAsync(_subject);
            await statisticsDbContext.Location.AddRangeAsync(_locations);
            await statisticsDbContext.SaveChangesAsync();
        }
    
        var blobStorageService = new Mock<IBlobStorageService>(Strict);
    
        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);
    
        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);
        
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
            
            // Verify that the import status has completed successfully.
            Assert.Equal(16, dataImport.ImportedRows);
            Assert.Equal(15, dataImport.LastProcessedRowIndex);
            Assert.Equal(DataImportStatus.COMPLETE, dataImport.Status);
        }
    }
    
    [Fact]
    public async Task ProcessStage3_FailsImportIfRowCountsDontMatch()
    {
        var import = _fixture
            .DefaultDataImport()
            .WithSubjectId(_subject.Id)
            .WithDefaultFiles("small-csv")
            .WithStatus(DataImportStatus.STAGE_3)
            .WithRowCounts(
                totalRows: 16,
                expectedImportedRows: 16
            )
            .Generate();
        
        var unexpectedImportedObservation = _fixture
            .DefaultObservation()
            .WithSubject(_subject)
            .Generate();
    
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
            
            await statisticsDbContext.Subject.AddAsync(_subject);
            await statisticsDbContext.Location.AddRangeAsync(_locations);
            await statisticsDbContext.Observation.AddRangeAsync(unexpectedImportedObservation);
            await statisticsDbContext.SaveChangesAsync();
        }
    
        var blobStorageService = new Mock<IBlobStorageService>(Strict);
    
        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);
    
        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);
        
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
            
            // Verify that the import status failed because of the addition Observation row.
            Assert.Equal(16, dataImport.ImportedRows);
            Assert.Equal(15, dataImport.LastProcessedRowIndex);
            Assert.Equal(DataImportStatus.FAILED, dataImport.Status);
        }
    }
    
    [Fact]
    public async Task ProcessStage3_PartiallyImportedAlready()
    {
        // This import has already imported 4 rows and is now being restarted.
        var import = _fixture
            .DefaultDataImport()
            .WithSubjectId(_subject.Id)
            .WithDefaultFiles("small-csv")
            .WithStatus(DataImportStatus.STAGE_3)
            .WithRowCounts(
                totalRows: 16,
                expectedImportedRows: 16,
                importedRows: 4,
                lastProcessedRowIndex: 3
            )
            .Generate();

        var alreadyImportedObservations = _fixture
            .DefaultObservation()
            .WithSubject(_subject)
            .Generate(4);
    
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
            
            await statisticsDbContext.Subject.AddAsync(_subject);
            await statisticsDbContext.Location.AddRangeAsync(_locations);
            await statisticsDbContext.Observation.AddRangeAsync(alreadyImportedObservations);
            
            await statisticsDbContext.SaveChangesAsync();
        }
    
        var blobStorageService = new Mock<IBlobStorageService>(Strict);
    
        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);
    
        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);
        
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
            
            // Verify that the import status has completed successfully.
            Assert.Equal(16, dataImport.ImportedRows);
            Assert.Equal(15, dataImport.LastProcessedRowIndex);
            Assert.Equal(DataImportStatus.COMPLETE, dataImport.Status);
        }
    }
    
    [Fact]
    public async Task ProcessStage3_PartiallyImportedAlready_BatchSizeChanged()
    {
        Environment.SetEnvironmentVariable("RowsPerBatch", "3");
        
        // This import has already imported 10 rows and is now being restarted.
        // The number of rows to import per batch has now changed to 3 though.
        // This means that the first 3 batches of 3 have already been fully 
        // imported. Of the 4th batch of 3 rows, the first row has been imported
        // already, so of the 4th batch, only the 2nd and 3rd rows should be imported.
        // The remaining 2 batches of 3 and 1 rows respectively should be imported fully. 
        var import = _fixture
            .DefaultDataImport()
            .WithSubjectId(_subject.Id)
            .WithDefaultFiles("small-csv")
            .WithStatus(DataImportStatus.STAGE_3)
            .WithRowCounts(
                totalRows: 16,
                expectedImportedRows: 16,
                importedRows: 10,
                lastProcessedRowIndex: 9
            )
            .Generate();

        var alreadyImportedObservations = _fixture
            .DefaultObservation()
            .WithSubject(_subject)
            .Generate(10);
    
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
            
            await statisticsDbContext.Subject.AddAsync(_subject);
            await statisticsDbContext.Location.AddRangeAsync(_locations);
            await statisticsDbContext.Observation.AddRangeAsync(alreadyImportedObservations);
            
            await statisticsDbContext.SaveChangesAsync();
        }
    
        var blobStorageService = new Mock<IBlobStorageService>(Strict);
    
        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);
    
        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);
        
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
            
            // Verify that the import status has completed successfully.
            Assert.Equal(16, dataImport.ImportedRows);
            Assert.Equal(15, dataImport.LastProcessedRowIndex);
            Assert.Equal(DataImportStatus.COMPLETE, dataImport.Status);
        }
    }
    
    [Fact]
    public async Task ProcessStage3_IgnoredRows()
    {
        // This import expects only 8 out of 16 rows to be imported, as 8 of them are ignored.
        var import = _fixture
            .DefaultDataImport()
            .WithSubjectId(_subject.Id)
            .WithDefaultFiles("ignored-school-rows")
            .WithStatus(DataImportStatus.STAGE_3)
            .WithRowCounts(
                totalRows: 16,
                expectedImportedRows: 8
            )
            .Generate();
    
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
            
            await statisticsDbContext.Subject.AddAsync(_subject);
            await statisticsDbContext.Location.AddRangeAsync(_locations);
            await statisticsDbContext.SaveChangesAsync();
        }
    
        var blobStorageService = new Mock<IBlobStorageService>(Strict);
    
        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);
    
        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);
        
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
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has completed successfully.
            Assert.Equal(8, dataImport.ImportedRows);
            Assert.Equal(15, dataImport.LastProcessedRowIndex);
            Assert.Equal(DataImportStatus.COMPLETE, dataImport.Status);
            
            // Verify that only the LA-level data was imported and the School-level data ignored.
            statisticsDbContext
                .Observation
                .Include(o => o.Location)
                .ForEach(o => Assert.Equal(GeographicLevel.LocalAuthority, o.Location.GeographicLevel));
        }
    }
    
        [Fact]
    public async Task ProcessStage3_IgnoredRows_PartiallyImported()
    {
        Environment.SetEnvironmentVariable("RowsPerBatch", "3");
        
        // This import expects only 8 out of 16 rows to be imported, as 8 of them are ignored.
        // This import was interrupted and is restarting. So far, 7 rows have been processed, and of those 7,
        // 4 are LA-level data that has been imported, and 3 are School-level data that has been ignored.
        var import = _fixture
            .DefaultDataImport()
            .WithSubjectId(_subject.Id)
            .WithDefaultFiles("ignored-school-rows")
            .WithStatus(DataImportStatus.STAGE_3)
            .WithRowCounts(
                totalRows: 16,
                expectedImportedRows: 8,
                importedRows: 4,
                lastProcessedRowIndex: 6
            )
            .Generate();
        
        var alreadyImportedObservations = _fixture
            .DefaultObservation()
            .WithLocation(_fixture
                .DefaultLocation()
                .WithGeographicLevel(GeographicLevel.LocalAuthority)
                .Generate())
            .WithSubject(_subject)
            .Generate(4);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
            
            await statisticsDbContext.Subject.AddAsync(_subject);
            await statisticsDbContext.Location.AddRangeAsync(_locations);
            await statisticsDbContext.Observation.AddRangeAsync(alreadyImportedObservations);
            
            await statisticsDbContext.SaveChangesAsync();
        }
    
        var blobStorageService = new Mock<IBlobStorageService>(Strict);
    
        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);
    
        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);
        
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
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has completed successfully.
            Assert.Equal(8, dataImport.ImportedRows);
            Assert.Equal(15, dataImport.LastProcessedRowIndex);
            Assert.Equal(DataImportStatus.COMPLETE, dataImport.Status);
            
            // Verify that only the LA-level data was imported and the School-level data ignored.
            statisticsDbContext
                .Observation
                .Include(o => o.Location)
                .ForEach(o => Assert.Equal(GeographicLevel.LocalAuthority, o.Location.GeographicLevel));
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
