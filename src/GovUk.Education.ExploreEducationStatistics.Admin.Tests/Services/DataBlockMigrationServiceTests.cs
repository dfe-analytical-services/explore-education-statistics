#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.DataBlockMigrationService.DataBlockMapMigrationPlan.MigrationStrategy;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataBlockMigrationServiceTests
    {
        [Fact]
        public async Task MigrateAllMaps_NoMaps()
        {
            await using var contentDbContext = InMemoryContentDbContext();
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var service = SetupService(contentDbContext, statisticsDbContext);

            var result = await service.MigrateMaps();

            result.AssertRight(new List<DataBlockMigrationService.MapMigrationResult>());
        }
        
        [Fact]
        public async Task MigrateAllMaps_NoMapsNeedMigrating()
        {
            // This Data Block already has a Boundary Level set on its Query and Map, so nothing needs doing to  it 
            // in terms of migration.
            var dataBlock = new DataBlock
            {
                Query = new ObservationQueryContext
                {
                    BoundaryLevel = 1
                },
                Charts = new List<IChart> {
                    new MapChart
                    {
                        BoundaryLevel = 1
                    }
                }
            };
                
            var contentDbContextId = Guid.NewGuid().ToString();
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using var statisticsDbContext = InMemoryStatisticsDbContext();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext, statisticsDbContext);
                var result = await service.MigrateMaps();
                result.AssertRight(new List<DataBlockMigrationService.MapMigrationResult>());
            }
        }
        
        [Fact]
        public async Task MigrateAllMaps_BoundaryLevelSetOnQueryButNotOnMapChartConfig()
        {
            var chosenBoundaryLevelId = 1;
            var chosenBoundaryLevelGeographicLevel = GeographicLevel.LocalAuthority;
            
            // This Data Block already has a Boundary Level set on its Query and Data Sets chosen that match its
            // Boundary Level's GeographicLevel, but the Boundary Level is not yet on its MapChart config.
            // We would therefore expect to see it migrated by having the chosen BoundaryLevel copied across to its
            // MapChart config.
            var dataBlock = new DataBlock
            {
                Query = new ObservationQueryContext
                {
                    BoundaryLevel = chosenBoundaryLevelId
                },
                Charts = new List<IChart> {
                    new MapChart
                    {
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = chosenBoundaryLevelGeographicLevel.ToString()
                                        }
                                    }
                                }
                            }}
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.BoundaryLevel.AddRangeAsync(new List<BoundaryLevel>
                {
                    new()
                    {
                        Id = chosenBoundaryLevelId,
                        Label = "Boundary Level 1",
                        Level = chosenBoundaryLevelGeographicLevel
                    },
                    new()
                    {
                        Id = 2,
                        Label = "Boundary Level 2",
                        Level = GeographicLevel.Region
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var service = SetupService(contentDbContext, statisticsDbContext);
                    var result = await service.MigrateMaps(dryRun: false);
                    var mapMigrationPlans = result.AssertRight();
                    var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                    Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                    Assert.Equal(RetainChosenBoundaryLevel, mapMigrationPlan.Strategy);
                    Assert.Equal(chosenBoundaryLevelId, mapMigrationPlan.BoundaryLevelId);
                    Assert.Equal(chosenBoundaryLevelGeographicLevel, mapMigrationPlan.GeographicLevel);
                }
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Equal(chosenBoundaryLevelId, updatedDataBlock.Query.BoundaryLevel);
                Assert.Equal(chosenBoundaryLevelId, updatedMap.BoundaryLevel);
            }
        }
        
        [Fact]
        public async Task MigrateAllMaps_SingleGeographicLevelAppearsInDataSets()
        {
            var singleGeographicLevelInDataSets = GeographicLevel.LocalAuthority;
            var matchingGeoLevelBoundaryLevelId = 1;
            
            // This Data Block has no Boundary Level chosen yet.  It will be inferred from its Data Sets.
            var dataBlock = new DataBlock
            {
                Created = DateTime.UtcNow,
                Query = new ObservationQueryContext(),
                Charts = new List<IChart> {
                    new MapChart
                    {
                        // Here we have multiple Data Sets added to this Chart, but they all represent data for the
                        // same Geographic Level.  
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = singleGeographicLevelInDataSets.ToString()
                                        }
                                    },
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = singleGeographicLevelInDataSets.ToString()
                                        }
                                    }
                                }
                            }},
                            {"Key2", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = singleGeographicLevelInDataSets.ToString()
                                        }
                                    }
                                }
                            }}
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.BoundaryLevel.AddRangeAsync(new List<BoundaryLevel>
                {
                    new()
                    {
                        Id = matchingGeoLevelBoundaryLevelId,
                        Label = "Boundary Level 1",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(-1)
                    },
                    new()
                    {
                        Id = 2,
                        Label = "Boundary Level 2",
                        Level = GeographicLevel.Region,
                        Created = DateTime.UtcNow.AddYears(-1)
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var service = SetupService(contentDbContext, statisticsDbContext);
                    var result = await service.MigrateMaps(dryRun: false);
                    var mapMigrationPlans = result.AssertRight();
                    var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                    Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                    Assert.Equal(SetBoundaryLevelToMatchSingleDataSetGeoLevel, mapMigrationPlan.Strategy);
                    Assert.Equal(matchingGeoLevelBoundaryLevelId, mapMigrationPlan.BoundaryLevelId);
                    Assert.Equal(singleGeographicLevelInDataSets, mapMigrationPlan.GeographicLevel);
                }
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedDataBlock.Query.BoundaryLevel);
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedMap.BoundaryLevel);
            }
        }
        
        [Fact]
        public async Task MigrateAllMaps_CountryAndSingleOtherGeographicLevelAppearsInDataSets()
        {
            var singleOtherGeographicLevelInDataSets = GeographicLevel.LocalAuthority;
            var matchingGeoLevelBoundaryLevelId = 1;
            
            // This Data Block has no Boundary Level chosen yet.  It will be inferred from its Data Sets.
            var dataBlock = new DataBlock
            {
                Created = DateTime.UtcNow,
                Query = new ObservationQueryContext(),
                Charts = new List<IChart> {
                    new MapChart
                    {
                        // Here we have multiple Data Sets added to this Chart, but they all represent data for either
                        // "Country" or one other Geographic Level.  
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = singleOtherGeographicLevelInDataSets.ToString()
                                        }
                                    },
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = GeographicLevel.Country.ToString()
                                        }
                                    }
                                }
                            }},
                            {"Key2", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = singleOtherGeographicLevelInDataSets.ToString()
                                        }
                                    }
                                }
                            }}
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.BoundaryLevel.AddRangeAsync(new List<BoundaryLevel>
                {
                    new()
                    {
                        Id = matchingGeoLevelBoundaryLevelId,
                        Label = "Boundary Level 1",
                        Level = singleOtherGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(-1)
                    },
                    new()
                    {
                        Id = 2,
                        Label = "Boundary Level 2",
                        Level = GeographicLevel.Region,
                        Created = DateTime.UtcNow.AddYears(-1)
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var service = SetupService(contentDbContext, statisticsDbContext);
                    var result = await service.MigrateMaps(dryRun: false);
                    var mapMigrationPlans = result.AssertRight();
                    var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                    Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                    Assert.Equal(SetBoundaryLevelToMatchSingleNonCountryDataSetLevel, mapMigrationPlan.Strategy);
                    Assert.Equal(matchingGeoLevelBoundaryLevelId, mapMigrationPlan.BoundaryLevelId);
                    Assert.Equal(singleOtherGeographicLevelInDataSets, mapMigrationPlan.GeographicLevel);
                }
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedDataBlock.Query.BoundaryLevel);
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedMap.BoundaryLevel);
            }
        }
        
        [Fact]
        public async Task MigrateAllMaps_MultipleNonCountryGeographicLevelsAppearInDataSets()
        {
            var geographicLevel1InDataSets = GeographicLevel.LocalAuthority;
            var geographicLevel2InDataSets = GeographicLevel.LocalAuthorityDistrict;
            
            // This Data Block has no Boundary Level chosen yet.  It cannot be inferred from its Data Sets as
            // multiple Levels exist there.
            var dataBlock = new DataBlock
            {
                Created = DateTime.UtcNow,
                Query = new ObservationQueryContext(),
                Charts = new List<IChart> {
                    new MapChart
                    {
                        // Here we have multiple Data Sets added to this Chart, which represent data for different 
                        // Geographic Levels.  There are no clues as to which Geographic Level is suitable for this
                        // particular Map.
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = geographicLevel1InDataSets.ToString()
                                        }
                                    },
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = geographicLevel2InDataSets.ToString()
                                        }
                                    }
                                }
                            }},
                            {"Key2", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = geographicLevel1InDataSets.ToString()
                                        }
                                    }
                                }
                            }}
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();

            await using var statisticsDbContext = InMemoryStatisticsDbContext();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext, statisticsDbContext);
                var result = await service.MigrateMaps(dryRun: false);
                var mapMigrationPlans = result.AssertRight();
                var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                Assert.Equal(QuestionWhichCorrectGeoLevelToSelect, mapMigrationPlan.Strategy);
                Assert.Null(mapMigrationPlan.BoundaryLevelId);
                Assert.Null(mapMigrationPlan.GeographicLevel);
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Null(updatedDataBlock.Query.BoundaryLevel);
                Assert.Null(updatedMap.BoundaryLevel);
            }
        }
        
        [Fact]
        public async Task MigrateAllMaps_NoDataSetsAvailable()
        {
            // This Data Block has no Boundary Level chosen yet.  It cannot be inferred from its Data Sets as
            // it has none.
            var dataBlock = new DataBlock
            {
                Created = DateTime.UtcNow,
                Query = new ObservationQueryContext(),
                Charts = new List<IChart> {
                    new MapChart()
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();

            await using var statisticsDbContext = InMemoryStatisticsDbContext();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext, statisticsDbContext);
                var result = await service.MigrateMaps(dryRun: false);
                var mapMigrationPlans = result.AssertRight();
                var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                Assert.Equal(FixMapConfigNoDataSetGeographicLevels, mapMigrationPlan.Strategy);
                Assert.Null(mapMigrationPlan.BoundaryLevelId);
                Assert.Null(mapMigrationPlan.GeographicLevel);
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Null(updatedDataBlock.Query.BoundaryLevel);
                Assert.Null(updatedMap.BoundaryLevel);
            }
        }
        
        [Fact]
        public async Task MigrateAllMaps_InvalidGeographicLevelCodeInDataSet()
        {
            // This Data Block has no Boundary Level chosen yet.  It cannot be inferred from its Data Sets as
            // invalid Level codes exist there.
            var dataBlock = new DataBlock
            {
                Created = DateTime.UtcNow,
                Query = new ObservationQueryContext(),
                Charts = new List<IChart> {
                    new MapChart
                    {
                        // Here we have multiple Data Sets added to this Chart, which represent data for different 
                        // Geographic Levels.  There are no clues as to which Geographic Level is suitable for this
                        // particular Map.
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = GeographicLevel.Region.ToString()
                                        }
                                    },
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = "Invalid Level"
                                        }
                                    }
                                }
                            }}
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();

            await using var statisticsDbContext = InMemoryStatisticsDbContext();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext, statisticsDbContext);
                var result = await service.MigrateMaps(dryRun: false);
                var mapMigrationPlans = result.AssertRight();
                var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                Assert.Equal(FixMapConfigInvalidDataSetGeographicLevels, mapMigrationPlan.Strategy);
                Assert.Null(mapMigrationPlan.BoundaryLevelId);
                Assert.Null(mapMigrationPlan.GeographicLevel);
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Null(updatedDataBlock.Query.BoundaryLevel);
                Assert.Null(updatedMap.BoundaryLevel);
            }
        }
        
        [Fact]
        public async Task MigrateAllMaps_ChooseCorrectBoundaryLevelIdToMatchDataBlockCreationTime()
        {
            var singleGeographicLevelInDataSets = GeographicLevel.LocalAuthority;
            var matchingGeoLevelBoundaryLevelId = 2;
            
            var dataBlock = new DataBlock
            {
                // This Data Block has been created "now".  Therefore it will use the Boundary Level with the closest
                // Created Date before this date.
                Created = DateTime.UtcNow,
                Query = new ObservationQueryContext(),
                Charts = new List<IChart> {
                    new MapChart
                    {
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = singleGeographicLevelInDataSets.ToString()
                                        }
                                    }
                                }
                            }}
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId, updateTimestamps: false))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // There are several Boundary Levels available for the given Geographic Level.  The matching one
                // will be identified using the Created date on the Data Block (and of course a matching Geographic
                // Level).
                await statisticsDbContext.BoundaryLevel.AddRangeAsync(new List<BoundaryLevel>
                {
                    new()
                    {
                        Id = 1,
                        Label = "Boundary Level 1",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(-2)
                    },
                    new()
                    {
                        Id = matchingGeoLevelBoundaryLevelId,
                        Label = "Boundary Level 2",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(-1)
                    },
                    new()
                    {
                        Id = 3,
                        Label = "Boundary Level 3",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(1)
                    },
                    new()
                    {
                        Id = 4,
                        Label = "Boundary Level 4",
                        Level = GeographicLevel.Region,
                        Created = DateTime.UtcNow.AddYears(-1)
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var service = SetupService(contentDbContext, statisticsDbContext);
                    var result = await service.MigrateMaps(dryRun: false);
                    var mapMigrationPlans = result.AssertRight();
                    var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                    Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                    Assert.Equal(SetBoundaryLevelToMatchSingleDataSetGeoLevel, mapMigrationPlan.Strategy);
                    Assert.Equal(matchingGeoLevelBoundaryLevelId, mapMigrationPlan.BoundaryLevelId);
                    Assert.Equal(singleGeographicLevelInDataSets, mapMigrationPlan.GeographicLevel);
                }
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedDataBlock.Query.BoundaryLevel);
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedMap.BoundaryLevel);
            }
        }
        
        [Fact]
        public async Task MigrateAllMaps_ChooseCorrectBoundaryLevelIdToMatchDataBlockCreationTime_BeforeFirstBoundaryLevel()
        {
            var singleGeographicLevelInDataSets = GeographicLevel.LocalAuthority;
            var matchingGeoLevelBoundaryLevelId = 1;
            
            var dataBlock = new DataBlock
            {
                // This Data Block has been created prior to any of our Boundary Levels' Created dates.
                // Therefore it will use the first Boundary Level with a matching Geographic Level.
                Created = DateTime.UtcNow.AddYears(-5),
                Query = new ObservationQueryContext(),
                Charts = new List<IChart> {
                    new MapChart
                    {
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = singleGeographicLevelInDataSets.ToString()
                                        }
                                    }
                                }
                            }}
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId, updateTimestamps: false))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // There are several Boundary Levels available for the given Geographic Level.  The matching one
                // will be identified using the Created date on the Data Block.
                await statisticsDbContext.BoundaryLevel.AddRangeAsync(new List<BoundaryLevel>
                {
                    new()
                    {
                        Id = matchingGeoLevelBoundaryLevelId,
                        Label = "Boundary Level 1",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(-2)
                    },
                    new()
                    {
                        Id = 2,
                        Label = "Boundary Level 2",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(-1)
                    },
                    new()
                    {
                        Id = 3,
                        Label = "Boundary Level 3",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(1)
                    },
                    new()
                    {
                        Id = 4,
                        Label = "Boundary Level 4",
                        Level = GeographicLevel.Region,
                        Created = DateTime.UtcNow.AddYears(-1)
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var service = SetupService(contentDbContext, statisticsDbContext);
                    var result = await service.MigrateMaps(dryRun: false);
                    var mapMigrationPlans = result.AssertRight();
                    var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                    Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                    Assert.Equal(SetBoundaryLevelToMatchSingleDataSetGeoLevel, mapMigrationPlan.Strategy);
                    Assert.Equal(matchingGeoLevelBoundaryLevelId, mapMigrationPlan.BoundaryLevelId);
                    Assert.Equal(singleGeographicLevelInDataSets, mapMigrationPlan.GeographicLevel);
                }
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedDataBlock.Query.BoundaryLevel);
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedMap.BoundaryLevel);
            }
        }
        
        [Fact]
        public async Task MigrateAllMaps_ChooseCorrectBoundaryLevelIdToMatchDataBlockCreationTime_AfterLastBoundaryLevel()
        {
            var singleGeographicLevelInDataSets = GeographicLevel.LocalAuthority;
            var matchingGeoLevelBoundaryLevelId = 3;
            
            var dataBlock = new DataBlock
            {
                // This Data Block has been created after any of our Boundary Levels' Created dates.
                // Therefore it will use the last Boundary Level with a matching Geographic Level.
                Created = DateTime.UtcNow.AddYears(5),
                Query = new ObservationQueryContext(),
                Charts = new List<IChart> {
                    new MapChart
                    {
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = singleGeographicLevelInDataSets.ToString()
                                        }
                                    }
                                }
                            }}
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId, updateTimestamps: false))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // There are several Boundary Levels available for the given Geographic Level.  The matching one
                // will be identified using the Created date on the Data Block.
                await statisticsDbContext.BoundaryLevel.AddRangeAsync(new List<BoundaryLevel>
                {
                    new()
                    {
                        Id = 1,
                        Label = "Boundary Level 1",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(-2)
                    },
                    new()
                    {
                        Id = 2,
                        Label = "Boundary Level 2",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(-1)
                    },
                    new()
                    {
                        Id = matchingGeoLevelBoundaryLevelId,
                        Label = "Boundary Level 3",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(1)
                    },
                    new()
                    {
                        Id = 4,
                        Label = "Boundary Level 4",
                        Level = GeographicLevel.Region,
                        Created = DateTime.UtcNow.AddYears(-1)
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var service = SetupService(contentDbContext, statisticsDbContext);
                    var result = await service.MigrateMaps(dryRun: false);
                    var mapMigrationPlans = result.AssertRight();
                    var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                    Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                    Assert.Equal(SetBoundaryLevelToMatchSingleDataSetGeoLevel, mapMigrationPlan.Strategy);
                    Assert.Equal(matchingGeoLevelBoundaryLevelId, mapMigrationPlan.BoundaryLevelId);
                    Assert.Equal(singleGeographicLevelInDataSets, mapMigrationPlan.GeographicLevel);
                }
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedDataBlock.Query.BoundaryLevel);
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedMap.BoundaryLevel);
            }
        }
        
        [Fact]
        public async Task MigrateAllMaps_ChooseCorrectBoundaryLevelIdToMatchReleaseCreationTime_ViaReleaseContentBlock()
        {
            var singleGeographicLevelInDataSets = GeographicLevel.LocalAuthority;
            var matchingGeoLevelBoundaryLevelId = 2;
            
            var dataBlock = new DataBlock
            {
                // This Data Block has no Created Date.  It will be inferred from its owning Release's Creation date
                // instead, as found via ReleaseContentBlock -> ContentBlock.
                Query = new ObservationQueryContext(),
                Charts = new List<IChart> {
                    new MapChart
                    {
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = singleGeographicLevelInDataSets.ToString()
                                        }
                                    }
                                }
                            }}
                        }
                    }
                }
            };

            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = new Release
                {
                    Created = DateTime.UtcNow
                },
                ContentBlock = dataBlock
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId, updateTimestamps: false))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.ReleaseContentBlocks.AddAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // There are several Boundary Levels available for the given Geographic Level.  The matching one
                // will be identified using the Created date on the Data Block's owning Release.
                await statisticsDbContext.BoundaryLevel.AddRangeAsync(new List<BoundaryLevel>
                {
                    new()
                    {
                        Id = 1,
                        Label = "Boundary Level 1",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(-2)
                    },
                    new()
                    {
                        Id = matchingGeoLevelBoundaryLevelId,
                        Label = "Boundary Level 2",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(-1)
                    },
                    new()
                    {
                        Id = 3,
                        Label = "Boundary Level 3",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(1)
                    },
                    new()
                    {
                        Id = 4,
                        Label = "Boundary Level 4",
                        Level = GeographicLevel.Region,
                        Created = DateTime.UtcNow.AddYears(-1)
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var service = SetupService(contentDbContext, statisticsDbContext);
                    var result = await service.MigrateMaps(dryRun: false);
                    var mapMigrationPlans = result.AssertRight();
                    var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                    Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                    Assert.Equal(SetBoundaryLevelToMatchSingleDataSetGeoLevel, mapMigrationPlan.Strategy);
                    Assert.Equal(matchingGeoLevelBoundaryLevelId, mapMigrationPlan.BoundaryLevelId);
                    Assert.Equal(singleGeographicLevelInDataSets, mapMigrationPlan.GeographicLevel);
                }
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedDataBlock.Query.BoundaryLevel);
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedMap.BoundaryLevel);
            }
        }
        
        [Fact]
        public async Task MigrateAllMaps_ChooseCorrectBoundaryLevelIdToMatchReleaseCreationTime_ViaReleaseContentSection()
        {
            var singleGeographicLevelInDataSets = GeographicLevel.LocalAuthority;
            var matchingGeoLevelBoundaryLevelId = 1;
            
            var dataBlock = new DataBlock
            {
                // This Data Block has no Created Date.  It will be inferred from its owning Release's Creation date
                // instead, as found via ReleaseContentSection -> ContentSection -> ContentBlock.
                Query = new ObservationQueryContext(),
                Charts = new List<IChart> {
                    new MapChart
                    {
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = singleGeographicLevelInDataSets.ToString()
                                        }
                                    }
                                }
                            }}
                        }
                    }
                }
            };

            var releaseContentSection = new ReleaseContentSection
            {
                Release = new Release
                {
                    Created = DateTime.UtcNow.AddDays(-400)
                },
                ContentSection = new ContentSection
                {
                    Content = new List<ContentBlock>
                    {
                        new HtmlBlock
                        {
                            Id = Guid.NewGuid()
                        },
                        dataBlock,
                        new HtmlBlock
                        {
                            Id = Guid.NewGuid()
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId, updateTimestamps: false))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.ReleaseContentSections.AddAsync(releaseContentSection);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // There are several Boundary Levels available for the given Geographic Level.  The matching one
                // will be identified using the Created date on the Data Block's owning Release.
                await statisticsDbContext.BoundaryLevel.AddRangeAsync(new List<BoundaryLevel>
                {
                    new()
                    {
                        Id = matchingGeoLevelBoundaryLevelId,
                        Label = "Boundary Level 1",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(-2)
                    },
                    new()
                    {
                        Id = 2,
                        Label = "Boundary Level 2",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(-1)
                    },
                    new()
                    {
                        Id = 3,
                        Label = "Boundary Level 3",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(1)
                    },
                    new()
                    {
                        Id = 4,
                        Label = "Boundary Level 4",
                        Level = GeographicLevel.Region,
                        Created = DateTime.UtcNow.AddYears(-1)
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var service = SetupService(contentDbContext, statisticsDbContext);
                    var result = await service.MigrateMaps(dryRun: false);
                    var mapMigrationPlans = result.AssertRight();
                    var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                    Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                    Assert.Equal(SetBoundaryLevelToMatchSingleDataSetGeoLevel, mapMigrationPlan.Strategy);
                    Assert.Equal(matchingGeoLevelBoundaryLevelId, mapMigrationPlan.BoundaryLevelId);
                    Assert.Equal(singleGeographicLevelInDataSets, mapMigrationPlan.GeographicLevel);
                }
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedDataBlock.Query.BoundaryLevel);
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedMap.BoundaryLevel);
            }
        }
        
        [Fact]
        public async Task MigrateAllMaps_ChooseCorrectBoundaryLevelIdToMatchReleaseCreationTime_NoPathToRelease()
        {
            var singleGeographicLevelInDataSets = GeographicLevel.LocalAuthority;
            var matchingGeoLevelBoundaryLevelId = 2;
            
            var dataBlock = new DataBlock
            {
                // This Data Block has no Created Date.  It also can't be inferred from an owning Release's created date
                // as no path back to the owning Release is available.  Therefore it will default to using UtcNow as the
                // created date.
                Query = new ObservationQueryContext(),
                Charts = new List<IChart> {
                    new MapChart
                    {
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = singleGeographicLevelInDataSets.ToString()
                                        }
                                    }
                                }
                            }}
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId, updateTimestamps: false))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // There are several Boundary Levels available for the given Geographic Level.  The matching one
                // will be identified using UtcNow as the fallback Created date.
                await statisticsDbContext.BoundaryLevel.AddRangeAsync(new List<BoundaryLevel>
                {
                    new()
                    {
                        Id = 1,
                        Label = "Boundary Level 1",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(-2)
                    },
                    new()
                    {
                        Id = matchingGeoLevelBoundaryLevelId,
                        Label = "Boundary Level 2",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(-1)
                    },
                    new()
                    {
                        Id = 3,
                        Label = "Boundary Level 3",
                        Level = singleGeographicLevelInDataSets,
                        Created = DateTime.UtcNow.AddYears(1)
                    },
                    new()
                    {
                        Id = 4,
                        Label = "Boundary Level 4",
                        Level = GeographicLevel.Region,
                        Created = DateTime.UtcNow.AddYears(-1)
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var service = SetupService(contentDbContext, statisticsDbContext);
                    var result = await service.MigrateMaps(dryRun: false);
                    var mapMigrationPlans = result.AssertRight();
                    var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                    Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                    Assert.Equal(SetBoundaryLevelToMatchSingleDataSetGeoLevel, mapMigrationPlan.Strategy);
                    Assert.Equal(matchingGeoLevelBoundaryLevelId, mapMigrationPlan.BoundaryLevelId);
                    Assert.Equal(singleGeographicLevelInDataSets, mapMigrationPlan.GeographicLevel);
                }
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedDataBlock.Query.BoundaryLevel);
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedMap.BoundaryLevel);
            }
        }
        
        [Fact]
        public async Task MigrateAllMaps_DataSetsWithNoGeographicLevelSpecified_InferGeographicLevelFromDataBlockLocations_SingleGeographicLevelAndCountry()
        {
            var singleGeographicLevelInLocations = GeographicLevel.LocalAuthority;
            var matchingGeoLevelBoundaryLevelId = 1;
            var location1Id = Guid.NewGuid();
            var location2Id = Guid.NewGuid();

            // This Data Block has no Boundary Level chosen yet.
            // It will be inferred from its Locations' Geographic Levels as the Data Sets don't
            // have specific Geographic Levels set.
            var dataBlock = new DataBlock
            {
                Created = DateTime.UtcNow,
                Query = new ObservationQueryContext
                {
                    LocationIds = ListOf(location1Id, location2Id)
                },
                Charts = new List<IChart> {
                    new MapChart
                    {
                        // Here we have multiple Data Sets added to this Chart, but they all represent data for either
                        // "Country" or one other Geographic Level.  
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                }
                            }}
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }
            
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Location.AddRangeAsync(
                    new Location
                    {
                        Id = location1Id,
                        GeographicLevel = GeographicLevel.LocalAuthority
                    }, 
                    new Location
                    {
                        Id = location2Id,
                        GeographicLevel = GeographicLevel.LocalAuthority
                    }, 
                    new Location
                    {
                        Id = Guid.NewGuid(),
                        GeographicLevel = GeographicLevel.Region
                    });
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.BoundaryLevel.AddRangeAsync(new List<BoundaryLevel>
                {
                    new()
                    {
                        Id = matchingGeoLevelBoundaryLevelId,
                        Label = "Boundary Level 1",
                        Level = singleGeographicLevelInLocations,
                        Created = DateTime.UtcNow.AddYears(-1)
                    },
                    new()
                    {
                        Id = 2,
                        Label = "Boundary Level 2",
                        Level = GeographicLevel.Region,
                        Created = DateTime.UtcNow.AddYears(-1)
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var service = SetupService(contentDbContext, statisticsDbContext);
                    var result = await service.MigrateMaps(dryRun: false);
                    var mapMigrationPlans = result.AssertRight();
                    var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                    Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                    Assert.Equal(SetBoundaryLevelToMatchSingleDataSetGeoLevel, mapMigrationPlan.Strategy);
                    Assert.Equal(matchingGeoLevelBoundaryLevelId, mapMigrationPlan.BoundaryLevelId);
                    Assert.Equal(singleGeographicLevelInLocations, mapMigrationPlan.GeographicLevel);
                }
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedDataBlock.Query.BoundaryLevel);
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedMap.BoundaryLevel);
            }
        }

        [Fact]
        public async Task MigrateAllMaps_InferGeographicLevelFromDataBlockLocations()
        {
            var singleGeographicLevelInLocations = GeographicLevel.LocalAuthority;
            var matchingGeoLevelBoundaryLevelId = 1;
            var location1Id = Guid.NewGuid();
            var location2Id = Guid.NewGuid();

            // This Data Block has no Boundary Level chosen yet.
            // It will be inferred from its Locations' Geographic Levels as the Data Sets don't
            // have specific Geographic Levels set.
            var dataBlock = new DataBlock
            {
                Created = DateTime.UtcNow,
                Query = new ObservationQueryContext
                {
                    LocationIds = ListOf(location1Id, location2Id)
                },
                Charts = new List<IChart> {
                    new MapChart
                    {
                        // Here we have multiple Data Sets added to this Chart, but they all represent data for either
                        // "Country" or one other Geographic Level.  
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                }
                            }}
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }
            
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Location.AddRangeAsync(
                    new Location
                    {
                        Id = location1Id,
                        GeographicLevel = GeographicLevel.LocalAuthority
                    }, 
                    new Location
                    {
                        Id = location2Id,
                        GeographicLevel = GeographicLevel.LocalAuthority
                    }, 
                    new Location
                    {
                        Id = Guid.NewGuid(),
                        GeographicLevel = GeographicLevel.Region
                    });
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.BoundaryLevel.AddRangeAsync(new List<BoundaryLevel>
                {
                    new()
                    {
                        Id = matchingGeoLevelBoundaryLevelId,
                        Label = "Boundary Level 1",
                        Level = singleGeographicLevelInLocations,
                        Created = DateTime.UtcNow.AddYears(-1)
                    },
                    new()
                    {
                        Id = 2,
                        Label = "Boundary Level 2",
                        Level = GeographicLevel.Region,
                        Created = DateTime.UtcNow.AddYears(-1)
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var service = SetupService(contentDbContext, statisticsDbContext);
                    var result = await service.MigrateMaps(dryRun: false);
                    var mapMigrationPlans = result.AssertRight();
                    var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                    Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                    Assert.Equal(SetBoundaryLevelToMatchSingleDataSetGeoLevel, mapMigrationPlan.Strategy);
                    Assert.Equal(matchingGeoLevelBoundaryLevelId, mapMigrationPlan.BoundaryLevelId);
                    Assert.Equal(singleGeographicLevelInLocations, mapMigrationPlan.GeographicLevel);
                }
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedDataBlock.Query.BoundaryLevel);
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedMap.BoundaryLevel);
            }
        }

        [Fact]
        public async Task MigrateAllMaps_InferGeographicLevelFromDataBlockLocations_CountryAndSingleOtherGeographicLevel()
        {
            var singleNonCountryGeographicLevelInLocations = GeographicLevel.LocalAuthority;
            var matchingGeoLevelBoundaryLevelId = 1;
            var location1Id = Guid.NewGuid();
            var location2Id = Guid.NewGuid();

            // This Data Block has no Boundary Level chosen yet.
            // It will be inferred from its Locations' Geographic Levels as the Data Sets don't
            // have specific Geographic Levels set.
            var dataBlock = new DataBlock
            {
                Created = DateTime.UtcNow,
                Query = new ObservationQueryContext
                {
                    LocationIds = ListOf(location1Id, location2Id)
                },
                Charts = new List<IChart> {
                    new MapChart
                    {
                        // Here we have multiple Data Sets added to this Chart, but they all represent data for either
                        // "Country" or one other Geographic Level.  
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                }
                            }}
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }
            
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Location.AddRangeAsync(
                    new Location
                    {
                        Id = location1Id,
                        GeographicLevel = GeographicLevel.LocalAuthority
                    }, 
                    new Location
                    {
                        Id = location2Id,
                        GeographicLevel = GeographicLevel.Country
                    }, 
                    new Location
                    {
                        Id = Guid.NewGuid(),
                        GeographicLevel = GeographicLevel.Region
                    });
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.BoundaryLevel.AddRangeAsync(new List<BoundaryLevel>
                {
                    new()
                    {
                        Id = matchingGeoLevelBoundaryLevelId,
                        Label = "Boundary Level 1",
                        Level = singleNonCountryGeographicLevelInLocations,
                        Created = DateTime.UtcNow.AddYears(-1)
                    },
                    new()
                    {
                        Id = 2,
                        Label = "Boundary Level 2",
                        Level = GeographicLevel.Region,
                        Created = DateTime.UtcNow.AddYears(-1)
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var service = SetupService(contentDbContext, statisticsDbContext);
                    var result = await service.MigrateMaps(dryRun: false);
                    var mapMigrationPlans = result.AssertRight();
                    var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                    Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                    Assert.Equal(SetBoundaryLevelToMatchSingleNonCountryDataSetLevel, mapMigrationPlan.Strategy);
                    Assert.Equal(matchingGeoLevelBoundaryLevelId, mapMigrationPlan.BoundaryLevelId);
                    Assert.Equal(singleNonCountryGeographicLevelInLocations, mapMigrationPlan.GeographicLevel);
                }
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedDataBlock.Query.BoundaryLevel);
                Assert.Equal(matchingGeoLevelBoundaryLevelId, updatedMap.BoundaryLevel);
            }
        }

        [Fact]
        public async Task MigrateAllMaps_InferGeographicLevelFromDataBlockLocations_MultipleNonCountryGeographicLevels()
        {
            var nonCountryGeographicLevel1InLocations = GeographicLevel.LocalAuthority;
            var nonCountryGeographicLevel2InLocations = GeographicLevel.LocalAuthorityDistrict;
            var location1Id = Guid.NewGuid();
            var location2Id = Guid.NewGuid();

            // This Data Block has no Boundary Level chosen yet.
            // It will be inferred from its Locations' Geographic Levels as the Data Sets don't
            // have specific Geographic Levels set.
            var dataBlock = new DataBlock
            {
                Created = DateTime.UtcNow,
                Query = new ObservationQueryContext
                {
                    LocationIds = ListOf(location1Id, location2Id)
                },
                Charts = new List<IChart> {
                    new MapChart
                    {
                        // Here we have multiple Data Sets added to this Chart, but they all represent data for either
                        // "Country" or one other Geographic Level.  
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {"Key1", new()
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                }
                            }}
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }
            
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Location.AddRangeAsync(
                    new Location
                    {
                        Id = location1Id,
                        GeographicLevel = nonCountryGeographicLevel1InLocations
                    }, 
                    new Location
                    {
                        Id = location2Id,
                        GeographicLevel = nonCountryGeographicLevel2InLocations
                    });
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.BoundaryLevel.AddRangeAsync(new List<BoundaryLevel>
                {
                    new()
                    {
                        Id = 1,
                        Label = "Boundary Level 1",
                        Level = nonCountryGeographicLevel1InLocations,
                        Created = DateTime.UtcNow.AddYears(-1)
                    },
                    new()
                    {
                        Id = 2,
                        Label = "Boundary Level 2",
                        Level = GeographicLevel.Region,
                        Created = DateTime.UtcNow.AddYears(-1)
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                {
                    var service = SetupService(contentDbContext, statisticsDbContext);
                    var result = await service.MigrateMaps(dryRun: false);
                    var mapMigrationPlans = result.AssertRight();
                    var mapMigrationPlan = Assert.Single(mapMigrationPlans);

                    Assert.Equal(dataBlock.Id, mapMigrationPlan.DataBlockId);
                    Assert.Equal(QuestionWhichCorrectGeoLevelToSelect, mapMigrationPlan.Strategy);
                    Assert.Null(mapMigrationPlan.BoundaryLevelId);
                    Assert.Null(mapMigrationPlan.GeographicLevel);
                }
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedDataBlock = await contentDbContext
                    .DataBlocks
                    .SingleAsync(db => db.Id == dataBlock.Id);

                var updatedMap = (updatedDataBlock.Charts[0] as MapChart)!;
                
                Assert.Null(updatedDataBlock.Query.BoundaryLevel);
                Assert.Null(updatedMap.BoundaryLevel);
            }
        }

        private static DataBlockMigrationService SetupService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IUserService? userService = null)
        {
            return new DataBlockMigrationService(
                contentDbContext,
                statisticsDbContext,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                Mock.Of<ILogger<DataBlockMigrationService>>()
            );
        }
    }
}
