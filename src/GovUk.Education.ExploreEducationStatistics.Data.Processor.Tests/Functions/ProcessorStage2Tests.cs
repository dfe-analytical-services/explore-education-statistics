#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
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
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Functions;

public class ProcessorStage2Tests
{
    private readonly string _contentDbContextId = Guid.NewGuid().ToString();
    private readonly string _statisticsDbContextId = Guid.NewGuid().ToString();

    [Fact]
    public async Task ProcessStage2()
    {
        await AssertStage2ItemsImportedCorrectly(new OrderingCsvStage2Scenario());
    }
    
    [Fact]
    public async Task ProcessStage2_SubjectMetaAlreadyImported()
    {
        var subjectId = Guid.NewGuid();
        var scenario = new OrderingCsvStage2Scenario(subjectId);
        
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            // Save the Filters without any FilterGroups attached - the FilterGroups are added later as part of the 
            // CSV row-by-row scanning.
            var filtersWithoutGroups = scenario.GetExpectedFilters();
            filtersWithoutGroups.ForEach(filter => filter.FilterGroups = new List<FilterGroup>());
            await statisticsDbContext.Filter.AddRangeAsync(filtersWithoutGroups);
            
            // Save the IndicatorGroups too. This completes the data that is saved when atomically saving the
            // Subject Meta as the first step of Stage 2 import.
            await statisticsDbContext.IndicatorGroup.AddRangeAsync(scenario.GetExpectedIndicatorGroups());
            await statisticsDbContext.SaveChangesAsync();
        }
        
        // Now assert that the Stage 2 import completes successfully when the Subject Meta is already pre-existing. 
        await AssertStage2ItemsImportedCorrectly(scenario);
    }
    
    [Fact]
    public async Task ProcessStage2_AnotherImportOfSimilarData()
    {
        var importerFilterCache = new ImporterFilterCache();
        var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());
        
        // Firstly import a CSV.
        var subjectId1 = Guid.NewGuid();
        var scenario1 = new OrderingCsvStage2Scenario(subjectId1);
        await AssertStage2ItemsImportedCorrectly(
            scenario1, 
            importerFilterCache, 
            importerLocationCache);
        
        // Then import a very similar CSV.
        //
        // We would expect though that the import would result in new Filters, FilterGroups and FilterItems,
        // IndicatorGroups and Indicators specifically for this new Subject.
        //
        // We would not however expect to have additional Locations imported, as these should be shared between
        // different Subjects. This is tested implicitly as a result of this second call to
        // `AssertStage2ItemsImportedCorrectly` which tests that the same list of Locations is available
        // as in the first call to `AssertStage2ItemsImportedCorrectly`, thus showing that no additional Locations
        // were added as part of the second import.
        
        var subjectId2 = Guid.NewGuid();
        var scenario2 = new OrderingCsvStage2Scenario(subjectId2);
        await AssertStage2ItemsImportedCorrectly(
            scenario2, 
            importerFilterCache, 
            importerLocationCache);
    }
    
    [Fact]
    public async Task ProcessStage2_LocationsAlreadyExist()
    {
        var scenario = new OrderingCsvStage2Scenario();

        var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());
        
        // Persist the Locations that are already in the CSV to import.
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await statisticsDbContext.Location.AddRangeAsync(scenario.GetExpectedLocations());
            await statisticsDbContext.SaveChangesAsync();
            
            // Fill the ImporterLocationCache with all existing Locations on "startup" of the Importer.
            // Note that this occurs in Startup.cs.
            importerLocationCache.LoadLocations(statisticsDbContext);
        }
        
        // Import the CSV over the top of the existing Locations.
        await AssertStage2ItemsImportedCorrectly(
            scenario, 
            locationCache: importerLocationCache);
        
        // Check that the list of the Locations is the same as before.
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            var locations = await statisticsDbContext.Location.ToListAsync();
            
            Assert.Equal(scenario.GetExpectedLocations().Count, locations.Count);
            
            locations.ForEach(location =>
            {
                var cachedLocation = importerLocationCache.Get(location);
                Assert.Equal(location.Id, cachedLocation.Id);
            });
            
            // Blank out the ids from the stored Locations to make testing equality easier with our list of expected
            // Locations.
            locations.ForEach(location => location.Id = Guid.Empty);
            scenario.GetExpectedLocations().ForEach(expectedLocation => Assert.Contains(expectedLocation, locations));
        }
    }

    private async Task AssertStage2ItemsImportedCorrectly(
        IProcessorStage2TestScenario scenario,
        ImporterFilterCache? memoryCache = null,
        IImporterLocationCache? locationCache = null)
    {
        var importerFilterCache = memoryCache ?? new ImporterFilterCache();
        var importerLocationCache = locationCache ?? new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());
        
        var metaFileUnderTest = scenario.GetFilenameUnderTest().Replace(".csv", ".meta.csv");

        var subject = new Subject
        {
            Id = scenario.GetSubjectId()
        };

        var import = new DataImport
        {
            Id = Guid.NewGuid(),
            SubjectId = subject.Id,
            File = new File
            {
                Id = Guid.NewGuid(),
                Filename = scenario.GetFilenameUnderTest(),
                Type = FileType.Data
            },
            MetaFile = new File
            {
                Id = Guid.NewGuid(),
                Filename = metaFileUnderTest,
                Type = FileType.Metadata
            },
            TotalRows = 16,
            Status = DataImportStatus.STAGE_2
        };

        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await statisticsDbContext.Subject.AddAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + scenario.GetFilenameUnderTest());

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

        var transactionHelper = new InMemoryDatabaseHelper(dbContextSupplier);
        
        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>(),
            transactionHelper);
        
        var guidGenerator = new SequentialGuidGenerator();
        
        var importerService = new ImporterService(
            guidGenerator,
            new ImporterFilterService(importerFilterCache),
            new ImporterLocationService(
                guidGenerator, 
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            new ImporterMetaService(guidGenerator, transactionHelper),
            dataImportService,
            Mock.Of<ILogger<ImporterService>>(),
            transactionHelper,
            importerFilterCache);

        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            Mock.Of<IBatchService>(Strict),
            blobStorageService.Object,
            dataImportService,
            importerService,
            transactionHelper);

        var processorService = BuildProcessorService(
            dbContextSupplier,
            dataImportService: dataImportService,
            blobStorageService: blobStorageService.Object,
            importerService: importerService,
            fileImportService: fileImportService);
        
        var importMessage = new ImportMessage(import.Id);

        var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>(Strict);

        importStagesMessageQueue
            .Setup(s => s.Add(importMessage));

        var function = BuildFunction(processorService, dataImportService);
        
        await function.ProcessUploads(
            importMessage, 
            new ExecutionContext(),
            importStagesMessageQueue.Object,
            Mock.Of<ICollector<ImportObservationsMessage>>(Strict));
        
        VerifyAllMocks(blobStorageService, importStagesMessageQueue);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            var filters = await statisticsDbContext
                .Filter
                .Include(f => f.FilterGroups)
                .ThenInclude(fg => fg.FilterItems)
                .Where(f => f.SubjectId == scenario.GetSubjectId())
                .ToListAsync();

            var filterLabels = filters
                .Select(f => f.Label)
                .OrderBy(label => label)
                .JoinToString(",");

            var expectedFilterLabels = scenario
                .GetExpectedFilters()
                .Select(f => f.Label)
                .OrderBy(label => label)
                .JoinToString(",");
            
            Assert.Equal(expectedFilterLabels, filterLabels);
            
            var filterNames = filters
                .Select(f => f.Name)
                .OrderBy(name => name)
                .JoinToString(",");

            var expectedFilterNames = scenario
                .GetExpectedFilters()
                .Select(f => f.Name)
                .OrderBy(name => name)
                .JoinToString(",");
            
            Assert.Equal(expectedFilterNames, filterNames);
            
            filters.ForEach(filter => Assert.Equal(subject.Id, filter.SubjectId));

            scenario.GetExpectedFilters().ForEach(expectedFilter =>
            {
                var matchingFilter = filters.Single(f => f.Label == expectedFilter.Label);

                var filterGroupPrefix = $"Filter Groups for Filter {expectedFilter.Label}: ";

                var filterGroupLabels = matchingFilter
                    .FilterGroups
                    .Select(fg => fg.Label)
                    .OrderBy(label => label)
                    .JoinToString(",");

                var expectedFilterGroupLabels = expectedFilter
                    .FilterGroups
                    .Select(f => f.Label)
                    .OrderBy(label => label)
                    .JoinToString(",");

                Assert.Equal(filterGroupPrefix + expectedFilterGroupLabels, filterGroupPrefix + filterGroupLabels);

                expectedFilter.FilterGroups.ForEach(expectedFilterGroup =>
                {
                    var matchingFilterGroup = matchingFilter.FilterGroups.Single(f => f.Label == expectedFilterGroup.Label);

                    var filterIndexPrefix =
                        $"Filter Items for Filter Group {expectedFilterGroup.Label}, Filter {expectedFilter.Label}: ";

                    var filterItemLabels = matchingFilterGroup
                        .FilterItems
                        .Select(fg => fg.Label)
                        .OrderBy(label => label)
                        .JoinToString(",");

                    var expectedFilterItemLabels = expectedFilterGroup
                        .FilterItems
                        .Select(fg => fg.Label)
                        .OrderBy(label => label)
                        .JoinToString(",");

                    Assert.Equal(filterIndexPrefix + expectedFilterItemLabels, filterIndexPrefix + filterItemLabels);

                    var cachedFilterGroup = importerFilterCache.GetOrCacheFilterGroup(
                        matchingFilter.Id, 
                        matchingFilterGroup.Label,
                        () => null!);
                    
                    Assert.Equal(matchingFilterGroup.Id, cachedFilterGroup.Id);
                    
                    expectedFilterGroup.FilterItems.ForEach(expectedFilterItem =>
                    {
                        var matchingFilterItem = matchingFilterGroup
                            .FilterItems
                            .Single(f => f.Label == expectedFilterItem.Label);

                        var cachedFilterItem = importerFilterCache.GetOrCacheFilterItem(
                            matchingFilterGroup.Id, 
                            matchingFilterItem.Label,
                            () => null!);
                        
                        Assert.Equal(matchingFilterItem.Id, cachedFilterItem.Id);
                    });
                });
            });

            var indicatorGroups = await statisticsDbContext
                .IndicatorGroup
                .Include(ig => ig.Indicators)
                .Where(ig => ig.SubjectId == scenario.GetSubjectId())
                .ToListAsync();
            
            var indicatorGroupLabels = indicatorGroups.Select(ig => ig.Label).OrderBy(label => label);
            var expectedIndicatorGroupLabels = scenario.GetExpectedIndicatorGroups().Select(ig => ig.Label).OrderBy(label => label);
            Assert.Equal(expectedIndicatorGroupLabels, indicatorGroupLabels);

            indicatorGroups.ForEach(indicatorGroup => Assert.Equal(subject.Id, indicatorGroup.SubjectId));
            
            scenario.GetExpectedIndicatorGroups().ForEach(expectedIndicatorGroup =>
            {
                var matchingIndicatorGroup = indicatorGroups.Single(ig => ig.Label == expectedIndicatorGroup.Label);

                var indicatorPrefix = $"Indicators for Indicator Group {expectedIndicatorGroup.Label}: ";

                var indicatorLabels = matchingIndicatorGroup
                    .Indicators
                    .Select(i => i.Label)
                    .OrderBy(label => label)
                    .JoinToString(",");

                var expectedIndicatorLabels = expectedIndicatorGroup
                    .Indicators
                    .Select(i => i.Label)
                    .OrderBy(label => label)
                    .JoinToString(",");

                Assert.Equal(indicatorPrefix + expectedIndicatorLabels, indicatorPrefix + indicatorLabels);
                
            });

            var locations = await statisticsDbContext.Location.ToListAsync();
            
            Assert.Equal(scenario.GetExpectedLocations().Count, locations.Count);
            
            locations.ForEach(location =>
            {
                var cachedLocation = importerLocationCache.Get(location);
                Assert.Equal(location.Id, cachedLocation.Id);
            });
            
            // Blank out the ids from the stored Locations to make testing equality easier with our list of expected
            // Locations
            locations.ForEach(location => location.Id = Guid.Empty);
            scenario.GetExpectedLocations().ForEach(expectedLocation => Assert.Contains(expectedLocation, locations));
        }
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            var dataImports = await contentDbContext
                .DataImports
                .ToListAsync();
            
            // Verify that the import status has moved onto Stage 3.
            dataImports.ForEach(dataImport => Assert.Equal(DataImportStatus.STAGE_3, dataImport.Status));
        }
    }

    private static ProcessorService BuildProcessorService(
        IDbContextSupplier dbContextSupplier,
        IDataImportService? dataImportService = null,
        IBlobStorageService? blobStorageService = null,
        IImporterService? importerService = null,
        IFileImportService? fileImportService = null)
    {
        return new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(),
            blobStorageService ?? Mock.Of<IBlobStorageService>(Strict),
            fileImportService ?? Mock.Of<IFileImportService>(Strict),
            Mock.Of<ISplitFileService>(Strict),
            importerService ?? Mock.Of<IImporterService>(Strict),
            dataImportService ?? Mock.Of<IDataImportService>(Strict),
            Mock.Of<IValidatorService>(Strict),
            Mock.Of<IDataArchiveService>(Strict),
            dbContextSupplier);
    }
    
    

    private static Processor.Functions.Processor BuildFunction(
        IProcessorService? processorService = null,
        IDataImportService? dataImportService = null)
    {
        return new Processor.Functions.Processor(
            Mock.Of<IFileImportService>(Strict),
            dataImportService ?? Mock.Of<IDataImportService>(Strict),
            processorService ?? Mock.Of<IProcessorService>(Strict),
            Mock.Of<IDbContextSupplier>(),
            Mock.Of<ILogger<Processor.Functions.Processor>>());
    }
}