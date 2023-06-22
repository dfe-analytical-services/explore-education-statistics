#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.ComparerUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
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
                .ForIndex(0, s => s
                    .SetLabel("Filter one")
                    .SetFilterGroups(_fixture
                        .DefaultFilterGroup()
                        .WithFilterItems(_fixture
                            .DefaultFilterItem()
                            .WithLabel("Total")
                            .Generate(1))
                        .Generate(1)))
                .ForIndex(1, s => s
                    .SetLabel("Filter two")
                    .SetFilterGroups(_fixture
                        .DefaultFilterGroup()
                        .WithFilterItems(_fixture
                            .DefaultFilterItem()
                            .ForInstance(s => s.Set(
                                fi => fi.Label, 
                                (_, _, context) => $"Choice {context.Index + 1}"))
                            .Generate(16))
                        .Generate(1)))
                .GenerateList())
            .WithIndicatorGroups(_fixture
                .DefaultIndicatorGroup()
                .WithIndicators(_fixture
                    .DefaultIndicator()
                    .ForIndex(0, s => s.SetLabel("Indicator one"))
                    .ForIndex(1, s => s.SetLabel("Indicator two"))
                    .GenerateList())
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
            .WithFiles("small-csv")
            .WithStatus(STAGE_3)
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
    
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);

        privateBlobStorageService.SetupStreamBlob(
            PrivateReleaseFiles,
            import.File.Path(),
            dataFilePath);

        privateBlobStorageService.SetupStreamBlob(
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
            new ImporterLocationService(
                guidGenerator,
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            importerMetaService,
            dataImportService,
            Mock.Of<ILogger<ImporterService>>(),
            databaseHelper,
            observationBatchImporter);

        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            privateBlobStorageService.Object,
            dataImportService,
            importerService);

        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            privateBlobStorageService.Object,
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

        VerifyAllMocks(privateBlobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has completed successfully.
            Assert.Equal(16, dataImport.ImportedRows);
            Assert.Equal(15, dataImport.LastProcessedRowIndex);
            Assert.Equal(COMPLETE, dataImport.Status);

            var observations = await statisticsDbContext
                .Observation
                .Include(o => o.FilterItems)
                .ToListAsync();
            
            Assert.Equal(16, observations.Count);
            
            var expectedIndicatorIds = _subject
                .IndicatorGroups
                .SelectMany(ig => ig.Indicators)
                .Select(i => i.Id);
            
            var expectedFilterItemIds = _subject
                .Filters
                .Select(f => f.Id);

            observations.ForEach(observation =>
            {
                // Check that measures for each Indicator are present.
                var indicatorIds = observation.Measures.Keys;
                Assert.True(SequencesAreEqualIgnoringOrder(expectedIndicatorIds, indicatorIds));
                    
                // Check that a Filter Item for each Filter is present in each row
                var filterItemIds = 
                    observation.FilterItems.Select(ofi => ofi.FilterId!.Value);
                Assert.True(SequencesAreEqualIgnoringOrder(expectedFilterItemIds, filterItemIds));
            });

            // Check the CsvRow values. They should equal the 1-based position in the
            // CSV, *including* the header row. So the first data row will have CsvRow
            // of 2. 
            Enumerable.Range(0, 16).ForEach(i => 
                Assert.Equal(i + 2, observations[i].CsvRow));

            // Thoroughly check the first Observation row for correct details.
            var firstObservation = observations[0];
            var expectedLocation = _locations.Single(l => l.LocalAuthority!.Name == "Birmingham");
            Assert.Equal(expectedLocation.Id, firstObservation.LocationId);
            Assert.Equal(2018, firstObservation.Year);
            Assert.Equal(TimeIdentifier.CalendarYear, firstObservation.TimeIdentifier);
            Assert.Equal(
                _subject.Filters[0].FilterGroups[0].FilterItems[0].Id,
                firstObservation.FilterItems[0].FilterItemId);
            Assert.Equal(
                _subject.Filters[1].FilterGroups[0].FilterItems[0].Id,
                firstObservation.FilterItems[1].FilterItemId);
            Assert.Equal("1", firstObservation.Measures[_subject.IndicatorGroups[0].Indicators[0].Id]);
            Assert.Equal("2", firstObservation.Measures[_subject.IndicatorGroups[0].Indicators[1].Id]);
            
            // Thoroughly check the last Observation row for correct details.
            var lastObservation = observations[15];
            var expectedLocation2 = _locations.Single(l => l.LocalAuthority!.Name == "Camden");
            Assert.Equal(expectedLocation2.Id, lastObservation.LocationId);
            Assert.Equal(2025, lastObservation.Year);
            Assert.Equal(TimeIdentifier.CalendarYear, lastObservation.TimeIdentifier);
            Assert.Equal(
                _subject.Filters[0].FilterGroups[0].FilterItems[0].Id,
                lastObservation.FilterItems[0].FilterItemId);
            Assert.Equal(
                _subject.Filters[1].FilterGroups[0].FilterItems[15].Id,
                lastObservation.FilterItems[1].FilterItemId);
            Assert.Equal("16", lastObservation.Measures[_subject.IndicatorGroups[0].Indicators[0].Id]);
            Assert.Equal("32", lastObservation.Measures[_subject.IndicatorGroups[0].Indicators[1].Id]);
        }
    }
    
    [Fact]
    public async Task ProcessStage3_FailsImportIfRowCountsDontMatch()
    {
        var import = _fixture
            .DefaultDataImport()
            .WithSubjectId(_subject.Id)
            .WithFiles("small-csv")
            .WithStatus(STAGE_3)
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
    
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);

        privateBlobStorageService.SetupStreamBlob(
            PrivateReleaseFiles,
            import.File.Path(),
            dataFilePath);

        privateBlobStorageService.SetupStreamBlob(
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
            new ImporterLocationService(
                guidGenerator,
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            importerMetaService,
            dataImportService,
            Mock.Of<ILogger<ImporterService>>(),
            databaseHelper,
            observationBatchImporter);

        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            privateBlobStorageService.Object,
            dataImportService,
            importerService);

        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            privateBlobStorageService.Object,
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

        VerifyAllMocks(privateBlobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status failed because of the addition Observation row.
            Assert.Equal(16, dataImport.ImportedRows);
            Assert.Equal(15, dataImport.LastProcessedRowIndex);
            Assert.Equal(FAILED, dataImport.Status);
        }
    }
    
    [Fact]
    public async Task ProcessStage3_PartiallyImportedAlready()
    {
        // This import has already imported 4 rows and is now being restarted.
        var import = _fixture
            .DefaultDataImport()
            .WithSubjectId(_subject.Id)
            .WithFiles("small-csv")
            .WithStatus(STAGE_3)
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
    
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);

        privateBlobStorageService.SetupStreamBlob(
            PrivateReleaseFiles,
            import.File.Path(),
            dataFilePath);

        privateBlobStorageService.SetupStreamBlob(
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
            new ImporterLocationService(
                guidGenerator,
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            importerMetaService,
            dataImportService,
            Mock.Of<ILogger<ImporterService>>(),
            databaseHelper,
            observationBatchImporter);

        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            privateBlobStorageService.Object,
            dataImportService,
            importerService);

        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            privateBlobStorageService.Object,
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

        VerifyAllMocks(privateBlobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has completed successfully.
            Assert.Equal(16, dataImport.ImportedRows);
            Assert.Equal(15, dataImport.LastProcessedRowIndex);
            Assert.Equal(COMPLETE, dataImport.Status);

            var observations = await statisticsDbContext
                .Observation
                .ToListAsync();
            
            Assert.Equal(16, observations.Count);
            
            // Check the CsvRow values. They should equal the 1-based position in the
            // CSV, *including* the header row. So the first data row will have CsvRow
            // of 2. 
            Enumerable.Range(0, 16).ForEach(i => 
                Assert.Equal(i + 2, observations[i].CsvRow));
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
            .WithFiles("small-csv")
            .WithStatus(STAGE_3)
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
    
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);

        privateBlobStorageService.SetupStreamBlob(
            PrivateReleaseFiles,
            import.File.Path(),
            dataFilePath);

        privateBlobStorageService.SetupStreamBlob(
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
            new ImporterLocationService(
                guidGenerator,
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            importerMetaService,
            dataImportService,
            Mock.Of<ILogger<ImporterService>>(),
            databaseHelper,
            observationBatchImporter);

        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            privateBlobStorageService.Object,
            dataImportService,
            importerService);

        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            privateBlobStorageService.Object,
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

        VerifyAllMocks(privateBlobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has completed successfully.
            Assert.Equal(16, dataImport.ImportedRows);
            Assert.Equal(15, dataImport.LastProcessedRowIndex);
            Assert.Equal(COMPLETE, dataImport.Status);
            
            var observations = await statisticsDbContext
                .Observation
                .ToListAsync();
            
            Assert.Equal(16, observations.Count);
            
            // Check the CsvRow values. They should equal the 1-based position in the
            // CSV, *including* the header row. So the first data row will have CsvRow
            // of 2. 
            Enumerable.Range(0, 16).ForEach(i => 
                Assert.Equal(i + 2, observations[i].CsvRow));
        }
    }
    
    [Fact]
    public async Task ProcessStage3_IgnoredRows()
    {
        // This import expects only 8 out of 16 rows to be imported, as 8 of them are ignored.
        var import = _fixture
            .DefaultDataImport()
            .WithSubjectId(_subject.Id)
            .WithFiles("ignored-school-rows")
            .WithStatus(STAGE_3)
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
    
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);

        privateBlobStorageService.SetupStreamBlob(
            PrivateReleaseFiles,
            import.File.Path(),
            dataFilePath);

        privateBlobStorageService.SetupStreamBlob(
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
            new ImporterLocationService(
                guidGenerator,
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            importerMetaService,
            dataImportService,
            Mock.Of<ILogger<ImporterService>>(),
            databaseHelper,
            observationBatchImporter);

        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            privateBlobStorageService.Object,
            dataImportService,
            importerService);

        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            privateBlobStorageService.Object,
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

        VerifyAllMocks(privateBlobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has completed successfully.
            Assert.Equal(8, dataImport.ImportedRows);
            Assert.Equal(15, dataImport.LastProcessedRowIndex);
            Assert.Equal(COMPLETE, dataImport.Status);
            
            // Verify that only the LA-level data was imported and the School-level data ignored.
            statisticsDbContext
                .Observation
                .Include(o => o.Location)
                .ForEach(o => Assert.Equal(GeographicLevel.LocalAuthority, o.Location.GeographicLevel));

            var observations = await statisticsDbContext
                .Observation
                .ToListAsync();
            
            Assert.Equal(8, observations.Count);
            
            // Check the CsvRow values. They should equal the 1-based position in the
            // CSV, *including* the header row. So the first data row will have CsvRow
            // of 2.  However, only alternate rows are being imported in this scenario,
            // meaning that we expect to see the CsvRows 2, 4, 6, 8 etc up to 18. 
            Enumerable.Range(0, 8).ForEach(i => 
                Assert.Equal((i * 2) + 2, observations[i].CsvRow));
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
            .WithFiles("ignored-school-rows")
            .WithStatus(STAGE_3)
            .WithRowCounts(
                totalRows: 16,
                expectedImportedRows: 8,
                importedRows: 4,
                lastProcessedRowIndex: 6
            )
            .Generate();
        
        // Generate already-imported Observations with alternating CsvRow numbers
        // e.g. 2, 4, 6, 8
        var alreadyImportedObservations = _fixture
            .DefaultObservation()
            .WithLocation(_fixture
                .DefaultLocation()
                .WithGeographicLevel(GeographicLevel.LocalAuthority)
                .Generate())
            .WithSubject(_subject)
            .ForInstance(s => s.Set(o => o.CsvRow, 
                (_, _, context) => (context.Index * 2) + 2))
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
    
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);

        privateBlobStorageService.SetupStreamBlob(
            PrivateReleaseFiles,
            import.File.Path(),
            dataFilePath);

        privateBlobStorageService.SetupStreamBlob(
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
            new ImporterLocationService(
                guidGenerator,
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            importerMetaService,
            dataImportService,
            Mock.Of<ILogger<ImporterService>>(),
            databaseHelper,
            observationBatchImporter);

        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            privateBlobStorageService.Object,
            dataImportService,
            importerService);

        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            privateBlobStorageService.Object,
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

        VerifyAllMocks(privateBlobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has completed successfully.
            Assert.Equal(8, dataImport.ImportedRows);
            Assert.Equal(15, dataImport.LastProcessedRowIndex);
            Assert.Equal(COMPLETE, dataImport.Status);
            
            var observations = await statisticsDbContext
                .Observation
                .Include(o => o.Location)
                .ToListAsync();

            Assert.Equal(8, observations.Count);
            
            // Verify that only the LA-level data was imported and the School-level data ignored.
            observations.ForEach(o => 
                Assert.Equal(GeographicLevel.LocalAuthority, o.Location.GeographicLevel));
            
            // Check the CsvRow values. They should equal the 1-based position in the
            // CSV, *including* the header row. So the first data row will have CsvRow
            // of 2.  However, only alternate rows are being imported in this scenario,
            // meaning that we expect to see the CsvRows 2, 4, 6, 8 etc up to 18. 
            Enumerable.Range(0, 8).ForEach(i => 
                Assert.Equal((i * 2) + 2, observations[i].CsvRow));
        }
    }
    
    [Fact]
    public async Task ProcessStage3_Cancelling()
    {
        Environment.SetEnvironmentVariable("RowsPerBatch", "3");
        
        var import = _fixture
            .DefaultDataImport()
            .WithSubjectId(_subject.Id)
            .WithFiles("small-csv")
            .WithStatus(STAGE_3)
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
    
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);

        privateBlobStorageService.SetupStreamBlob(
            PrivateReleaseFiles,
            import.File.Path(),
            dataFilePath);

        privateBlobStorageService.SetupStreamBlob(
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

        var observationBatchImporterMock = new Mock<TestObservationBatchImporter>
        {
            CallBase = true
        };

        var importerService = new ImporterService(
            guidGenerator,
            new ImporterLocationService(
                guidGenerator,
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            importerMetaService,
            dataImportService,
            Mock.Of<ILogger<ImporterService>>(),
            databaseHelper,
            observationBatchImporterMock.Object);

        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            privateBlobStorageService.Object,
            dataImportService,
            importerService);

        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            privateBlobStorageService.Object,
            fileImportService,
            importerService,
            dataImportService,
            Mock.Of<IValidatorService>(Strict),
            Mock.Of<IDataArchiveService>(Strict),
            dbContextSupplier);

        var function = BuildFunction(
            processorService: processorService,
            dataImportService: dataImportService);

        // Cancel the import after the first batch of Observations is imported.
        observationBatchImporterMock
            .Setup(s =>
                s.ImportObservationBatch(
                    It.IsAny<StatisticsDbContext>(),
                    It.IsAny<IEnumerable<Observation>>()))
            .Callback(() => function.CancelImports(new CancelImportMessage(import.Id)));

        await function.ProcessUploads(
            new ImportMessage(import.Id),
            new ExecutionContext(),
            Mock.Of<ICollector<ImportMessage>>(Strict));

        VerifyAllMocks(privateBlobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import has been cancelled after the first 3 rows were imported.
            Assert.Equal(3, dataImport.ImportedRows);
            Assert.Equal(2, dataImport.LastProcessedRowIndex);
            Assert.Equal(CANCELLED, dataImport.Status);
            
            var observations = await statisticsDbContext
                .Observation
                .ToListAsync();

            Assert.Equal(3, observations.Count);
        }
    }
    
    [Fact]
    public async Task ProcessStage3_CancelledAlready()
    {
        Environment.SetEnvironmentVariable("RowsPerBatch", "3");
        
        var import = _fixture
            .DefaultDataImport()
            .WithSubjectId(_subject.Id)
            .WithFiles("small-csv")
            .WithStatus(CANCELLED)
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
    
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId: _contentDbContextId,
            statisticsDbContextId: _statisticsDbContextId);

        var databaseHelper = new InMemoryDatabaseHelper(dbContextSupplier);

        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>());

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
            new ImporterLocationService(
                guidGenerator,
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            importerMetaService,
            dataImportService,
            Mock.Of<ILogger<ImporterService>>(),
            databaseHelper,
            observationBatchImporter);

        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            privateBlobStorageService.Object,
            dataImportService,
            importerService);

        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            privateBlobStorageService.Object,
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

        VerifyAllMocks(privateBlobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import has not imported any rows as it was already cancelled.
            Assert.Equal(0, dataImport.ImportedRows);
            Assert.Null(dataImport.LastProcessedRowIndex);
            Assert.Equal(CANCELLED, dataImport.Status);
            
            var observations = await statisticsDbContext
                .Observation
                .ToListAsync();

            Assert.Empty(observations);
        }
    }
    
    [Fact]
    public async Task ProcessStage3_AdditionalFiltersAndIndicators()
    {
        var additionalFilter = _fixture
            .DefaultFilter()
            .WithSubject(_subject)
            .WithLabel("Additional filter")
            .WithFilterGroups(_fixture
                .DefaultFilterGroup()
                .WithFilterItems(_fixture
                    .DefaultFilterItem()
                    .ForIndex(1, s => s.SetLabel("Not specified"))
                    .Generate(3))
                .Generate(1))
            .Generate();

        // Add an additional Indicator to the existing Indicator Group
        var additionalIndicator = _fixture
            .DefaultIndicator()
            .WithIndicatorGroup(_subject.IndicatorGroups[0])
            .WithLabel("Additional indicator")
            .Generate();
        
        var import = _fixture
            .DefaultDataImport()
            .WithSubjectId(_subject.Id)
            .WithFiles("additional-filters-and-indicators")
            .WithStatus(STAGE_3)
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
            await statisticsDbContext.Filter.AddRangeAsync(additionalFilter);
            await statisticsDbContext.SaveChangesAsync();
        }
    
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);

        privateBlobStorageService.SetupStreamBlob(
            PrivateReleaseFiles,
            import.File.Path(),
            dataFilePath);

        privateBlobStorageService.SetupStreamBlob(
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
            new ImporterLocationService(
                guidGenerator,
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            importerMetaService,
            dataImportService,
            Mock.Of<ILogger<ImporterService>>(),
            databaseHelper,
            observationBatchImporter);

        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            privateBlobStorageService.Object,
            dataImportService,
            importerService);

        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            privateBlobStorageService.Object,
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

        VerifyAllMocks(privateBlobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has completed successfully.
            Assert.Equal(16, dataImport.ImportedRows);
            Assert.Equal(15, dataImport.LastProcessedRowIndex);
            Assert.Equal(COMPLETE, dataImport.Status);

            var observations = await statisticsDbContext
                .Observation
                .Include(o => o.FilterItems)
                .ToListAsync();
            
            Assert.Equal(16, observations.Count);
            
            // Check that the additional Filter's values are "Not specified" for each row.
            var notSpecifiedFilterItem = additionalFilter.FilterGroups[0].FilterItems[1];
            observations.ForEach(observation => 
                Assert.Equal(notSpecifiedFilterItem.Id, observation.FilterItems[1].FilterItemId));
            
            // Check that the additional Indicator's Measure is null for each row.
            observations.ForEach(observation => 
                Assert.Null(observation.Measures[additionalIndicator.Id]));
        }
    }
    
    [Fact]
    public async Task ProcessStage3_SpecialFilterItemAndIndicatorValues()
    {
        var subject = _fixture
            .DefaultSubject()
            .WithFilters(_fixture
                .DefaultFilter()
                .WithLabel("Filter one")
                .WithFilterGroups(_fixture
                    .DefaultFilterGroup()
                    .WithFilterItems(_fixture
                        .DefaultFilterItem()
                        
                        // Despite various variations of case and additional whitespace in the CSV values
                        // for this Filter, they will all resolve to a single "Value 1" Filter Item.
                        .ForIndex(0, s => s.SetLabel("Value 1"))
                        
                        // Blank values for this Filter in the CSV will be assigned the special "Not specified"
                        // Filter Item, as created during Stage 2 of the import.
                        .ForIndex(1, s => s.SetLabel("Not specified"))
                        
                        .GenerateList())
                    .Generate(1))
                .GenerateList(1))
            .WithIndicatorGroups(_fixture
                .DefaultIndicatorGroup()
                .WithIndicators(_fixture
                    .DefaultIndicator()
                    .WithLabel("Indicator one")
                    .GenerateList(1))
                .Generate(1))
            .Generate();
        
        var import = _fixture
            .DefaultDataImport()
            .WithSubjectId(subject.Id)
            .WithFiles("small-csv-with-special-data")
            .WithStatus(STAGE_3)
            .WithRowCounts(
                totalRows: 5,
                expectedImportedRows: 5
            )
            .Generate();
    
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
            
            await statisticsDbContext.Subject.AddAsync(subject);
            await statisticsDbContext.Location.AddRangeAsync(_locations);
            await statisticsDbContext.SaveChangesAsync();
        }
    
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.File.Filename);

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + import.MetaFile.Filename);

        privateBlobStorageService.SetupStreamBlob(
            PrivateReleaseFiles,
            import.File.Path(),
            dataFilePath);

        privateBlobStorageService.SetupStreamBlob(
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
            new ImporterLocationService(
                guidGenerator,
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            importerMetaService,
            dataImportService,
            Mock.Of<ILogger<ImporterService>>(),
            databaseHelper,
            observationBatchImporter);

        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            privateBlobStorageService.Object,
            dataImportService,
            importerService);

        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            privateBlobStorageService.Object,
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

        VerifyAllMocks(privateBlobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has completed successfully.
            Assert.Equal(5, dataImport.ImportedRows);
            Assert.Equal(4, dataImport.LastProcessedRowIndex);
            Assert.Equal(COMPLETE, dataImport.Status);

            var observations = await statisticsDbContext
                .Observation
                .Include(o => o.FilterItems)
                .ToListAsync();
            
            Assert.Equal(5, observations.Count);
            
            // Assert that every permutation of "Value 1" in the data CSV is trimmed and case-ignored 
            // to match with the appropriate available "Value 1" Filter Item.
            observations.GetRange(0, 4).ForEach(observation => 
                Assert.Equal(
                    subject.Filters[0].FilterGroups[0].FilterItems[0].Id,
                    observation.FilterItems[0].FilterItemId));
            
            // Assert that the blank value for Filter one in the last CSV row will be assigned the
            // special "Not specified" Filter Item.
            Assert.Equal(
                subject.Filters[0].FilterGroups[0].FilterItems[1].Id,
                observations[4].FilterItems[0].FilterItemId);
            
            // Assert that the special Indicator values are saved appropriately.
            Assert.Equal("", observations[0].Measures.Values.ToList()[0]);
            Assert.Equal("~", observations[1].Measures.Values.ToList()[0]);
            Assert.Equal("a", observations[2].Measures.Values.ToList()[0]);
            Assert.Equal(" ", observations[3].Measures.Values.ToList()[0]);
        }
    }
    
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class TestObservationBatchImporter : IObservationBatchImporter
    {
        public virtual async Task ImportObservationBatch(StatisticsDbContext context, IEnumerable<Observation> observations)
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
            Mock.Of<ILogger<Processor.Functions.Processor>>(),
            rethrowExceptions: true);
    }
}
