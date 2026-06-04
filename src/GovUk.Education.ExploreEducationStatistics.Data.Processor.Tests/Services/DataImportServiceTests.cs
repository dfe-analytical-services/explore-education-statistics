#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;

public class DataImportServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task GetImportStatus()
    {
        var import = new DataImport { Status = STAGE_1 };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        var service = BuildDataImportService(contentDbContextId: contentDbContextId);
        var result = await service.GetImportStatus(import.Id);

        Assert.Equal(STAGE_1, result);
    }

    [Fact]
    public async Task GetImportStatus_NotFound()
    {
        var service = BuildDataImportService();
        var result = await service.GetImportStatus(Guid.NewGuid());

        Assert.Equal(NOT_FOUND, result);
    }

    [Fact]
    public async Task GetImport()
    {
        var import = new DataImport
        {
            File = new File(),
            MetaFile = new File(),
            Status = STAGE_1,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        var service = BuildDataImportService(contentDbContextId: contentDbContextId);
        var result = await service.GetImport(import.Id);

        Assert.Equal(import.Id, result.Id);

        Assert.Empty(result.Errors);

        Assert.NotNull(import.File);
        Assert.Equal(import.File.Id, result.File.Id);

        Assert.NotNull(import.MetaFile);
        Assert.Equal(import.MetaFile.Id, result.MetaFile.Id);
    }

    [Fact]
    public async Task GetImport_ImportHasErrors()
    {
        var import = new DataImport
        {
            Errors = new List<DataImportError> { new("error 1"), new("error 2") },
            File = new File(),
            MetaFile = new File(),
            Status = STAGE_1,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        var service = BuildDataImportService(contentDbContextId: contentDbContextId);
        var result = await service.GetImport(import.Id);

        Assert.Equal(import.Id, result.Id);

        Assert.NotNull(result.Errors);
        Assert.Equal(2, result.Errors.Count);
        Assert.Equal("error 1", result.Errors[0].Message);
        Assert.Equal("error 2", result.Errors[1].Message);
    }

    [Fact]
    public async Task Update()
    {
        var import = new DataImport
        {
            ExpectedImportedRows = 1,
            ImportedRows = 1,
            TotalRows = 1,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        var service = BuildDataImportService(contentDbContextId);

        await service.Update(
            import.Id,
            expectedImportedRows: 3000,
            importedRows: 5000,
            totalRows: 10000,
            geographicLevels: new HashSet<GeographicLevel> { GeographicLevel.Country, GeographicLevel.Region }
        );

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var updated = await contentDbContext.DataImports.SingleAsync(i => i.Id == import.Id);

            Assert.Equal(3000, updated.ExpectedImportedRows);
            Assert.Equal(5000, updated.ImportedRows);
            Assert.Equal(10000, updated.TotalRows);

            Assert.Equal(2, updated.GeographicLevels.Count);
            Assert.Contains(GeographicLevel.Country, updated.GeographicLevels);
            Assert.Contains(GeographicLevel.Region, updated.GeographicLevels);
        }
    }

    [Fact]
    public async Task Update_Partial()
    {
        var import = new DataImport
        {
            ExpectedImportedRows = 1,
            ImportedRows = 1,
            TotalRows = 1,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        var service = BuildDataImportService(contentDbContextId);

        await service.Update(
            import.Id,
            importedRows: 5000,
            geographicLevels: new HashSet<GeographicLevel> { GeographicLevel.Country, GeographicLevel.Region }
        );

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var updated = await contentDbContext.DataImports.SingleAsync(i => i.Id == import.Id);

            Assert.Equal(1, updated.ExpectedImportedRows);
            Assert.Equal(5000, updated.ImportedRows);
            Assert.Equal(1, updated.TotalRows);

            Assert.Equal(2, updated.GeographicLevels.Count);
            Assert.Contains(GeographicLevel.Country, updated.GeographicLevels);
            Assert.Contains(GeographicLevel.Region, updated.GeographicLevels);
        }
    }

    [Fact]
    public async Task WriteDataSetMetaFile_Success()
    {
        var totalRows = 9393;

        var subject = _fixture.DefaultSubject().Generate();

        var file = _fixture
            .DefaultFile(FileType.Data)
            .WithDataSetFileMeta(null)
            .WithDataSetFileVersionGeographicLevels([])
            .WithSubjectId(subject.Id)
            .Generate();

        var observation1 = _fixture
            .DefaultObservation()
            .WithSubject(subject)
            .WithLocation(new Location { GeographicLevel = GeographicLevel.Country })
            .WithTimePeriod(2000, April)
            .Generate();

        var observation2 = _fixture
            .DefaultObservation()
            .WithSubject(subject)
            .WithLocation(new Location { GeographicLevel = GeographicLevel.LocalAuthority })
            .WithTimePeriod(2001, May)
            .Generate();

        var observation3 = _fixture
            .DefaultObservation()
            .WithSubject(subject)
            .WithLocation(new Location { GeographicLevel = GeographicLevel.Region })
            .WithTimePeriod(2002, June)
            .Generate();

        var filter = new Filter
        {
            SubjectId = subject.Id,
            Id = Guid.NewGuid(),
            Label = "Filter label",
            Hint = "Filter hint",
            Name = "filter_column_name",
        };

        var indicatorGroup = new IndicatorGroup
        {
            SubjectId = subject.Id,
            Indicators = new List<Indicator>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Indicator label",
                    Name = "indicator_column_name",
                },
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Files.Add(file);
            await contentDbContext.SaveChangesAsync();
        }

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.Observation.AddRange(observation1, observation2, observation3);
            statisticsDbContext.Filter.Add(filter);
            statisticsDbContext.IndicatorGroup.Add(indicatorGroup);
            await statisticsDbContext.SaveChangesAsync();
        }

        var service = BuildDataImportService(contentDbContextId, statisticsDbContextId);

        await service.WriteDataSetFileMeta(file.Id, subject.Id, totalRows);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var updatedFile = contentDbContext
                .Files.Include(f => f.DataSetFileVersionGeographicLevels)
                .Single(f => f.SubjectId == subject.Id);

            var geogLvls = updatedFile.DataSetFileVersionGeographicLevels.Select(gl => gl.GeographicLevel).ToList();
            Assert.Equal(3, geogLvls.Count);
            Assert.Contains(GeographicLevel.Country, geogLvls);
            Assert.Contains(GeographicLevel.LocalAuthority, geogLvls);
            Assert.Contains(GeographicLevel.Region, geogLvls);

            Assert.NotNull(updatedFile.DataSetFileMeta);
            var meta = updatedFile.DataSetFileMeta;

            Assert.Equal(totalRows, meta.NumDataFileRows);

            Assert.Equal("2000", meta.TimePeriodRange.Start.Period);
            Assert.Equal(April, meta.TimePeriodRange.Start.TimeIdentifier);
            Assert.Equal("2002", meta.TimePeriodRange.End.Period);
            Assert.Equal(June, meta.TimePeriodRange.End.TimeIdentifier);

            var dbFilter = Assert.Single(meta.Filters);
            Assert.Equal(filter.Id, dbFilter.Id);
            Assert.Equal(filter.Label, dbFilter.Label);
            Assert.Equal(filter.Name, dbFilter.ColumnName);

            var dbIndicator = Assert.Single(meta.Indicators);
            Assert.Equal(indicatorGroup.Indicators[0].Id, dbIndicator.Id);
            Assert.Equal(indicatorGroup.Indicators[0].Label, dbIndicator.Label);
            Assert.Equal(indicatorGroup.Indicators[0].Name, dbIndicator.ColumnName);

            Assert.NotNull(updatedFile.FilterHierarchies);
            Assert.Empty(updatedFile.FilterHierarchies);
        }
    }

    [Fact]
    public async Task GenerateFilterHierarchies_Success()
    {
        var subject = new Subject { Id = Guid.NewGuid() };

        var filters = new List<Filter>
        {
            new()
            {
                SubjectId = subject.Id,
                Name = "root_filter",
                FilterGroups =
                [
                    new FilterGroup
                    {
                        Label = "Root filter group",
                        FilterItems =
                        [
                            new FilterItem { Label = "RootFilterItem0" },
                            new FilterItem { Label = "RootFilterItem1" },
                        ],
                    },
                ],
                ParentFilter = null,
            },
            new()
            {
                SubjectId = subject.Id,
                Name = "child_filter1",
                FilterGroups =
                [
                    new FilterGroup
                    {
                        Label = "Child filter 1 group",
                        FilterItems =
                        [
                            new FilterItem { Label = "ChildFilter1Item0" },
                            new FilterItem { Label = "ChildFilter1Item1" },
                            new FilterItem { Label = "ChildFilter1Item2" },
                        ],
                    },
                ],
                ParentFilter = "root_filter",
            },
            new()
            {
                SubjectId = subject.Id,
                Name = "child_filter2",
                FilterGroups =
                [
                    new FilterGroup
                    {
                        Label = "Child filter 2 group",
                        FilterItems =
                        [
                            new FilterItem { Label = "ChildFilter2Item0" },
                            new FilterItem { Label = "ChildFilter2Item1" },
                            new FilterItem { Label = "ChildFilter2Item2" },
                            new FilterItem { Label = "ChildFilter2Item3" },
                            new FilterItem { Label = "ChildFilter2Item4" },
                            new FilterItem { Label = "ChildFilter2Item5" },
                        ],
                    },
                ],
                ParentFilter = "child_filter1",
            },
        };

        var rootFilterItem0 = filters[0].FilterGroups[0].FilterItems[0];
        var rootFilterItem1 = filters[0].FilterGroups[0].FilterItems[1];
        var childFilter1Item0 = filters[1].FilterGroups[0].FilterItems[0];
        var childFilter1Item1 = filters[1].FilterGroups[0].FilterItems[1];
        var childFilter1Item2 = filters[1].FilterGroups[0].FilterItems[2];
        var childFilter2Item0 = filters[2].FilterGroups[0].FilterItems[0];
        var childFilter2Item1 = filters[2].FilterGroups[0].FilterItems[1];
        var childFilter2Item2 = filters[2].FilterGroups[0].FilterItems[2];
        var childFilter2Item3 = filters[2].FilterGroups[0].FilterItems[3];
        var childFilter2Item4 = filters[2].FilterGroups[0].FilterItems[4];
        var childFilter2Item5 = filters[2].FilterGroups[0].FilterItems[5];

        var observation0 = _fixture
            .DefaultObservation()
            .WithSubject(subject)
            .WithFilterItems([rootFilterItem0, childFilter1Item0, childFilter2Item0]);

        var observation1 = _fixture
            .DefaultObservation()
            .WithSubject(subject)
            .WithFilterItems([rootFilterItem1, childFilter1Item1, childFilter2Item1]);

        var observation2 = _fixture
            .DefaultObservation()
            .WithSubject(subject)
            .WithFilterItems([rootFilterItem1, childFilter1Item1, childFilter2Item2]);

        var observation3 = _fixture
            .DefaultObservation()
            .WithSubject(subject)
            .WithFilterItems([rootFilterItem1, childFilter1Item2, childFilter2Item3]);

        var observation4 = _fixture
            .DefaultObservation()
            .WithSubject(subject)
            .WithFilterItems([rootFilterItem1, childFilter1Item2, childFilter2Item4]);

        var observation5 = _fixture
            .DefaultObservation()
            .WithSubject(subject)
            .WithFilterItems([rootFilterItem1, childFilter1Item2, childFilter2Item5]);

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.Filter.AddRange(filters);
            statisticsDbContext.Observation.AddRange(
                observation0,
                observation1,
                observation2,
                observation3,
                observation4,
                observation5
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var results = await DataImportService.GenerateFilterHierarchies(
                statisticsDbContext,
                filters
                    .Select(f => new FilterMeta
                    {
                        Id = f.Id,
                        Label = f.Label,
                        Hint = f.Hint,
                        ColumnName = f.Name,
                        ParentFilter = f.ParentFilter,
                    })
                    .ToList()
            );

            var hierarchy = Assert.Single(results);
            Assert.Equal([filters[0].Id, filters[1].Id, filters[2].Id], hierarchy.FilterIds);

            Assert.Equal(2, hierarchy.Tiers.Count);

            // tier 0
            Assert.Equal(2, hierarchy.Tiers[0].Count); // two root filter items
            Assert.Equal([rootFilterItem0.Id, rootFilterItem1.Id], hierarchy.Tiers[0].Keys.ToList());
            Assert.Equal([childFilter1Item0.Id], hierarchy.Tiers[0][rootFilterItem0.Id]);
            Assert.Equal([childFilter1Item1.Id, childFilter1Item2.Id], hierarchy.Tiers[0][rootFilterItem1.Id]);

            // tier 1
            Assert.Equal(3, hierarchy.Tiers[1].Count); // three child filter 1 items
            Assert.Equal(
                [childFilter1Item0.Id, childFilter1Item1.Id, childFilter1Item2.Id],
                hierarchy.Tiers[1].Keys.ToList()
            );
            Assert.Equal([childFilter2Item0.Id], hierarchy.Tiers[1][childFilter1Item0.Id]);
            Assert.Equal([childFilter2Item1.Id, childFilter2Item2.Id], hierarchy.Tiers[1][childFilter1Item1.Id]);
            Assert.Equal(
                [childFilter2Item3.Id, childFilter2Item4.Id, childFilter2Item5.Id],
                hierarchy.Tiers[1][childFilter1Item2.Id]
            );
        }
    }

    [Fact]
    public async Task GenerateFilterHierarchies_TwoRootFilters_Success()
    {
        var subject = new Subject { Id = Guid.NewGuid() };

        var filters = new List<Filter>
        {
            new()
            {
                SubjectId = subject.Id,
                Name = "root_filter0",
                FilterGroups =
                [
                    new FilterGroup
                    {
                        Label = "Root filter 0 group",
                        FilterItems = [new FilterItem { Label = "RootFilter0Item0" }],
                    },
                ],
                ParentFilter = null,
            },
            new()
            {
                SubjectId = subject.Id,
                Name = "child_filter0",
                FilterGroups =
                [
                    new FilterGroup
                    {
                        Label = "Child filter 0 group",
                        FilterItems = [new FilterItem { Label = "ChildFilter0Item0" }],
                    },
                ],
                ParentFilter = "root_filter0",
            },
            new()
            {
                SubjectId = subject.Id,
                Name = "root_filter1",
                FilterGroups =
                [
                    new FilterGroup
                    {
                        Label = "Root filter 1 group",
                        FilterItems = [new FilterItem { Label = "RootFilter1Item0" }],
                    },
                ],
                ParentFilter = null,
            },
            new()
            {
                SubjectId = subject.Id,
                Name = "child_filter1",
                FilterGroups =
                [
                    new FilterGroup
                    {
                        Label = "Child filter 1 group",
                        FilterItems = [new FilterItem { Label = "ChildFilter1Item0" }],
                    },
                ],
                ParentFilter = "root_filter1",
            },
        };

        var rootFilter0Item0 = filters[0].FilterGroups[0].FilterItems[0];
        var childFilter0Item0 = filters[1].FilterGroups[0].FilterItems[0];
        var rootFilter1Item0 = filters[2].FilterGroups[0].FilterItems[0];
        var childFilter1Item0 = filters[3].FilterGroups[0].FilterItems[0];

        var observation0 = _fixture
            .DefaultObservation()
            .WithSubject(subject)
            .WithFilterItems([rootFilter0Item0, childFilter0Item0]);

        var observation1 = _fixture
            .DefaultObservation()
            .WithSubject(subject)
            .WithFilterItems([rootFilter1Item0, childFilter1Item0]);

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.Filter.AddRange(filters);
            statisticsDbContext.Observation.AddRange(observation0, observation1);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var results = await DataImportService.GenerateFilterHierarchies(
                statisticsDbContext,
                filters
                    .Select(f => new FilterMeta
                    {
                        Id = f.Id,
                        Label = f.Label,
                        Hint = f.Hint,
                        ColumnName = f.Name,
                        ParentFilter = f.ParentFilter,
                    })
                    .ToList()
            );

            Assert.Equal(2, results.Count);

            // hierarchy for root filter 0
            var hierarchy0 = results[0];

            Assert.Equal([filters[0].Id, filters[1].Id], hierarchy0.FilterIds);

            Assert.Single(hierarchy0.Tiers);

            Assert.Single(hierarchy0.Tiers[0]); // single root filter 0 item
            Assert.Equal([rootFilter0Item0.Id], hierarchy0.Tiers[0].Keys.ToList());
            Assert.Equal([childFilter0Item0.Id], hierarchy0.Tiers[0][rootFilter0Item0.Id]);

            // hierarchy for root filter 1
            var hierarchy1 = results[1];

            Assert.Equal([filters[2].Id, filters[3].Id], hierarchy1.FilterIds);

            Assert.Single(hierarchy1.Tiers);

            Assert.Single(hierarchy1.Tiers[0]); // single root filter 1 item
            Assert.Equal([rootFilter1Item0.Id], hierarchy1.Tiers[0].Keys.ToList());
            Assert.Equal([childFilter1Item0.Id], hierarchy1.Tiers[0][rootFilter1Item0.Id]);
        }
    }

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_GenerateMapping_Indicators_Success()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var originalSubjectId = Guid.NewGuid();
        var replacementSubjectId = Guid.NewGuid();

        var originalFile = new File
        {
            Id = originalDataFileId,
            SubjectId = originalSubjectId,
            Type = FileType.Data,
        };

        var replacementFile = new File
        {
            Id = replacementDataFileId,
            SubjectId = replacementSubjectId,
            Type = FileType.Data,
            Replacing = originalFile,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Files.AddRange(originalFile, replacementFile);
            await contentDbContext.SaveChangesAsync();
        }

        var originalIndicatorA1 = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A1 - to be removed",
            Name = "indicator_a1",
        };

        var originalIndicatorA2 = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A2",
            Name = "indicator_a2",
        };

        var originalIndicatorA4 = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A4 - will move to a different group and so not be automapped",
            Name = "indicator_a4",
        };

        // No original A3, as it will be a new indicator in the replacement

        var originalIndicatorGroupA = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Group A",
            SubjectId = originalSubjectId,
            Indicators = [originalIndicatorA1, originalIndicatorA2, originalIndicatorA4],
        };

        var replacementIndicatorA2 = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A2",
            Name = "indicator_a2",
        };

        var replacementIndicatorA3 = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A3 - new",
            Name = "indicator_a3",
        };

        var replacementIndicatorGroupA = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Group A",
            SubjectId = replacementSubjectId,
            Indicators = [replacementIndicatorA2, replacementIndicatorA3],
        };

        var replacementIndicatorA4 = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A4 - moved from Group A to new Group B",
            Name = "indicator_a4",
        };

        var replacementIndicatorGroupB = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Group B",
            SubjectId = replacementSubjectId,
            Indicators = [replacementIndicatorA4],
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.IndicatorGroup.AddRange(
                originalIndicatorGroupA,
                replacementIndicatorGroupA,
                replacementIndicatorGroupB
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        var service = BuildDataImportService(contentDbContextId, statisticsDbContextId);

        await service.CreateInitialDataSetMappingIfReplacement(replacementFileId: replacementDataFileId);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var result = contentDbContext.DataSetMappings.Single(m =>
                m.OriginalDataFileId == originalDataFileId && m.ReplacementDataFileId == replacementDataFileId
            );

            var expectedMapping = new DataSetMapping
            {
                OriginalDataFileId = originalDataFileId,
                ReplacementDataFileId = replacementDataFileId,
                IndicatorMappings = new Dictionary<Guid, IndicatorMapping>
                {
                    {
                        originalIndicatorA1.Id,
                        CreateIndicatorMapping(originalIndicatorA1, originalIndicatorGroupA, mapStatus: MapStatus.Unset)
                    },
                    {
                        originalIndicatorA2.Id,
                        CreateIndicatorMapping(
                            originalIndicatorA2,
                            originalIndicatorGroupA,
                            replacementIndicatorA2,
                            replacementIndicatorGroupA,
                            mapStatus: MapStatus.AutoSet
                        )
                    },
                    {
                        originalIndicatorA4.Id,
                        CreateIndicatorMapping(originalIndicatorA4, originalIndicatorGroupA, mapStatus: MapStatus.Unset)
                    },
                },
                UnmappedReplacementIndicators =
                [
                    new UnmappedIndicator
                    {
                        Id = replacementIndicatorA3.Id,
                        Label = replacementIndicatorA3.Label,
                        ColumnName = replacementIndicatorA3.Name,
                        GroupId = replacementIndicatorGroupA.Id,
                        GroupLabel = replacementIndicatorGroupA.Label,
                    },
                    new UnmappedIndicator
                    {
                        Id = replacementIndicatorA4.Id,
                        Label = replacementIndicatorA4.Label,
                        ColumnName = replacementIndicatorA4.Name,
                        GroupId = replacementIndicatorGroupB.Id,
                        GroupLabel = replacementIndicatorGroupB.Label,
                    },
                ],
            };

            result.AssertDeepEqualTo(expectedMapping, ignoreProperties: [mapping => mapping.Id]);
        }
    }

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_ReplacementHasDifferentLocation_LocationMatchedByCode()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId,
        };

        var replacementFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = originalFile };

        var replacementReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = replacementFile };

        var originalFilterItem = new FilterItem { Id = Guid.NewGuid(), Label = "Test filter item - not changing" };

        var replacementFilterItem = new FilterItem { Label = "Test filter item - not changing" };

        var originalFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { originalFilterItem },
        };

        var replacementFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { replacementFilterItem },
        };

        var originalFilter = new Filter
        {
            Label = "Filter - not changing",
            Name = "filter_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { originalFilterGroup },
        };

        var replacementFilter = new Filter
        {
            Label = "Filter - not changing",
            Name = "filter_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementFilterGroup },
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var replacementIndicator = new Indicator
        {
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicator },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicator },
        };

        Country england = new("E92000001", "England");
        LocalAuthority derby = new("E06000015", "", "Derby");

        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = derby,
        };
        var observationForOriginalLocation = new Observation
        {
            SubjectId = originalReleaseSubject.SubjectId,
            Location = originalLocation,
        };

        // Replacement location has a different id but the primary attribute code remains the same
        var replacementLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Country = england,
            LocalAuthority = derby,
        };
        var observationForReplacementLocation = new Observation
        {
            SubjectId = replacementReleaseSubject.SubjectId,
            Location = replacementLocation,
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[] { originalFilterItem.Id },
                Indicators = new[] { originalIndicator.Id },
                LocationIds = [originalLocation.Id],
                TimePeriod = timePeriod,
            },
            ReleaseVersion = releaseVersion,
        };

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)> { (2019, CalendarYear), (2020, CalendarYear) }
            );

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject, replacementReleaseSubject);
            statisticsDbContext.Filter.AddRange(originalFilter, replacementFilter);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup, replacementIndicatorGroup);
            statisticsDbContext.Location.AddRange(originalLocation, replacementLocation);
            statisticsDbContext.Observation.AddRange(observationForOriginalLocation, observationForReplacementLocation);
            await statisticsDbContext.SaveChangesAsync();
        }

        var dataImportService = BuildDataImportService(contentDbContextId, statisticsDbContextId);

        await dataImportService.CreateInitialDataSetMappingIfReplacement(replacementFile.Id);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var dbDataSetMapping = contentDbContext.DataSetMappings.Single(m =>
                m.OriginalDataFileId == originalFile.Id && m.ReplacementDataFileId == replacementFile.Id
            );

            Assert.Equal(
                new Dictionary<Guid, LocationMapping>
                {
                    {
                        originalLocation.Id,
                        CreateLocationMapping(originalLocation, replacementLocation, MapStatus.AutoSet)
                    },
                },
                dbDataSetMapping.LocationMappings
            );

            Assert.Empty(dbDataSetMapping.UnmappedReplacementLocations);
        }
    }

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_NoMappingCreatedIfNotReplacement_Success()
    {
        var file = new File
        {
            Id = Guid.NewGuid(),
            Replacing = null, // a DataSetMapping is only created if a replacement is ongoing
            Type = FileType.Data,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Add(file);
            await contentDbContext.SaveChangesAsync();
        }

        var statisticsDbContextId = Guid.NewGuid().ToString();
        var service = BuildDataImportService(
            contentDbContextId: contentDbContextId,
            statisticsDbContextId: statisticsDbContextId
        );

        await service.CreateInitialDataSetMappingIfReplacement(replacementFileId: file.Id);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            Assert.False(contentDbContext.DataSetMappings.Any());
        }
    }

    private static DataImportService BuildDataImportService(
        string? contentDbContextId = null,
        string? statisticsDbContextId = null
    )
    {
        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId ?? Guid.NewGuid().ToString(),
            statisticsDbContextId ?? Guid.NewGuid().ToString()
        );

        return new DataImportService(dbContextSupplier, Mock.Of<ILogger<DataImportService>>(Strict));
    }

    private static IndicatorMapping CreateIndicatorMapping(
        Indicator original,
        IndicatorGroup originalGroup,
        Indicator? replacement = null,
        IndicatorGroup? replacementGroup = null,
        MapStatus mapStatus = MapStatus.Unset
    )
    {
        return new IndicatorMapping
        {
            OriginalId = original.Id,
            OriginalColumnName = original.Name,
            OriginalLabel = original.Label,
            OriginalGroupId = originalGroup.Id,
            OriginalGroupLabel = originalGroup.Label,
            ReplacementId = replacement?.Id,
            ReplacementColumnName = replacement?.Name,
            ReplacementLabel = replacement?.Label,
            ReplacementGroupId = replacementGroup?.Id,
            ReplacementGroupLabel = replacementGroup?.Label,
            Status = mapStatus,
        };
    }

    private static LocationMapping CreateLocationMapping(
        Location original,
        Location? replacement = null,
        MapStatus status = MapStatus.Unset
    )
    {
        return new LocationMapping
        {
            OriginalId = original.Id,
            OriginalCode = original.ToLocationAttribute().Code!,
            OriginalName = original.ToLocationAttribute().Name!,
            OriginalGeographicLevel = original.GeographicLevel,
            ReplacementId = replacement?.Id,
            ReplacementCode = replacement?.ToLocationAttribute().Code!,
            ReplacementName = replacement?.ToLocationAttribute().Name!,
            ReplacementGeographicLevel = replacement?.GeographicLevel,
            Status = status,
        };
    }
}
