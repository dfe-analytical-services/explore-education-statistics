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
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.DataBlockMigrationService.DataBlockMapMigrationPlan.MigrationStrategy;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.DataBlockMigrationService.DataBlockMapMigrationPlan.MigrationType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataBlockMigrationService : IDataBlockMigrationService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IUserService _userService;
        private readonly ILogger<DataBlockMigrationService> _logger;

        public DataBlockMigrationService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext, 
            IUserService userService, 
            ILogger<DataBlockMigrationService> logger)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _userService = userService;
            _logger = logger;
        }

        public async Task<Either<ActionResult, List<DataBlockMapMigrationPlan>>> GetMigrateMapPlans()
        {
            var locations = GetAllLocations();
            var boundaryLevels = await GetAllBoundaryLevels();
            var mapsNeedingMigration = (await GetAllDataBlocksWithMaps())
                .Where(dataBlock => 
                    (dataBlock.Charts[0] as MapChart)?.BoundaryLevel == null 
                    || dataBlock.Query.BoundaryLevel == null);

            var plans = await mapsNeedingMigration
                .ToAsyncEnumerable()
                .SelectAwait(async db =>
                {
                    var specificBoundaryLevelId = db.Query.BoundaryLevel;
                    
                    var dataBlockGeographicLevels = await locations
                        .Where(l => db.Query.LocationIds.Contains(l.Id))
                        .Select(l => l.GeographicLevel)
                        .Distinct()
                        .ToListAsync();

                    return new DataBlockMapMigrationPlan(
                        db.Id, 
                        specificBoundaryLevelId != null 
                            ? boundaryLevels.Single(bl => bl.Id == specificBoundaryLevelId)
                            : null,
                        dataBlockGeographicLevels,
                        db.Charts[0].Axes?.Values.ToList());
                })
                .ToListAsync();

            return plans;
        }

        public async Task<Either<ActionResult, List<MapMigrationResult>>> MigrateMaps(bool dryRun = true)
        {
            return await _userService
                .CheckIsBauUser()
                .OnSuccess(() => DoMapMigration(dryRun));
        }

        private async Task<Either<ActionResult, List<MapMigrationResult>>> DoMapMigration(bool dryRun)
        {
            var boundaryLevels = await GetAllBoundaryLevels();

            return await GetMigrateMapPlans()
                .OnSuccess(async plans =>
                {
                    var migrationResults = await plans
                        .ToAsyncEnumerable()
                        .SelectAwait(async plan =>
                        {
                            try
                            {
                                var dataBlock = await _contentDbContext
                                    .DataBlocks
                                    .SingleAsync(db => db.Id == plan.DataBlockId);

                                var map = (MapChart) dataBlock.Charts[0];

                                switch (plan.Strategy)
                                {
                                    case RetainChosenBoundaryLevel:
                                    {
                                        map.BoundaryLevel = dataBlock.Query.BoundaryLevel ?? 0;
                                        await SaveChanges(dataBlock, dryRun);
                                        return new MapMigrationResult(
                                            plan.DataBlockId,
                                            plan.Type,
                                            plan.Strategy,
                                            map.BoundaryLevel,
                                            boundaryLevels.Single(level => level.Id == map.BoundaryLevel).Level);
                                    }
                                    case SetBoundaryLevelToMatchSingleDataSetGeoLevel:
                                    {
                                        var geographicLevel = plan
                                            .DataSetGeographicLevels
                                            .Single();

                                        var createdDate = await GetDataBlockCreatedDate(dataBlock);

                                        var boundaryLevel = GetLatestBoundaryLevelAtCreationTime(
                                            createdDate,
                                            geographicLevel,
                                            boundaryLevels);

                                        dataBlock.Query.BoundaryLevel = boundaryLevel.Id;
                                        map.BoundaryLevel = boundaryLevel.Id;
                                        await SaveChanges(dataBlock, dryRun);
                                        return new MapMigrationResult(
                                            plan.DataBlockId,
                                            plan.Type,
                                            plan.Strategy,
                                            map.BoundaryLevel,
                                            boundaryLevels.Single(level => level.Id == map.BoundaryLevel).Level);
                                    }
                                    case SetBoundaryLevelToMatchSingleNonCountryDataSetLevel:
                                    {
                                        var nonCountryGeographicLevel = plan
                                            .DataSetGeographicLevels
                                            .Single(geographicLevel => geographicLevel != GeographicLevel.Country);

                                        var createdDate = await GetDataBlockCreatedDate(dataBlock);

                                        var boundaryLevel = GetLatestBoundaryLevelAtCreationTime(
                                            createdDate,
                                            nonCountryGeographicLevel,
                                            boundaryLevels);

                                        dataBlock.Query.BoundaryLevel = boundaryLevel.Id;
                                        map.BoundaryLevel = boundaryLevel.Id;
                                        await SaveChanges(dataBlock, dryRun);
                                        return new MapMigrationResult(
                                            plan.DataBlockId,
                                            plan.Type,
                                            plan.Strategy,
                                            map.BoundaryLevel,
                                            boundaryLevels.Single(level => level.Id == map.BoundaryLevel).Level);
                                    }
                                    case QuestionWhichCorrectGeoLevelToSelect:
                                    case FixMapConfigNoDataSetGeographicLevels:
                                    case FixMapConfigInvalidDataSetGeographicLevels:
                                        return new MapMigrationResult(
                                            plan.DataBlockId,
                                            plan.Type,
                                            plan.Strategy,
                                            null,
                                            null);
                                    default:
                                        throw new ArgumentOutOfRangeException(
                                            $"Unknown migration strategy {plan.Strategy}");
                                }
                            }
                            catch (Exception e)
                            {
                                return new MapMigrationResult(
                                    plan.DataBlockId,
                                    plan.Type,
                                    UnknownErrorEncountered,
                                    null,
                                    null)
                                {
                                    ExceptionMessage = e.Message
                                };
                            }
                        })
                        .ToListAsync();

                    return migrationResults;
                });
        }

        private async Task SaveChanges(DataBlock dataBlock, bool dryRun)
        {
            if (!dryRun)
            {
                _contentDbContext.DataBlocks.Update(dataBlock);
                await _contentDbContext.SaveChangesAsync();
            }
        }

        private async Task<DateTime?> GetDataBlockCreatedDate(DataBlock dataBlock)
        {
            try
            {
                if (dataBlock.Created != null)
                {
                    return dataBlock.Created.Value;
                }

                var owningReleaseByReleaseContentBlocks = (await _contentDbContext
                        .ReleaseContentBlocks
                        .Include(rcb => rcb.Release)
                        .SingleOrDefaultAsync(rcb => rcb.ContentBlockId == dataBlock.Id))
                    ?.Release;

                if (owningReleaseByReleaseContentBlocks != null)
                {
                    return owningReleaseByReleaseContentBlocks.Created;
                }

                var owningReleaseByContentSection = (await _contentDbContext
                        .ReleaseContentSections
                        .Include(rcs => rcs.Release)
                        .Include(rcs => rcs.ContentSection)
                        .ThenInclude(cs => cs.Content)
                        .Where(rcs => rcs.ContentSection.Content.Contains(dataBlock))
                        .FirstOrDefaultAsync())
                    ?.Release;

                if (owningReleaseByContentSection != null)
                {
                    return owningReleaseByContentSection.Created;
                }

                return null;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to determine CreatedDate for DataBlock {DataBlockId}", dataBlock.Id);
                return null;
            }
        }

        private 
            List<(
                GeographicLevel geographicLevel, 
                DateTime start, 
                DateTime end,
                BoundaryLevel boundaryLevel
            )>
            GetBoundaryLevelsWithDateRanges(List<BoundaryLevel> boundaryLevels)
        {
            var groupedByGeographicLevel = boundaryLevels
                .GroupBy(boundary => boundary.Level)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderBy(boundary => boundary.Created).ToList());

            return groupedByGeographicLevel
                .SelectMany(levelAndBoundaries =>
                {
                    var (level, boundaries) = levelAndBoundaries;
                    return boundaries
                        .Select(boundary =>
                        {
                            var indexOfBoundary = boundaries.IndexOf(boundary);
                            var previousBoundary = indexOfBoundary > 0 ? boundaries[indexOfBoundary - 1] : null;
                            var nextBoundary = indexOfBoundary < boundaries.Count - 1 ? boundaries[indexOfBoundary + 1] : null;
                            return (
                                level,
                                previousBoundary == null ? boundary.Created.AddYears(-10) : boundary.Created,
                                nextBoundary?.Created.AddMilliseconds(-1) ?? boundary.Created.AddYears(10),
                                boundary
                            );
                        });
                })
                .ToList();
        }

        private BoundaryLevel GetLatestBoundaryLevelAtCreationTime(
            DateTime? dataBlockCreatedDate,
            GeographicLevel geographicLevel, 
            List<BoundaryLevel> boundaryLevels)
        {
            var boundaryLevelsWithDateRanges = GetBoundaryLevelsWithDateRanges(boundaryLevels);

            var dataBlockCreatedOrNow = dataBlockCreatedDate ?? DateTime.UtcNow;
            
            return boundaryLevelsWithDateRanges
                .Single(boundary =>
                {
                    var (level, start, end, _) = boundary;

                    return level == geographicLevel &&
                           dataBlockCreatedOrNow.CompareTo(start) >= 0 &&
                           dataBlockCreatedOrNow.CompareTo(end) <= 0;
                })
                .boundaryLevel;
        }

        private async Task<List<DataBlock>> GetAllDataBlocksWithMaps()
        {
            return (await _contentDbContext
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
                FixMapConfigNoDataSetGeographicLevels,
                FixMapConfigInvalidDataSetGeographicLevels,
                UnknownErrorEncountered,
            }
            
            public Guid DataBlockId { get; }

            public MigrationType Type { get; } = Undefined;

            public MigrationStrategy Strategy { get; } = FixMapConfigNoDataSetGeographicLevels;

            [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
            public List<GeographicLevel> DataBlockGeographicLevels { get; }

            [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
            public List<GeographicLevel> DataSetGeographicLevels { get; }

            public DataBlockMapMigrationPlan(
                Guid dataBlockId, 
                BoundaryLevel? specificBoundaryLevel,
                List<GeographicLevel> dataBlockGeographicLevels, 
                List<ChartAxisConfiguration>? chartAxisConfigurations)
            {
                DataBlockId = dataBlockId;
                DataBlockGeographicLevels = dataBlockGeographicLevels;
                
                var dataSets = chartAxisConfigurations?
                    .SelectMany(axisConfig => axisConfig.DataSets)
                    .ToList() ?? new List<ChartDataSet>();

                try
                {
                    DataSetGeographicLevels = dataSets
                        .SelectMany(dataSet =>
                        {
                            var level = dataSet.Location?.Level;

                            // If no level is specified on this Data Set, it is intended for all
                            // Geographic Levels, so return all Geo Levels for this Data Block in
                            // this case.
                            if (level == null)
                            {
                                return DataBlockGeographicLevels;
                            }

                            if (!Enum.TryParse<GeographicLevel>(level, ignoreCase: true, out var matchedLevel))
                            {
                                throw new ArgumentException(
                                    $"Invalid Level in Data Set for block {dataBlockId}: {level}");
                            }

                            return ListOf(matchedLevel);
                        })
                        .Distinct()
                        .ToList();
                }
                catch (ArgumentException)
                {
                    Strategy = FixMapConfigInvalidDataSetGeographicLevels;
                    return;
                }
                
                if (DataSetGeographicLevels.Count == 0)
                {
                    Type = Undefined;
                    Strategy = FixMapConfigNoDataSetGeographicLevels;
                    return;
                }
                
                if (specificBoundaryLevel != null)
                {
                    if (DataSetGeographicLevels.Count == 1)
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
                    if (DataSetGeographicLevels.Count == 1)
                    {
                        Type = NoSpecificBoundarySetAndSingleGeoLevelInDataSets;
                        Strategy = SetBoundaryLevelToMatchSingleDataSetGeoLevel;
                    }
                    else if (DataSetGeographicLevels.Count == 2 && DataSetGeographicLevels.Contains(GeographicLevel.Country))
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
            }
        }
            
        public record MapMigrationResult
        {
            public Guid DataBlockId { get; }
            public DataBlockMapMigrationPlan.MigrationType Type { get; } = Undefined;
            public DataBlockMapMigrationPlan.MigrationStrategy Strategy { get; } = FixMapConfigNoDataSetGeographicLevels;
            public long? BoundaryLevelId { get; }

            [JsonConverter(typeof(StringEnumConverter))]
            public GeographicLevel? GeographicLevel { get; }
            
            public string ExceptionMessage { get; init; }

            public MapMigrationResult(
                Guid dataBlockId, 
                DataBlockMapMigrationPlan.MigrationType type, 
                DataBlockMapMigrationPlan.MigrationStrategy strategy, 
                long? boundaryLevelId, 
                GeographicLevel? geographicLevel)
            {
                DataBlockId = dataBlockId;
                Type = type;
                Strategy = strategy;
                BoundaryLevelId = boundaryLevelId;
                GeographicLevel = geographicLevel;
            }
        }
    }
}
