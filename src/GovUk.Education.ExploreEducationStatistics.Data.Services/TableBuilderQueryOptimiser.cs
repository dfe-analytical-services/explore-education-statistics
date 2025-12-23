#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Options;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class TableBuilderQueryOptimiser(
    StatisticsDbContext statisticsDbContext,
    IFilterItemRepository filterItemRepository,
    IOptions<TableBuilderOptions> options
) : ITableBuilderQueryOptimiser
{
    private async Task<int> GetMaximumTableCellCount(FullTableQuery query)
    {
        var filterItemIds = query.GetFilterItemIds();

        var countsOfFilterItemsByFilter =
            filterItemIds.Count == 0
                ? []
                : (await filterItemRepository.CountFilterItemsByFilter(filterItemIds))
                    .Select(pair =>
                    {
                        var (_, count) = pair;
                        return count;
                    })
                    .ToList();

        return TableBuilderUtils.MaximumTableCellCount(
            countOfIndicators: query.Indicators.Count(),
            countOfLocations: query.LocationIds.Count,
            countOfTimePeriods: TimePeriodUtil.Range(query.TimePeriod).Count,
            countsOfFilterItemsByFilter: countsOfFilterItemsByFilter
        );
    }

    public async Task<bool> IsCroppingRequired(FullTableQuery query)
    {
        var maxCells = await GetMaximumTableCellCount(query);
        return maxCells > options.Value.MaxTableCellsAllowed;
    }

    public async Task<FullTableQuery> CropQuery(FullTableQuery query, CancellationToken cancellationToken)
    {
        var maxSelections = 5;

        if (query.TimePeriod is not null)
        {
            var timePeriods = TimePeriodUtil.Range(query.TimePeriod);

            if (timePeriods.Count > maxSelections)
            {
                var croppedTimePeriods = timePeriods.TakeLast(maxSelections);
                query.TimePeriod.StartYear = croppedTimePeriods.First().Year;
                query.TimePeriod.StartCode = croppedTimePeriods.First().TimeIdentifier;
                query.TimePeriod.EndYear = croppedTimePeriods.Last().Year;
                query.TimePeriod.EndCode = croppedTimePeriods.Last().TimeIdentifier;

                if (!await IsCroppingRequired(query))
                {
                    return query;
                }
            }
        }

        var maxLocationsPerGeographicLevel = new Dictionary<GeographicLevel, int>
        {
            { GeographicLevel.Country, 1 },
            { GeographicLevel.Region, 2 },
            { GeographicLevel.LocalAuthority, 2 },
            { GeographicLevel.EnglishDevolvedArea, 2 },
            { GeographicLevel.Institution, 2 },
            { GeographicLevel.MayoralCombinedAuthority, 2 },
            { GeographicLevel.LocalEnterprisePartnership, 2 },
            { GeographicLevel.LocalSkillsImprovementPlanArea, 2 },
            { GeographicLevel.LocalAuthorityDistrict, 2 },
            { GeographicLevel.MultiAcademyTrust, 2 },
            { GeographicLevel.OpportunityArea, 2 },
            { GeographicLevel.ParliamentaryConstituency, 2 },
            { GeographicLevel.PlanningArea, 2 },
            { GeographicLevel.PoliceForceArea, 2 },
            { GeographicLevel.Provider, 2 },
            { GeographicLevel.RscRegion, 2 },
            { GeographicLevel.School, 2 },
            { GeographicLevel.Sponsor, 2 },
            { GeographicLevel.Ward, 2 },
        };

        var locations = await statisticsDbContext
            .Location.Where(l => query.LocationIds.Contains(l.Id))
            .ToListAsync(cancellationToken);

        var croppedLocationIds = locations
            .GroupBy(l => l.GeographicLevel)
            .SelectMany(g => g.OrderBy(l => l.Id).Take(maxLocationsPerGeographicLevel[g.Key]).Select(l => l.Id))
            .Take(maxSelections)
            .ToList();

        query.LocationIds = croppedLocationIds;

        return query;
    }
}
