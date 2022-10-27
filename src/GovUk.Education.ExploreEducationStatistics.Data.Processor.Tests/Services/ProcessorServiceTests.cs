#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;

public class ProcessorServiceTests
{
    [Fact]
    public async Task ProcessStage2()
    {
        const string fileUnderTest = "ordering-test-4.csv";
        
        var expectedFilters = ListOf(
            new Filter
            {
                Label = "Filter one",
                FilterGroups = ListOf(
                    new FilterGroup
                    {
                        Label = "Default",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "Total"
                            })
                    })
            },
            new Filter
            {
                Label = "Filter two",
                FilterGroups = ListOf(
                    new FilterGroup
                    {
                        Label = "One group",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "One"
                            })
                    },
                    new FilterGroup
                    {
                        Label = "Two group",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "Two"
                            })
                    }
                )
            },
            new Filter
            {
                Label = "Filter three",
                FilterGroups = ListOf(
                    new FilterGroup
                    {
                        Label = "Default",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "Total"
                            })
                    }
                )
            },
            new Filter
            {
                Label = "Filter four",
                FilterGroups = ListOf(
                    new FilterGroup
                    {
                        Label = "One group",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "One"
                            },
                            new FilterItem
                            {
                                Label = "Two"
                            })
                    },
                    new FilterGroup
                    {
                        Label = "Two group",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "One"
                            },
                            new FilterItem
                            {
                                Label = "Two"
                            })
                    }
                )
            });

        var expectedIndicatorGroups = ListOf(
            new IndicatorGroup
            {
                Label = "Default",
                Indicators = ListOf(
                    new Indicator
                    {
                        Label = "Indicator one"
                    })
            });

        var expectedLocations = ListOf(
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
            });
        
        await AssertStage2ItemsImportedCorrectly(
            fileUnderTest, 
            expectedFilters, 
            expectedIndicatorGroups,
            expectedLocations);
    }

    private static async Task AssertStage2ItemsImportedCorrectly(
        string fileUnderTest,
        List<Filter> expectedFilters,
        List<IndicatorGroup> expectedIndicatorGroups, 
        List<Location> expectedLocations)
    {
        var metaFileUnderTest = fileUnderTest.Replace(".csv", ".meta.csv");

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

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
                Filename = fileUnderTest
            },
            MetaFile = new File
            {
                Id = Guid.NewGuid(),
                Filename = metaFileUnderTest
            }
        };

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Subject.AddAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + fileUnderTest);

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + metaFileUnderTest);

        blobStorageService
            .Setup(s => s.StreamBlob(PrivateReleaseFiles, import.File.Path(), null))
            .ReturnsAsync(() => System.IO.File.OpenRead(dataFilePath));

        blobStorageService
            .Setup(s => s.StreamBlob(PrivateReleaseFiles, import.MetaFile.Path(), null))
            .ReturnsAsync(() => System.IO.File.OpenRead(metaFilePath));

        var dbContextSupplier = new InMemoryDbContextSupplier(contentDbContextId);
        
        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>());

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

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var filters = await statisticsDbContext
                .Filter
                .Include(f => f.FilterGroups)
                .ThenInclude(fg => fg.FilterItems)
                .ToListAsync();

            var filterLabels = filters.Select(f => f.Label).OrderBy(label => label);
            var expectedFilterLabels = expectedFilters.Select(f => f.Label).OrderBy(label => label);
            Assert.Equal(expectedFilterLabels, filterLabels);

            expectedFilters.ForEach(expectedFilter =>
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
            var expectedIndicatorGroupLabels = expectedIndicatorGroups.Select(ig => ig.Label).OrderBy(label => label);
            Assert.Equal(expectedIndicatorGroupLabels, indicatorGroupLabels);

            expectedIndicatorGroups.ForEach(expectedIndicatorGroup =>
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
            Assert.Equal(expectedLocations.Count, locations.Count);
            expectedLocations.ForEach(expectedLocation => Assert.Contains(expectedLocation, locations));
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