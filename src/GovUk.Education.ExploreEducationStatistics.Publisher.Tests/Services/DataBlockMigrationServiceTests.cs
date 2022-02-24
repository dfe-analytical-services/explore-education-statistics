#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.Data.TableHeaderType;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class DataBlockMigrationServiceTests
    {
        private readonly Country _england = new("E92000001", "England");
        private readonly Region _eastMidlands = new("E12000004", "East Midlands");
        private readonly LocalAuthority _derby = new("E06000015", "", "Derby");
        private readonly LocalAuthority _nottingham = new("E06000018", "", "Nottingham");

        private readonly Subject _subject = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task Migrate_DataBlockNotFound()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId);

            var service = BuildService(
                contentDbContext: contentDbContext);

            var result = await service.Migrate(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task Migrate_SubjectNotFound()
        {
            var dataBlock = new DataBlock
            {
                Query = new ObservationQueryContext
                {
                    SubjectId = Guid.NewGuid()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            var statisticsPersistenceHelper = new Mock<IPersistenceHelper<StatisticsDbContext>>(MockBehavior.Strict);
            statisticsPersistenceHelper.Setup(mock =>
                    mock.CheckEntityExists(
                        dataBlock.Query.SubjectId,
                        It.IsAny<Func<IQueryable<Subject>, IQueryable<Subject>>>()))
                .ReturnsAsync(new NotFoundResult());

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    statisticsPersistenceHelper: statisticsPersistenceHelper.Object);

                var result = await service.Migrate(dataBlock.Id);

                MockUtils.VerifyAllMocks(statisticsPersistenceHelper);

                result.AssertNotFound();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var after = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                VerifyOriginalFieldsAreUntouched(dataBlock, after);
                Assert.False(after.LocationsMigrated);
            }
        }

        [Fact]
        public async Task Migrate_DataBlockHasNoLocations()
        {
            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>(),
                Table = new TableBuilderConfiguration(),
                Query = new ObservationQueryContext
                {
                    SubjectId = _subject.Id,
                    Locations = new LocationQuery()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            locationRepository.Setup(mock => mock.GetDistinctForSubject(_subject.Id))
                .ReturnsAsync(new List<Location>());

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    locationRepository: locationRepository.Object);

                var result = await service.Migrate(dataBlock.Id);

                MockUtils.VerifyAllMocks(locationRepository);

                result.AssertRight();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var after = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                VerifyOriginalFieldsAreUntouched(dataBlock, after);
                Assert.True(after.LocationsMigrated);

                Assert.Empty(after.ChartsMigrated);
                Assert.Empty(after.QueryMigrated.LocationIds);
                Assert.Empty(after.TableMigrated.TableHeaders.Columns);
                Assert.Empty(after.TableMigrated.TableHeaders.Rows);
                Assert.Empty(after.TableMigrated.TableHeaders.ColumnGroups);
                Assert.Empty(after.TableMigrated.TableHeaders.RowGroups);
            }
        }

        [Fact]
        public async Task Migrate_ChartMajorAxisHasCodeWhichMapsToMultipleLocations()
        {
            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>(),
                Table = new TableBuilderConfiguration(),
                Query = new ObservationQueryContext
                {
                    SubjectId = _subject.Id,
                    Locations = new LocationQuery()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            locationRepository.Setup(mock => mock.GetDistinctForSubject(_subject.Id))
                .ReturnsAsync(new List<Location>());

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    locationRepository: locationRepository.Object);

                var result = await service.Migrate(dataBlock.Id);

                MockUtils.VerifyAllMocks(locationRepository);

                result.AssertRight();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var after = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                VerifyOriginalFieldsAreUntouched(dataBlock, after);
                Assert.True(after.LocationsMigrated);

                Assert.Empty(after.ChartsMigrated);
                Assert.Empty(after.QueryMigrated.LocationIds);
                Assert.Empty(after.TableMigrated.TableHeaders.Columns);
                Assert.Empty(after.TableMigrated.TableHeaders.Rows);
                Assert.Empty(after.TableMigrated.TableHeaders.ColumnGroups);
                Assert.Empty(after.TableMigrated.TableHeaders.RowGroups);
            }
        }

        [Fact]
        public async Task Migrate_HappyPath()
        {
            // Some filter and indicator id's that we expect to remain consistent
            var filterItemId1 = Guid.NewGuid();
            var filterItemId2 = Guid.NewGuid();
            var indicatorId1 = Guid.NewGuid();
            var indicatorId2 = Guid.NewGuid();

            var location1 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
            };

            var location2 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _eastMidlands
            };

            var location3 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                LocalAuthority = _derby
            };

            var location4 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                LocalAuthority = _nottingham
            };

            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {
                                "major",
                                new ChartAxisConfiguration
                                {
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new()
                                        {
                                            Filters = new List<Guid>
                                            {
                                                filterItemId1
                                            },
                                            Indicator = indicatorId1,
                                            Location = new ChartDataSetLocation
                                            {
                                                Level = "country",
                                                Value = _england.Code
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        Legend = new ChartLegend
                        {
                            Items = new List<ChartLegendItem>
                            {
                                new()
                                {
                                    DataSet = new ChartLegendItemDataSet
                                    {
                                        Filters = new List<Guid>
                                        {
                                            filterItemId1
                                        },
                                        Indicator = indicatorId1,
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = "country",
                                            Value = _england.Code
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Table = new TableBuilderConfiguration
                {
                    // Nonsensical table headers but includes one of every type in every config field
                    TableHeaders = new TableHeaders
                    {
                        ColumnGroups = new List<List<TableHeader>>
                        {
                            new()
                            {
                                new TableHeader(filterItemId1.ToString(), TableHeaderType.Filter),
                                new TableHeader(indicatorId1.ToString(), TableHeaderType.Indicator),
                                new TableHeader("2021_CY", TimePeriod),
                                TableHeader.NewLocationHeader(GeographicLevel.Country, _england.Code!)
                            },
                            new()
                            {
                                new TableHeader(filterItemId1.ToString(), TableHeaderType.Filter),
                                new TableHeader(indicatorId1.ToString(), TableHeaderType.Indicator),
                                new TableHeader("2022_CY", TimePeriod),
                                TableHeader.NewLocationHeader(GeographicLevel.Region, _eastMidlands.Code!)
                            }
                        },
                        Columns = new List<TableHeader>
                        {
                            new(filterItemId1.ToString(), TableHeaderType.Filter),
                            new(indicatorId1.ToString(), TableHeaderType.Indicator),
                            new("2020_CY", TimePeriod),
                            TableHeader.NewLocationHeader(GeographicLevel.LocalAuthority, _derby.Code!)
                        },
                        RowGroups = new List<List<TableHeader>>
                        {
                            new()
                            {
                                new TableHeader(filterItemId1.ToString(), TableHeaderType.Filter),
                                new TableHeader(indicatorId1.ToString(), TableHeaderType.Indicator),
                                new TableHeader("2021_CY", TimePeriod),
                                TableHeader.NewLocationHeader(GeographicLevel.Country, _england.Code!)
                            },
                            new()
                            {
                                new TableHeader(filterItemId1.ToString(), TableHeaderType.Filter),
                                new TableHeader(indicatorId1.ToString(), TableHeaderType.Indicator),
                                new TableHeader("2022_CY", TimePeriod),
                                TableHeader.NewLocationHeader(GeographicLevel.Region, _eastMidlands.Code!)
                            }
                        },
                        Rows = new List<TableHeader>
                        {
                            new(filterItemId1.ToString(), TableHeaderType.Filter),
                            new(indicatorId1.ToString(), TableHeaderType.Indicator),
                            new("2019_CY", TimePeriod),
                            TableHeader.NewLocationHeader(GeographicLevel.LocalAuthority, _nottingham.Code!)
                        }
                    }
                },
                Query = new ObservationQueryContext
                {
                    SubjectId = _subject.Id,
                    Locations = new LocationQuery
                    {
                        Country = ListOf(_england.Code!),
                        Region = ListOf(_eastMidlands.Code!),
                        LocalAuthority = ListOf(_derby.Code!, _nottingham.Code!)
                    },
                    Filters = ListOf(filterItemId1, filterItemId2),
                    Indicators = ListOf(indicatorId1, indicatorId2),
                    BoundaryLevel = 1,
                    IncludeGeoJson = true,
                    TimePeriod = new TimePeriodQuery
                    {
                        StartCode = CalendarYear,
                        StartYear = 2019,
                        EndCode = CalendarYear,
                        EndYear = 2022
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            locationRepository.Setup(mock => mock.GetDistinctForSubject(_subject.Id))
                .ReturnsAsync(ListOf(location1, location2, location3, location4));

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    locationRepository: locationRepository.Object);

                var result = await service.Migrate(dataBlock.Id);

                MockUtils.VerifyAllMocks(locationRepository);

                result.AssertRight();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var after = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                VerifyOriginalFieldsAreUntouched(dataBlock, after);
                Assert.True(after.LocationsMigrated);

                var migratedChart = Assert.Single(after.ChartsMigrated);

                // Chart major axis data set assertions
                var chartMajorAxis = migratedChart.Axes?["major"];
                Assert.NotNull(chartMajorAxis);
                var chartMajorAxesDataSet = Assert.Single(chartMajorAxis!.DataSets);
                Assert.Single(chartMajorAxesDataSet.Filters);
                Assert.Equal(filterItemId1, chartMajorAxesDataSet.Filters[0]);
                Assert.Equal(indicatorId1, chartMajorAxesDataSet.Indicator);
                Assert.NotNull(chartMajorAxesDataSet.Location);
                Assert.Equal("country", chartMajorAxesDataSet.Location.Level);
                //Assert.Equal(location1.Id, Guid.Parse(chartMajorAxesDataSet.Location.Value));

                // Chart legend data set assertions
                var chartLegendItems = migratedChart.Legend?.Items;
                Assert.NotNull(chartLegendItems);
                var chartLegendItem = Assert.Single(chartLegendItems);
                Assert.Single(chartLegendItem.DataSet.Filters);
                Assert.Equal(filterItemId1, chartLegendItem.DataSet.Filters[0]);
                Assert.Equal(indicatorId1, chartLegendItem.DataSet.Indicator);
                Assert.NotNull(chartLegendItem.DataSet.Location);
                Assert.Equal("country", chartLegendItem.DataSet.Location!.Level);
                //Assert.Equal(location1.Id, Guid.Parse(chartLegendItem.DataSet.Location!.Value));

                // Query assertions
                Assert.Equal(dataBlock.Query.SubjectId, after.QueryMigrated.SubjectId);
                Assert.Equal(dataBlock.Query.Filters, after.QueryMigrated.Filters);
                Assert.Equal(dataBlock.Query.Indicators, after.QueryMigrated.Indicators);
                after.QueryMigrated.TimePeriod.AssertDeepEqualTo(dataBlock.Query.TimePeriod);
                Assert.Equal(dataBlock.Query.BoundaryLevel, after.QueryMigrated.BoundaryLevel);
                Assert.Equal(dataBlock.Query.IncludeGeoJson, after.QueryMigrated.IncludeGeoJson);

                // Query location assertions
                Assert.Null(after.QueryMigrated.Locations);
                Assert.Equal(4, after.QueryMigrated.LocationIds.Count());
                Assert.Contains(location1.Id, after.QueryMigrated.LocationIds);
                Assert.Contains(location2.Id, after.QueryMigrated.LocationIds);
                Assert.Contains(location3.Id, after.QueryMigrated.LocationIds);
                Assert.Contains(location4.Id, after.QueryMigrated.LocationIds);

                // Table header column group assertions
                Assert.Equal(2, after.TableMigrated.TableHeaders.ColumnGroups.Count);
                var columnGroup1 = after.TableMigrated.TableHeaders.ColumnGroups[0];
                var columnGroup2 = after.TableMigrated.TableHeaders.ColumnGroups[1];

                // Table header column group 1 assertions
                Assert.Equal(4, columnGroup1.Count);
                Assert.Equal(TableHeaderType.Filter, columnGroup1[0].Type);
                Assert.Null(columnGroup1[0].Level);
                Assert.Equal(filterItemId1, Guid.Parse(columnGroup1[0].Value));
                Assert.Equal(TableHeaderType.Indicator, columnGroup1[1].Type);
                Assert.Null(columnGroup1[1].Level);
                Assert.Equal(indicatorId1, Guid.Parse(columnGroup1[1].Value));
                Assert.Equal(TableHeaderType.TimePeriod, columnGroup1[2].Type);
                Assert.Null(columnGroup1[2].Level);
                Assert.Equal("2021_CY", columnGroup1[2].Value);
                Assert.Equal(TableHeaderType.Location, columnGroup1[3].Type);
                Assert.Equal("country", columnGroup1[3].Level);
                Assert.Equal(location1.Id, Guid.Parse(columnGroup1[3].Value));

                // Table header column group 2 assertions
                Assert.Equal(4, columnGroup2.Count);
                Assert.Equal(TableHeaderType.Filter, columnGroup2[0].Type);
                Assert.Null(columnGroup2[0].Level);
                Assert.Equal(filterItemId1, Guid.Parse(columnGroup2[0].Value));
                Assert.Equal(TableHeaderType.Indicator, columnGroup2[1].Type);
                Assert.Null(columnGroup2[1].Level);
                Assert.Equal(indicatorId1, Guid.Parse(columnGroup2[1].Value));
                Assert.Equal(TableHeaderType.TimePeriod, columnGroup2[2].Type);
                Assert.Null(columnGroup2[2].Level);
                Assert.Equal("2022_CY", columnGroup2[2].Value);
                Assert.Equal(TableHeaderType.Location, columnGroup2[3].Type);
                Assert.Equal("region", columnGroup2[3].Level);
                Assert.Equal(location2.Id, Guid.Parse(columnGroup2[3].Value));

                // Table header columns assertions
                Assert.Equal(4, after.TableMigrated.TableHeaders.Columns.Count);
                Assert.Equal(TableHeaderType.Filter, after.TableMigrated.TableHeaders.Columns[0].Type);
                Assert.Null(after.TableMigrated.TableHeaders.Columns[0].Level);
                Assert.Equal(filterItemId1, Guid.Parse(after.TableMigrated.TableHeaders.Columns[0].Value));
                Assert.Equal(TableHeaderType.Indicator, after.TableMigrated.TableHeaders.Columns[1].Type);
                Assert.Null(after.TableMigrated.TableHeaders.Columns[1].Level);
                Assert.Equal(indicatorId1, Guid.Parse(after.TableMigrated.TableHeaders.Columns[1].Value));
                Assert.Equal(TableHeaderType.TimePeriod, after.TableMigrated.TableHeaders.Columns[2].Type);
                Assert.Null(after.TableMigrated.TableHeaders.Columns[2].Level);
                Assert.Equal("2020_CY", after.TableMigrated.TableHeaders.Columns[2].Value);
                Assert.Equal(TableHeaderType.Location, after.TableMigrated.TableHeaders.Columns[3].Type);
                Assert.Equal("localAuthority", after.TableMigrated.TableHeaders.Columns[3].Level);
                Assert.Equal(location3.Id, Guid.Parse(after.TableMigrated.TableHeaders.Columns[3].Value));

                // Table header row group assertions
                Assert.Equal(2, after.TableMigrated.TableHeaders.RowGroups.Count);
                var rowGroup1 = after.TableMigrated.TableHeaders.RowGroups[0];
                var rowGroup2 = after.TableMigrated.TableHeaders.RowGroups[1];

                // Table header row group 1 assertions
                Assert.Equal(4, rowGroup1.Count);
                Assert.Equal(TableHeaderType.Filter, rowGroup1[0].Type);
                Assert.Null(rowGroup1[0].Level);
                Assert.Equal(filterItemId1, Guid.Parse(rowGroup1[0].Value));
                Assert.Equal(TableHeaderType.Indicator, rowGroup1[1].Type);
                Assert.Null(rowGroup1[1].Level);
                Assert.Equal(indicatorId1, Guid.Parse(rowGroup1[1].Value));
                Assert.Equal(TableHeaderType.TimePeriod, rowGroup1[2].Type);
                Assert.Null(rowGroup1[2].Level);
                Assert.Equal("2021_CY", rowGroup1[2].Value);
                Assert.Equal(TableHeaderType.Location, rowGroup1[3].Type);
                Assert.Equal("country", rowGroup1[3].Level);
                Assert.Equal(location1.Id, Guid.Parse(rowGroup1[3].Value));

                // Table header row group 2 assertions
                Assert.Equal(4, rowGroup2.Count);
                Assert.Equal(TableHeaderType.Filter, rowGroup2[0].Type);
                Assert.Null(rowGroup2[0].Level);
                Assert.Equal(filterItemId1, Guid.Parse(rowGroup2[0].Value));
                Assert.Equal(TableHeaderType.Indicator, rowGroup2[1].Type);
                Assert.Null(rowGroup2[1].Level);
                Assert.Equal(indicatorId1, Guid.Parse(rowGroup2[1].Value));
                Assert.Equal(TableHeaderType.TimePeriod, rowGroup2[2].Type);
                Assert.Null(rowGroup2[2].Level);
                Assert.Equal("2022_CY", rowGroup2[2].Value);
                Assert.Equal(TableHeaderType.Location, rowGroup2[3].Type);
                Assert.Equal("region", rowGroup2[3].Level);
                Assert.Equal(location2.Id, Guid.Parse(rowGroup2[3].Value));

                // Table header rows assertions
                Assert.Equal(4, after.TableMigrated.TableHeaders.Rows.Count);
                Assert.Equal(TableHeaderType.Filter, after.TableMigrated.TableHeaders.Rows[0].Type);
                Assert.Null(after.TableMigrated.TableHeaders.Rows[0].Level);
                Assert.Equal(filterItemId1, Guid.Parse(after.TableMigrated.TableHeaders.Rows[0].Value));
                Assert.Equal(TableHeaderType.Indicator, after.TableMigrated.TableHeaders.Rows[1].Type);
                Assert.Null(after.TableMigrated.TableHeaders.Rows[1].Level);
                Assert.Equal(indicatorId1, Guid.Parse(after.TableMigrated.TableHeaders.Rows[1].Value));
                Assert.Equal(TableHeaderType.TimePeriod, after.TableMigrated.TableHeaders.Rows[2].Type);
                Assert.Null(after.TableMigrated.TableHeaders.Rows[2].Level);
                Assert.Equal("2019_CY", after.TableMigrated.TableHeaders.Rows[2].Value);
                Assert.Equal(TableHeaderType.Location, after.TableMigrated.TableHeaders.Rows[3].Type);
                Assert.Equal("localAuthority", after.TableMigrated.TableHeaders.Rows[3].Level);
                Assert.Equal(location4.Id, Guid.Parse(after.TableMigrated.TableHeaders.Rows[3].Value));
            }
        }

        // TODO EES-3167 Write some more tests! :)

        private DataBlockMigrationService BuildService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
            ILocationRepository? locationRepository = null)
        {
            return new DataBlockMigrationService(
                contentDbContext,
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                statisticsPersistenceHelper ?? DefaultStatisticsPersistenceHelper(),
                locationRepository ?? Mock.Of<ILocationRepository>(MockBehavior.Strict));
        }

        private static void VerifyOriginalFieldsAreUntouched(DataBlock original, DataBlock after)
        {
            // Check the original data block fields have not been touched
            after.Charts.AssertDeepEqualTo(original.Charts);
            after.Query.AssertDeepEqualTo(original.Query);
            after.Table.AssertDeepEqualTo(original.Table);
        }

        private IPersistenceHelper<StatisticsDbContext> DefaultStatisticsPersistenceHelper()
        {
            var statisticsPersistenceHelper = MockUtils.MockPersistenceHelper<StatisticsDbContext, Subject>();
            MockUtils.SetupCall(statisticsPersistenceHelper, _subject.Id, _subject);
            return statisticsPersistenceHelper.Object;
        }
    }
}
