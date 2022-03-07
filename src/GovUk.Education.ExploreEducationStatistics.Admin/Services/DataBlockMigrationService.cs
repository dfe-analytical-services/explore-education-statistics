#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.DataBlockMigrationService.DataBlockMapMigrationPlan.MigrationStrategy;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.DataBlockMigrationService.DataBlockMapMigrationPlan.MigrationType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataBlockMigrationService : IDataBlockMigrationService
    {
        private readonly ContentDbContext _context;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IStorageQueueService _storageQueueService;
        private readonly IUserService _userService;
        private readonly ILogger<DataBlockMigrationService> _logger;

        public DataBlockMigrationService(
            ContentDbContext context,
            StatisticsDbContext statisticsDbContext,
            IStorageQueueService storageQueueService,
            IUserService userService,
            ILogger<DataBlockMigrationService> logger)
        {
            _context = context;
            _statisticsDbContext = statisticsDbContext;
            _storageQueueService = storageQueueService;
            _userService = userService;
            _logger = logger;
            _statisticsDbContext = statisticsDbContext;
        }

        public async Task<Either<ActionResult, Unit>> MigrateAll()
        {
            return await _userService.CheckCanRunMigrations()
                .OnSuccess<ActionResult, Unit, Unit>(async _ =>
                {
                    var messageCount = await _storageQueueService.GetApproximateMessageCount(MigrateDataBlocksQueue);

                    switch (messageCount)
                    {
                        case null:
                            return new BadRequestObjectResult(
                                $"Unexpected null message count for queue {MigrateDataBlocksQueue}");
                        case > 0:
                            return new BadRequestObjectResult(
                                $"Found non-empty queue {MigrateDataBlocksQueue}. Message count: {messageCount}");
                    }

                    // Create migration queue messages for all data blocks that have not already been marked as migrated 
                    var dataBlocks = await _context.DataBlocks
                        .AsNoTracking()
                        .Where(dataBlock => dataBlock.LocationsMigrated == null || !dataBlock.LocationsMigrated)
                        .Select(dataBlock => new MigrateDataBlockMessage(dataBlock.Id))
                        .ToListAsync();

                    if (dataBlocks.Any())
                    {
                        _logger.LogInformation("Adding {count} messages to the '{queue}' queue", dataBlocks.Count,
                            MigrateDataBlocksQueue);
                        await _storageQueueService.AddMessages(MigrateDataBlocksQueue, dataBlocks);
                    }

                    return Unit.Instance;
                });
        }

        public async Task<Either<ActionResult, List<DataBlockMapMigrationPlan>>> GetMigrateMapPlans()
        {
            // return 
            //     await _userService
            //     .CheckCanRunMigrations()
            //     .OnSuccess(async _ =>
            //     {
                    var locations = GetAllLocations();
                    var boundaryLevels = await GetAllBoundaryLevels();
                    var dataBlocksWithMaps = await GetAllDataBlocksWithMaps();

                    var plans = await dataBlocksWithMaps
                        .ToAsyncEnumerable()
                        .SelectAwait(async db =>
                        {
                            var specificBoundaryLevel = db.Query.BoundaryLevel;
                            
                            var dataBlockLocationsQuery =
                                LocationPredicateBuilder
                                    .Build(db.Query.LocationIds?.ToList(), db.Query.Locations);

                            var dataBlockGeographicLevels = await locations
                                .Where(dataBlockLocationsQuery)
                                .Select(l => l.GeographicLevel)
                                .Distinct()
                                .ToListAsync();

                            return new DataBlockMapMigrationPlan(
                                db.Id, 
                                specificBoundaryLevel != null 
                                    ? boundaryLevels.Find(bl => bl.Id == specificBoundaryLevel)
                                    : null,
                                dataBlockGeographicLevels,
                                db.Charts[0].Axes?.Values.ToList());
                        })
                        .ToListAsync();

                    return plans;
                // });
        }

        private async Task<List<DataBlock>> GetAllDataBlocksWithMaps()
        {
            return (await _context
                    .DataBlocks
                    .AsNoTracking()
                    .ToListAsync()
                )
                .Where(db => db.Charts.Count > 0 && db.Charts[0].Type == ChartType.Map)
                .ToList();
        }

        private Task<List<BoundaryLevel>> GetAllBoundaryLevels()
        {
            return _statisticsDbContext
                .BoundaryLevel
                .AsNoTracking()
                .ToListAsync();
        }

        private IQueryable<Location> GetAllLocations()
        {
            var locations = _statisticsDbContext
                .Location
                .AsNoTracking();
            return locations;
        }

        public async Task<Either<ActionResult, List<DataBlockMapMigrationPlan>>> MigrateMaps()
        {
            // return 
            //     await _userService
            //     .CheckCanRunMigrations()
            //     .OnSuccess(async _ =>
            //     {
            
            var boundaryLevels = await GetAllBoundaryLevels();
            var boundaryLevelDateRanges = GetBoundaryLevelsWithDateRanges();

            return
                GetMigrateMapPlans()
                    .OnSuccess(plans =>
                    {
                        plans
                            .ToAsyncEnumerable()
                            .ForEachAwaitAsync(async plan =>
                            {
                                var dataBlock = await _context
                                    .DataBlocks
                                    .SingleAsync(db => db.Id == plan.DataBlockId);

                                var map = (MapChart) dataBlock.Charts[0];
                                
                                switch (plan.Strategy)
                                {
                                    case RetainChosenBoundaryLevel:
                                    {
                                        map.BoundaryLevel = dataBlock.Query.BoundaryLevel ?? 0;
                                        break;
                                    }
                                    case SetBoundaryLevelToMatchSingleDataSetGeoLevel:
                                    {
                                        var geographicLevel = plan
                                            .DataSetGeographicLevels
                                            .Single();
                                        
                                        var boundaryLevel = GetLatestBoundaryLevelAtCreationTime(
                                            dataBlock.Created, 
                                            geographicLevel,
                                            boundaryLevels);
                                        
                                        dataBlock.Query.BoundaryLevel = boundaryLevel.Id;
                                        map.BoundaryLevel = boundaryLevel.Id;
                                        break;
                                    }
                                    case SetBoundaryLevelToMatchSingleNonCountryDataSetLevel:
                                    {
                                        var nonCountryGeographicLevel = plan
                                            .DataSetGeographicLevels
                                            .Single(geographicLevel => geographicLevel != GeographicLevel.Country);
                                        
                                        var boundaryLevel = GetLatestBoundaryLevelAtCreationTime(
                                            dataBlock.Created, 
                                            nonCountryGeographicLevel,
                                            boundaryLevels);
                                        
                                        dataBlock.Query.BoundaryLevel = boundaryLevel.Id;
                                        map.BoundaryLevel = boundaryLevel.Id;
                                        break;
                                    }
                                    case QuestionWhichCorrectGeoLevelToSelect:
                                        break;
                                    case FixMapConfigNoDataSetGeographicLevels:
                                        break;
                                    default: 
                                        throw new ArgumentOutOfRangeException($"Unknown migration strategy {plan.Strategy}");
                                }
                            });
                    });
        }

        private Dictionary<
            (
                GeographicLevel geographicLevel, 
                DateTime start, 
                DateTime end
            ), 
            BoundaryLevel> 
            GetBoundaryLevelsWithDateRanges(List<BoundaryLevel> boundaryLevels)
        {
            return boundaryLevels
                .ToDictionary(boundaryLevel => boundaryLevel.Label)
        }

        private BoundaryLevel GetLatestBoundaryLevelAtCreationTime(
            DateTime? dataBlockCreated,
            GeographicLevel geographicLevel, 
            List<BoundaryLevel> boundaryLevels)
        {
            boundaryLevels.SingleOrDefault(bl => bl.Published)
            throw new NotImplementedException();
        }

        public record DataBlockMapMigrationPlan
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum MigrationType
            {
                SpecificBoundarySetAndSingleGeoLevelInDataSets,
                SpecificBoundarySetAndMultipleGeoLevelsInDataSets,
                NoSpecificBoundarySetAndSingleGeoLevelInDataSets,
                NoSpecificBoundarySetAndCountryAndAnotherGeoLevelInDataSets,
                NoSpecificBoundarySetAndMultiNonCountryGeoLevelsInDataSets,
                Undefined
            }
            
            [JsonConverter(typeof(StringEnumConverter))]
            public enum MigrationStrategy
            {
                RetainChosenBoundaryLevel,
                SetBoundaryLevelToMatchSingleDataSetGeoLevel,
                SetBoundaryLevelToMatchSingleNonCountryDataSetLevel,
                QuestionWhichCorrectGeoLevelToSelect,
                FixMapConfigNoDataSetGeographicLevels
            }
            
            public Guid DataBlockId { get; }

            public MigrationType Type { get; } = Undefined;

            public MigrationStrategy Strategy { get; } = FixMapConfigNoDataSetGeographicLevels;

            public long? SpecificBoundaryLevelId { get; }
            [JsonConverter(typeof(StringEnumConverter))]
            public GeographicLevel? SpecificBoundaryLevelGeographicLevel { get; }

            [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
            public List<GeographicLevel> DataBlockGeographicLevels { get; }

            [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
            public List<GeographicLevel> DataSetGeographicLevels { get; }

            public DataBlockMapMigrationPlan(Guid dataBlockId, BoundaryLevel? specificBoundaryLevel,
                List<GeographicLevel> dataBlockGeographicLevels, 
                List<ChartAxisConfiguration>? chartAxisConfigurations)
            {
                DataBlockId = dataBlockId;
                SpecificBoundaryLevelGeographicLevel = specificBoundaryLevel?.Level;
                SpecificBoundaryLevelId = specificBoundaryLevel?.Id;
                DataBlockGeographicLevels = dataBlockGeographicLevels;
                
                var dataSets = chartAxisConfigurations
                    .SelectMany(axisConfig => axisConfig.DataSets)
                    .ToList();

                DataSetGeographicLevels = dataSets
                    .SelectMany(dataSet =>
                    {
                        var level = dataSet.Location?.Level;

                        if (level == null)
                        {
                            return DataBlockGeographicLevels;
                        }

                        if (!Enum.TryParse<GeographicLevel>(level, ignoreCase: true, out var matchedLevel))
                        {
                            throw new ArgumentException($"Invalid Level in Data Set for block {dataBlockId}: {level}");
                        }
                        
                        return ListOf(matchedLevel);
                    })
                    .Distinct()
                    .ToList();

                if (specificBoundaryLevel != null)
                {
                    if (DataSetGeographicLevels.Count == 0)
                    {
                        Type = Undefined;
                        Strategy = FixMapConfigNoDataSetGeographicLevels;
                    }
                    else if (DataSetGeographicLevels.Count == 1)
                    {
                        Type = SpecificBoundarySetAndSingleGeoLevelInDataSets;
                        Strategy = RetainChosenBoundaryLevel;
                    }
                    else
                    {
                        Type = SpecificBoundarySetAndMultipleGeoLevelsInDataSets;
                        Strategy = RetainChosenBoundaryLevel;
                    }
                }
                else
                {
                    if (DataSetGeographicLevels.Count == 0)
                    {
                        Type = Undefined;
                        Strategy = FixMapConfigNoDataSetGeographicLevels;
                    }
                    else if (DataSetGeographicLevels.Count == 1)
                    {
                        Type = NoSpecificBoundarySetAndSingleGeoLevelInDataSets;
                        Strategy = SetBoundaryLevelToMatchSingleDataSetGeoLevel;
                    }
                    else if (DataSetGeographicLevels.Count == 2)
                    {
                        if (DataSetGeographicLevels.Contains(GeographicLevel.Country))
                        {
                            Type = NoSpecificBoundarySetAndCountryAndAnotherGeoLevelInDataSets;
                            Strategy = SetBoundaryLevelToMatchSingleNonCountryDataSetLevel;
                        }
                        else
                        {
                            Type = NoSpecificBoundarySetAndMultiNonCountryGeoLevelsInDataSets;
                            Strategy = QuestionWhichCorrectGeoLevelToSelect;
                        }
                    } 
                    else
                    {
                        Type = NoSpecificBoundarySetAndMultiNonCountryGeoLevelsInDataSets;
                        Strategy = QuestionWhichCorrectGeoLevelToSelect;
                    }
                }
            }
        }
    }
}
