#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services
{
    public class DataImportServiceTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task GetImportStatus()
        {
            var import = new DataImport
            {
                Status = STAGE_1
            };

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
                ZipFile = new File(),
                Status = STAGE_1
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

            Assert.NotNull(import.ZipFile);
            Assert.Equal(import.ZipFile.Id, result.ZipFile!.Id);
        }

        [Fact]
        public async Task GetImport_ImportHasErrors()
        {
            var import = new DataImport
            {
                Errors = new List<DataImportError>
                {
                    new("error 1"),
                    new("error 2")
                },
                File = new File(),
                MetaFile = new File(),
                ZipFile = new File(),
                Status = STAGE_1
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
                TotalRows = 1
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
                geographicLevels: new HashSet<GeographicLevel>
                {
                    GeographicLevel.Country,
                    GeographicLevel.Region
                });

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
                TotalRows = 1
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
                geographicLevels: new HashSet<GeographicLevel>
                {
                    GeographicLevel.Country,
                    GeographicLevel.Region
                });

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
            var subject = _fixture.DefaultSubject()
                .Generate();

            var file = _fixture.DefaultFile(FileType.Data)
                .WithDataSetFileMeta(null)
                .WithDataSetFileVersionGeographicLevels([])
                .WithSubjectId(subject.Id)
                .Generate();

            var observation1 = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithLocation(new Location { GeographicLevel = GeographicLevel.Country })
                .WithTimePeriod(2000, TimeIdentifier.April)
                .Generate();

            var observation2 = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithLocation(new Location { GeographicLevel = GeographicLevel.LocalAuthority, })
                .WithTimePeriod(2001, TimeIdentifier.May)
                .Generate();

            var observation3 = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithLocation(new Location { GeographicLevel = GeographicLevel.Region, })
                .WithTimePeriod(2002, TimeIdentifier.June)
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

            var service = BuildDataImportService(
                contentDbContextId,
                statisticsDbContextId);

            await service.WriteDataSetFileMeta(file.Id, subject.Id);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedFile = contentDbContext.Files
                    .Include(f => f.DataSetFileVersionGeographicLevels)
                    .Single(f => f.SubjectId == subject.Id);

                var geogLvls = updatedFile.DataSetFileVersionGeographicLevels
                    .Select(gl => gl.GeographicLevel)
                    .ToList();
                Assert.Equal(3, geogLvls.Count);
                Assert.Contains(GeographicLevel.Country, geogLvls);
                Assert.Contains(GeographicLevel.LocalAuthority, geogLvls);
                Assert.Contains(GeographicLevel.Region, geogLvls);

                Assert.NotNull(updatedFile.DataSetFileMeta);
                var meta = updatedFile.DataSetFileMeta;

                Assert.Equal("2000", meta.TimePeriodRange.Start.Period);
                Assert.Equal(TimeIdentifier.April, meta.TimePeriodRange.Start.TimeIdentifier);
                Assert.Equal("2002", meta.TimePeriodRange.End.Period);
                Assert.Equal(TimeIdentifier.June, meta.TimePeriodRange.End.TimeIdentifier);

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
                    FilterGroups = [
                        new FilterGroup
                        {
                            Label  = "Root filter group",
                            FilterItems = [
                                new FilterItem { Label = "RootFilterItem0", },
                                new FilterItem { Label = "RootFilterItem1", },
                            ],
                        },
                    ],
                    ParentFilter = null,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Name = "child_filter1",
                    FilterGroups = [
                        new FilterGroup
                        {
                            Label  = "Child filter 1 group",
                            FilterItems = [
                                new FilterItem { Label = "ChildFilter1Item0", },
                                new FilterItem { Label = "ChildFilter1Item1", },
                                new FilterItem { Label = "ChildFilter1Item2", },
                            ],
                        },
                    ],
                    ParentFilter = "root_filter",
                },
                new()
                {
                    SubjectId = subject.Id,
                    Name = "child_filter2",
                    FilterGroups = [
                        new FilterGroup
                        {
                            Label  = "Child filter 2 group",
                            FilterItems = [
                                new FilterItem { Label = "ChildFilter2Item0", },
                                new FilterItem { Label = "ChildFilter2Item1", },
                                new FilterItem { Label = "ChildFilter2Item2", },
                                new FilterItem { Label = "ChildFilter2Item3", },
                                new FilterItem { Label = "ChildFilter2Item4", },
                                new FilterItem { Label = "ChildFilter2Item5", },
                            ],
                        },
                    ],
                    ParentFilter = "child_filter1",
                }
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

            var observation0 = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithFilterItems([
                    rootFilterItem0,
                    childFilter1Item0,
                    childFilter2Item0,
                ]);

            var observation1 = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithFilterItems([
                    rootFilterItem1,
                    childFilter1Item1,
                    childFilter2Item1,
                ]);

            var observation2 = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithFilterItems([
                    rootFilterItem1,
                    childFilter1Item1,
                    childFilter2Item2,
                ]);

            var observation3 = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithFilterItems([
                    rootFilterItem1,
                    childFilter1Item2,
                    childFilter2Item3,
                ]);

            var observation4 = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithFilterItems([
                    rootFilterItem1,
                    childFilter1Item2,
                    childFilter2Item4,
                ]);

            var observation5 = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithFilterItems([
                    rootFilterItem1,
                    childFilter1Item2,
                    childFilter2Item5,
                ]);

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Filter.AddRange(filters);
                statisticsDbContext.Observation.AddRange(
                    observation0, observation1, observation2, observation3, observation4, observation5);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var results = await DataImportService.GenerateFilterHierarchies(
                    statisticsDbContext,
                    filters.Select(f => new FilterMeta
                    {
                        Id = f.Id,
                        Label = f.Label,
                        Hint = f.Hint,
                        ColumnName = f.Name,
                        ParentFilter = f.ParentFilter,
                    }).ToList());

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
                    hierarchy.Tiers[1].Keys.ToList());
                Assert.Equal([childFilter2Item0.Id], hierarchy.Tiers[1][childFilter1Item0.Id]);
                Assert.Equal([childFilter2Item1.Id, childFilter2Item2.Id], hierarchy.Tiers[1][childFilter1Item1.Id]);
                Assert.Equal([childFilter2Item3.Id, childFilter2Item4.Id, childFilter2Item5.Id],
                    hierarchy.Tiers[1][childFilter1Item2.Id]);
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
                    FilterGroups = [
                        new FilterGroup
                        {
                            Label  = "Root filter 0 group",
                            FilterItems = [
                                new FilterItem { Label = "RootFilter0Item0", },
                            ],
                        },
                    ],
                    ParentFilter = null,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Name = "child_filter0",
                    FilterGroups = [
                        new FilterGroup
                        {
                            Label  = "Child filter 0 group",
                            FilterItems = [
                                new FilterItem { Label = "ChildFilter0Item0", },
                            ],
                        },
                    ],
                    ParentFilter = "root_filter0",
                },
                new()
                {
                    SubjectId = subject.Id,
                    Name = "root_filter1",
                    FilterGroups = [
                        new FilterGroup
                        {
                            Label  = "Root filter 1 group",
                            FilterItems = [
                                new FilterItem { Label = "RootFilter1Item0", },
                            ],
                        },
                    ],
                    ParentFilter = null,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Name = "child_filter1",
                    FilterGroups = [
                        new FilterGroup
                        {
                            Label  = "Child filter 1 group",
                            FilterItems = [
                                new FilterItem { Label = "ChildFilter1Item0", },
                            ],
                        },
                    ],
                    ParentFilter = "root_filter1",
                },
            };

            var rootFilter0Item0 = filters[0].FilterGroups[0].FilterItems[0];
            var childFilter0Item0 = filters[1].FilterGroups[0].FilterItems[0];
            var rootFilter1Item0 = filters[2].FilterGroups[0].FilterItems[0];
            var childFilter1Item0 = filters[3].FilterGroups[0].FilterItems[0];

            var observation0 = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithFilterItems([
                    rootFilter0Item0,
                    childFilter0Item0,
                ]);

            var observation1 = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithFilterItems([
                    rootFilter1Item0,
                    childFilter1Item0,
                ]);

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
                    filters.Select(f => new FilterMeta
                    {
                        Id = f.Id,
                        Label = f.Label,
                        Hint = f.Hint,
                        ColumnName = f.Name,
                        ParentFilter = f.ParentFilter,
                    }).ToList());

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

        private static DataImportService BuildDataImportService(
            string? contentDbContextId = null,
            string? statisticsDbContextId = null)
        {
            var dbContextSupplier = new InMemoryDbContextSupplier(
                contentDbContextId ?? Guid.NewGuid().ToString(),
                statisticsDbContextId ?? Guid.NewGuid().ToString());

            return new DataImportService(
                dbContextSupplier,
                Mock.Of<ILogger<DataImportService>>(Strict));
        }
    }
}
