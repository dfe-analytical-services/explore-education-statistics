#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.DataBlockMigrationService.DataBlockMapMigrationPlan.MigrationStrategy;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.DataBlockMigrationService.DataBlockMapMigrationPlan.MigrationType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataBlockMigrationService : IDataBlockMigrationService
    {
        private readonly ContentDbContext _context;
        private readonly StatisticsDbContext _statisticsDbContext;

        public DataBlockMigrationService(
            ContentDbContext context,
            StatisticsDbContext statisticsDbContext)
        {
            _context = context;
            _statisticsDbContext = statisticsDbContext;
            _statisticsDbContext = statisticsDbContext;
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
                            
                            var dataBlockGeographicLevels = await locations
                                .Where(l => db.Query.LocationIds.Contains(l.Id))
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

        public async Task<Either<ActionResult, List<MapMigrationResult>>> MigrateMaps()
        {
            var boundaryLevels = await GetAllBoundaryLevels();

            return await GetMigrateMapPlans()
                .OnSuccess(async plans =>
                {
                    var migrationResults = await plans
                        .ToAsyncEnumerable()
                        .SelectAwait(async plan =>
                        {
                            var dataBlock = await _context
                                .DataBlocks
                                .SingleAsync(db => db.Id == plan.DataBlockId);

                            var map = (MapChart)dataBlock.Charts[0];

                            switch (plan.Strategy)
                            {
                                case RetainChosenBoundaryLevel:
                                {
                                    map.BoundaryLevel = dataBlock.Query.BoundaryLevel ?? 0;
                                    return new MapMigrationResult(plan.DataBlockId, plan.Strategy,
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
                                    return new MapMigrationResult(plan.DataBlockId, plan.Strategy,
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
                                    return new MapMigrationResult(plan.DataBlockId, plan.Strategy,
                                        map.BoundaryLevel,
                                        boundaryLevels.Single(level => level.Id == map.BoundaryLevel).Level);
                                }
                                case QuestionWhichCorrectGeoLevelToSelect:
                                    return new MapMigrationResult(plan.DataBlockId, plan.Strategy,
                                        null,
                                        null);
                                case FixMapConfigNoDataSetGeographicLevels:
                                    return new MapMigrationResult(plan.DataBlockId, plan.Strategy,
                                        null,
                                        null);
                                default:
                                    throw new ArgumentOutOfRangeException(
                                        $"Unknown migration strategy {plan.Strategy}");
                            }
                        })
                        .ToListAsync();

                    return migrationResults;
                });
        }

        private async Task<DateTime> GetDataBlockCreatedDate(DataBlock dataBlock)
        {
            DateTime createdDate;

            if (dataBlock.Created != null)
            {
                createdDate = dataBlock.Created.Value;
            }
            else
            {
                var releaseIds = await _statisticsDbContext
                    .ReleaseSubject
                    .Where(rs => rs.SubjectId == dataBlock.Query.SubjectId)
                    .Select(rs => rs.ReleaseId)
                    .ToListAsync();

                var firstRelease = await _context
                    .Releases
                    .Where(release => releaseIds.Contains(release.Id))
                    .OrderBy(release => release.Created)
                    .FirstAsync();

                createdDate = firstRelease.Created;
            }

            return createdDate;
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
            DateTime dataBlockCreated,
            GeographicLevel geographicLevel, 
            List<BoundaryLevel> boundaryLevels)
        {
            var boundaryLevelsWithDateRanges = GetBoundaryLevelsWithDateRanges(boundaryLevels);
            return boundaryLevelsWithDateRanges
                .Single(boundary =>
                {
                    var (level, start, end, _) = boundary;

                    return level == geographicLevel &&
                           dataBlockCreated.CompareTo(start) >= 0 &&
                           dataBlockCreated.CompareTo(end) <= 0;
                })
                .boundaryLevel;
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
            
        public record MapMigrationResult
        {
            public Guid DataBlockId { get; }
            public DataBlockMapMigrationPlan.MigrationStrategy Strategy { get; } = FixMapConfigNoDataSetGeographicLevels;
            public long? BoundaryLevelId { get; }

            [JsonConverter(typeof(StringEnumConverter))]
            public GeographicLevel? GeographicLevel { get; }

            public MapMigrationResult(Guid dataBlockId, DataBlockMapMigrationPlan.MigrationStrategy strategy, long? boundaryLevelId, GeographicLevel? geographicLevel)
            {
                DataBlockId = dataBlockId;
                Strategy = strategy;
                BoundaryLevelId = boundaryLevelId;
                GeographicLevel = geographicLevel;
            }
        }
    }
}
