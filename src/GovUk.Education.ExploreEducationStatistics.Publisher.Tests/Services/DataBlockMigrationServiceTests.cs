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
        private readonly LocalAuthority _derbyDuplicate = new("E06000015", "", "Derby (2)");
        private readonly LocalAuthority _nottingham = new("E06000018", "", "Nottingham");
        private readonly LocalAuthority _nottinghamDuplicate = new("E06000018", "", "Nottingham (2)");
        private readonly School _hogwarts = new("10000001", "Hogwarts School");
        private readonly School _hogwartsDuplicate = new("10000001", "Hogwarts School (2)");
        private readonly School _rydellHigh = new("10000002", "Rydell High School");
        private readonly School _rydellHighDuplicate = new("10000002", "Rydell High School (2)");

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
                Assert.Equal(location1.Id, Guid.Parse(chartMajorAxesDataSet.Location.Value));

                // Chart legend data set assertions
                var chartLegendItems = migratedChart.Legend?.Items;
                Assert.NotNull(chartLegendItems);
                var chartLegendItem = Assert.Single(chartLegendItems);
                Assert.Single(chartLegendItem.DataSet.Filters);
                Assert.Equal(filterItemId1, chartLegendItem.DataSet.Filters[0]);
                Assert.Equal(indicatorId1, chartLegendItem.DataSet.Indicator);
                Assert.NotNull(chartLegendItem.DataSet.Location);
                Assert.Equal("country", chartLegendItem.DataSet.Location!.Level);
                Assert.Equal(location1.Id, Guid.Parse(chartLegendItem.DataSet.Location!.Value));

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

        [Fact]
        public async Task Migrate_QueryHasDuplicateLocationCodesTransformedToMultipleIds()
        {
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

            // Location contains same code as Derby
            var location3Duplicate = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                LocalAuthority = _derbyDuplicate
            };

            var location4 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                LocalAuthority = _nottingham
            };

            // Location contains same code as Nottingham
            var location4Duplicate = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                LocalAuthority = _nottinghamDuplicate
            };

            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>(),
                Table = new TableBuilderConfiguration(),
                Query = new ObservationQueryContext
                {
                    SubjectId = _subject.Id,
                    Locations = new LocationQuery
                    {
                        Country = ListOf(_england.Code!),
                        Region = ListOf(_eastMidlands.Code!),
                        LocalAuthority = ListOf(_derby.Code!, _nottingham.Code!)
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
                .ReturnsAsync(ListOf(
                    location1,
                    location2,
                    location3,
                    location3Duplicate,
                    location4,
                    location4Duplicate));

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

                Assert.Null(after.QueryMigrated.Locations);
                Assert.Equal(6, after.QueryMigrated.LocationIds.Count());
                Assert.Contains(location1.Id, after.QueryMigrated.LocationIds);
                Assert.Contains(location2.Id, after.QueryMigrated.LocationIds);
                Assert.Contains(location3.Id, after.QueryMigrated.LocationIds);
                Assert.Contains(location4.Id, after.QueryMigrated.LocationIds);
                // Assert locations with the same codes are also included
                Assert.Contains(location3Duplicate.Id, after.QueryMigrated.LocationIds);
                Assert.Contains(location4Duplicate.Id, after.QueryMigrated.LocationIds);
            }
        }

        [Fact]
        public async Task Migrate_QueryHasCodeNotInSubjectLocations_FailsToMigrate()
        {
            var location1 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
            };

            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>(),
                Table = new TableBuilderConfiguration(),
                Query = new ObservationQueryContext
                {
                    SubjectId = _subject.Id,
                    Locations = new LocationQuery
                    {
                        Country = ListOf(_england.Code!),
                        // This region is not in the Subject locations
                        Region = ListOf(_eastMidlands.Code!)
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
                .ReturnsAsync(ListOf(location1));

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    locationRepository: locationRepository.Object);

                var exception =
                    await Assert.ThrowsAsync<InvalidOperationException>(() => service.Migrate(dataBlock.Id));
                Assert.Equal(
                    $"Expected a mapping for data block '{dataBlock.Id}' query location with level 'Region' and code '{_eastMidlands.Code}'",
                    exception.Message);

                MockUtils.VerifyAllMocks(locationRepository);
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var after = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                VerifyOriginalFieldsAreUntouched(dataBlock, after);
                VerifyMigrationFailed(after);
            }
        }

        [Fact]
        public async Task Migrate_TableHeadersAreTransformedToMultipleHeaders()
        {
            // Test that table header configuration containing School or Provider locations are transformed into
            // multiple adjacent headers where there are duplicate locations with identical codes

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
                GeographicLevel = GeographicLevel.School,
                Country = _england,
                School = _hogwarts
            };

            // Location contains same code for School attribute
            var location3Duplicate = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.School,
                Country = _england,
                School = _hogwartsDuplicate
            };

            var location4 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.School,
                Country = _england,
                School = _rydellHigh
            };

            // Location contains same code for School attribute
            var location4Duplicate = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.School,
                Country = _england,
                School = _rydellHighDuplicate
            };

            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>(),
                Table = new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        ColumnGroups = new List<List<TableHeader>>
                        {
                            new()
                            {
                                TableHeader.NewLocationHeader(GeographicLevel.Country, _england.Code!)
                            }
                        },
                        Columns = new List<TableHeader>
                        {
                            TableHeader.NewLocationHeader(GeographicLevel.Region, _eastMidlands.Code!)
                        },
                        RowGroups = new List<List<TableHeader>>
                        {
                            new()
                            {
                                TableHeader.NewLocationHeader(GeographicLevel.School, _hogwarts.Code!)
                            },
                        },
                        Rows = new List<TableHeader>
                        {
                            TableHeader.NewLocationHeader(GeographicLevel.School, _rydellHigh.Code!)
                        }
                    }
                },
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
                .ReturnsAsync(ListOf(
                    location1,
                    location2,
                    location3,
                    location3Duplicate,
                    location4,
                    location4Duplicate));

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

                // Table header column group assertions
                var columnGroup = Assert.Single(after.TableMigrated.TableHeaders.ColumnGroups);
                var columnGroupColumn = Assert.Single(columnGroup);
                Assert.Equal(TableHeaderType.Location, columnGroupColumn.Type);
                Assert.Equal("country", columnGroupColumn.Level);
                Assert.Equal(location1.Id, Guid.Parse(columnGroupColumn.Value));

                // Table header columns assertions
                var column = Assert.Single(after.TableMigrated.TableHeaders.Columns);
                Assert.Equal(TableHeaderType.Location, column.Type);
                Assert.Equal("region", column.Level);
                Assert.Equal(location2.Id, Guid.Parse(column.Value));

                // Table header row group assertions
                var rowGroups = after.TableMigrated.TableHeaders.RowGroups;
                var rowGroup = Assert.Single(rowGroups);
                Assert.Equal(2, rowGroup.Count);
                var rowGroupRow1 = rowGroup[0];
                Assert.Equal(TableHeaderType.Location, rowGroupRow1.Type);
                Assert.Equal("school", rowGroupRow1.Level);
                Assert.Equal(location3.Id, Guid.Parse(rowGroupRow1.Value));
                // Assert an additional table header has been inserted for a location with the same code
                var rowGroupRow2 = rowGroup[1];
                Assert.Equal(TableHeaderType.Location, rowGroupRow2.Type);
                Assert.Equal("school", rowGroupRow2.Level);
                Assert.Equal(location3Duplicate.Id, Guid.Parse(rowGroupRow2.Value));

                // Table header rows assertions
                var rows = after.TableMigrated.TableHeaders.Rows;
                Assert.Equal(2, rows.Count);
                Assert.Equal(TableHeaderType.Location, rows[0].Type);
                Assert.Equal("school", rows[0].Level);
                Assert.Equal(location4.Id, Guid.Parse(rows[0].Value));
                // Assert an additional table header has been inserted for a location with the same code
                Assert.Equal(TableHeaderType.Location, rows[1].Type);
                Assert.Equal("school", rows[1].Level);
                Assert.Equal(location4Duplicate.Id, Guid.Parse(rows[1].Value));
            }
        }

        [Fact]
        public async Task Migrate_TableHeaderHasCodeNotInSubjectLocations_FailsToMigrate()
        {
            var location1 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
            };

            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>(),
                Table = new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        ColumnGroups = new List<List<TableHeader>>(),
                        Columns = new List<TableHeader>
                        {
                            TableHeader.NewLocationHeader(GeographicLevel.Country, _england.Code!)
                        },
                        RowGroups = new List<List<TableHeader>>(),
                        Rows = new List<TableHeader>
                        {
                            // This region is not in the Subject locations
                            TableHeader.NewLocationHeader(GeographicLevel.Region, _eastMidlands.Code!)
                        }
                    }
                },
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
                .ReturnsAsync(ListOf(
                    location1));

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    locationRepository: locationRepository.Object);

                var exception =
                    await Assert.ThrowsAsync<InvalidOperationException>(() => service.Migrate(dataBlock.Id));
                Assert.Equal(
                    $"Expected a mapping for data block '{dataBlock.Id}' Rows table header with level '{GeographicLevel.Region}' and code '{_eastMidlands.Code}'",
                    exception.Message);

                MockUtils.VerifyAllMocks(locationRepository);
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var after = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                VerifyOriginalFieldsAreUntouched(dataBlock, after);
                VerifyMigrationFailed(after);
            }
        }

        [Fact]
        public async Task Migrate_TableHeaderHasDuplicateLocationsCodeNotInRowsOrRowGroups_FailsToMigrate()
        {
            // A check of Prod data reveals there's no locations in Column/ColumnGroup headers for Provider/School level data.
            // Since what we do for Rows/RowGroups is not completely safe we limit creating extra headers for ColumnGroups/Columns.
            // Assert the migration fails if there's a duplicate location code in a Column/ColumnGroup header
            // so that we can investigate.

            var location1 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.School,
                Country = _england,
                School = _hogwarts
            };

            var location2 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.School,
                Country = _england,
                School = _rydellHigh
            };

            // Location contains same code for School attribute
            var location2Duplicate = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.School,
                Country = _england,
                School = _rydellHighDuplicate
            };

            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>(),
                Table = new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        ColumnGroups = new List<List<TableHeader>>(),
                        Columns = new List<TableHeader>
                        {
                            TableHeader.NewLocationHeader(GeographicLevel.School, _rydellHigh.Code!)
                        },
                        RowGroups = new List<List<TableHeader>>(),
                        Rows = new List<TableHeader>
                        {
                            TableHeader.NewLocationHeader(GeographicLevel.School, _hogwarts.Code!)
                        }
                    }
                },
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
                .ReturnsAsync(ListOf(
                    location1,
                    location2,
                    location2Duplicate));

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    locationRepository: locationRepository.Object);

                var exception =
                    await Assert.ThrowsAsync<InvalidOperationException>(() => service.Migrate(dataBlock.Id));
                Assert.Equal(
                    $"Only expecting one location id for data block '{dataBlock.Id}' Columns table header with level '{GeographicLevel.School}' and code '{_rydellHigh.Code}'",
                    exception.Message);

                MockUtils.VerifyAllMocks(locationRepository);
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var after = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                VerifyOriginalFieldsAreUntouched(dataBlock, after);
                VerifyMigrationFailed(after);
            }
        }

        [Fact]
        public async Task Migrate_TableHeaderHasDuplicateLocationsCodeNotSchoolOrProvider_FailsToMigrate()
        {
            // Assert the migration fails if there's a duplicate location code in a table header that's not a School or Provider
            // so that we can investigate.

            var location1 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                LocalAuthority = _derby
            };

            var location2 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                LocalAuthority = _nottingham
            };

            // Location contains same code for Local Authority attribute
            var location2Duplicate = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                LocalAuthority = _nottinghamDuplicate
            };

            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>(),
                Table = new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        ColumnGroups = new List<List<TableHeader>>(),
                        Columns = new List<TableHeader>(),
                        RowGroups = new List<List<TableHeader>>
                        {
                            new()
                            {
                                TableHeader.NewLocationHeader(GeographicLevel.LocalAuthority, _derby.Code!)
                            }
                        },
                        Rows = new List<TableHeader>
                        {
                            TableHeader.NewLocationHeader(GeographicLevel.LocalAuthority, _nottingham.Code!)
                        }
                    }
                },
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
                .ReturnsAsync(ListOf(
                    location1,
                    location2,
                    location2Duplicate));

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    locationRepository: locationRepository.Object);

                var exception =
                    await Assert.ThrowsAsync<InvalidOperationException>(() => service.Migrate(dataBlock.Id));
                Assert.Equal(
                    $"Only expecting one location id for data block '{dataBlock.Id}' Rows table header with level '{GeographicLevel.LocalAuthority}' and code '{_nottingham.Code}'",
                    exception.Message);

                MockUtils.VerifyAllMocks(locationRepository);
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var after = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                VerifyOriginalFieldsAreUntouched(dataBlock, after);
                VerifyMigrationFailed(after);
            }
        }

        [Fact]
        public async Task Migrate_ChartMajorAxisHasCodeNotInSubjectLocations_FailsToMigrate()
        {
            var location1 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
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
                                            Location = new ChartDataSetLocation
                                            {
                                                Level = "region",
                                                // This region is not in the Subject locations
                                                Value = _eastMidlands.Code
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
                .ReturnsAsync(ListOf(
                    location1));

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    locationRepository: locationRepository.Object);

                var exception =
                    await Assert.ThrowsAsync<InvalidOperationException>(() => service.Migrate(dataBlock.Id));
                Assert.Equal(
                    $"Expected a mapping for data block '{dataBlock.Id}' chart major axis data set location with level 'region' and code '{_eastMidlands.Code}'",
                    exception.Message);

                MockUtils.VerifyAllMocks(locationRepository);
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var after = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                VerifyOriginalFieldsAreUntouched(dataBlock, after);
                VerifyMigrationFailed(after);
            }
        }

        [Fact]
        public async Task Migrate_ChartLegendHasCodeNotInSubjectLocations_FailsToMigrate()
        {
            var location1 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
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
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = "region",
                                            // This region is not in the Subject locations
                                            Value = _eastMidlands.Code
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
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
                .ReturnsAsync(ListOf(
                    location1));

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    locationRepository: locationRepository.Object);

                var exception =
                    await Assert.ThrowsAsync<InvalidOperationException>(() => service.Migrate(dataBlock.Id));
                Assert.Equal(
                    $"Expected a mapping for data block '{dataBlock.Id}' chart legend data set location with level 'region' and code '{_eastMidlands.Code}'",
                    exception.Message);

                MockUtils.VerifyAllMocks(locationRepository);
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var after = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                VerifyOriginalFieldsAreUntouched(dataBlock, after);
                VerifyMigrationFailed(after);
            }
        }

        [Fact]
        public async Task Migrate_ChartMajorAxisHasDuplicateLocationCode_FailsToMigrate()
        {
            // A check of Prod data reveals there's no charts for Provider/School level data.
            // Assert the migration fails if there's a duplicate location code in a chart major axis data set
            // so that we can investigate.

            var location1 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.School,
                Country = _england,
                School = _hogwarts
            };

            var location2 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.School,
                Country = _england,
                School = _rydellHigh
            };

            // Location contains same code for School attribute
            var location2Duplicate = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.School,
                Country = _england,
                School = _rydellHighDuplicate
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
                                            Location = new ChartDataSetLocation
                                            {
                                                Level = "school",
                                                Value = _rydellHigh.Code
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
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = "school",
                                            Value = _hogwarts.Code
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
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
                .ReturnsAsync(ListOf(
                    location1,
                    location2,
                    location2Duplicate));

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    locationRepository: locationRepository.Object);

                var exception =
                    await Assert.ThrowsAsync<InvalidOperationException>(() => service.Migrate(dataBlock.Id));
                Assert.Equal(
                    $"Only expecting one location id for data block '{dataBlock.Id}' chart major axis data set location with level 'school' and code '{_rydellHigh.Code}'",
                    exception.Message);

                MockUtils.VerifyAllMocks(locationRepository);
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var after = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                VerifyOriginalFieldsAreUntouched(dataBlock, after);
                VerifyMigrationFailed(after);
            }
        }

        [Fact]
        public async Task Migrate_ChartLegendHasDuplicateLocationCode_FailsToMigrate()
        {
            // A check of Prod data reveals there's no charts for Provider/School level data.
            // Assert the migration fails if there's a duplicate location code in a chart legend data set
            // so that we can investigate.

            var location1 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.School,
                Country = _england,
                School = _hogwarts
            };

            var location2 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.School,
                Country = _england,
                School = _rydellHigh
            };

            // Location contains same code for School attribute
            var location2Duplicate = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.School,
                Country = _england,
                School = _rydellHighDuplicate
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
                                            Location = new ChartDataSetLocation
                                            {
                                                Level = "school",
                                                Value = _hogwarts.Code
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
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = "school",
                                            Value = _rydellHigh.Code
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
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
                .ReturnsAsync(ListOf(
                    location1,
                    location2,
                    location2Duplicate));

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    locationRepository: locationRepository.Object);

                var exception =
                    await Assert.ThrowsAsync<InvalidOperationException>(() => service.Migrate(dataBlock.Id));
                Assert.Equal(
                    $"Only expecting one location id for data block '{dataBlock.Id}' chart legend data set location with level 'school' and code '{_rydellHigh.Code}'",
                    exception.Message);

                MockUtils.VerifyAllMocks(locationRepository);
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var after = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                VerifyOriginalFieldsAreUntouched(dataBlock, after);
                VerifyMigrationFailed(after);
            }
        }

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

        private static void VerifyMigrationFailed(DataBlock after)
        {
            Assert.False(after.LocationsMigrated);
            Assert.Empty(after.ChartsMigrated);
            Assert.Null(after.QueryMigrated);
            Assert.Null(after.TableMigrated);
        }

        private IPersistenceHelper<StatisticsDbContext> DefaultStatisticsPersistenceHelper()
        {
            var statisticsPersistenceHelper = MockUtils.MockPersistenceHelper<StatisticsDbContext, Subject>();
            MockUtils.SetupCall(statisticsPersistenceHelper, _subject.Id, _subject);
            return statisticsPersistenceHelper.Object;
        }
    }
}
