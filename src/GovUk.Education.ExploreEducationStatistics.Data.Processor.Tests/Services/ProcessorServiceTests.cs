#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;

public class ProcessorServiceTests
{
    private readonly string _contentDbContextId = Guid.NewGuid().ToString();
    private readonly string _statisticsDbContextId = Guid.NewGuid().ToString();

    [Fact]
    public async Task ProcessStage2()
    {
        await AssertStage2ItemsImportedCorrectly(new OrderingCsvScenario());
    }
    
    [Fact]
    public async Task ProcessStage2_SubjectMetaAlreadyImported()
    {
        var subjectId = Guid.NewGuid();
        var scenario = new OrderingCsvScenario(subjectId);
        
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

    private async Task AssertStage2ItemsImportedCorrectly(IProcessorServiceTestScenario scenario)
    {
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
                Filename = scenario.GetFilenameUnderTest()
            },
            MetaFile = new File
            {
                Id = Guid.NewGuid(),
                Filename = metaFileUnderTest
            }
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

        blobStorageService
            .Setup(s => s.StreamBlob(PrivateReleaseFiles, import.File.Path(), null))
            .ReturnsAsync(() => System.IO.File.OpenRead(dataFilePath));

        blobStorageService
            .Setup(s => s.StreamBlob(PrivateReleaseFiles, import.MetaFile.Path(), null))
            .ReturnsAsync(() => System.IO.File.OpenRead(metaFilePath));

        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId: _contentDbContextId,
            statisticsDbContextId: _statisticsDbContextId);
        
        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>());

        // TODO DW - verify this
        var importerMemoryCache = new Mock<ImporterMemoryCache>(Strict);

        var guidGenerator = new SequentialGuidGenerator();

        var transactionHelper = new InMemoryTransactionHelper();
        
        var importerService = new ImporterService(
            guidGenerator,
            new ImporterFilterService(importerMemoryCache.Object),
            new ImporterLocationService(importerMemoryCache.Object, guidGenerator),
            new ImporterMetaService(guidGenerator, transactionHelper),
            dataImportService,
            Mock.Of<ILogger<ImporterService>>(),
            transactionHelper);

        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            Mock.Of<IBatchService>(Strict),
            blobStorageService.Object,
            dataImportService,
            importerService,
            transactionHelper);

        var service = BuildService(
            dbContextSupplier,
            dataImportService: dataImportService,
            blobStorageService: blobStorageService.Object,
            importerService: importerService,
            fileImportService: fileImportService);

        await service.ProcessStage2(import.Id);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            var filters = await statisticsDbContext
                .Filter
                .Include(f => f.FilterGroups)
                .ThenInclude(fg => fg.FilterItems)
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
                });
            });

            var indicatorGroups = await statisticsDbContext
                .IndicatorGroup
                .Include(ig => ig.Indicators)
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
            
            // Blank out the ids from the stored Locations to make testing equality easier with our list of expected
            // Locations
            locations.ForEach(location => location.Id = Guid.Empty);
            Assert.Equal(scenario.GetExpectedLocations().Count, locations.Count);
            scenario.GetExpectedLocations().ForEach(expectedLocation => Assert.Contains(expectedLocation, locations));
        }
    }

    private static ProcessorService BuildService(
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
}