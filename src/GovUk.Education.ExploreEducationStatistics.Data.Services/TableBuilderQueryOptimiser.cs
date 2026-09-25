#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Options;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.ValidationErrorMessages;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class TableBuilderQueryOptimiser(
    IStorageDataSetResolver storageDataSetResolver,
    IOptions<TableBuilderOptions> options
) : ITableBuilderQueryOptimiser
{
    private const int MaxSelections = 5;

    private async Task<Either<ActionResult, int>> GetMaximumTableCellCount(FullTableQuery query)
    {
        var filterItemIds = query.GetFilterItemIds();

        List<int> countsOfFilterItemsByFilter;

        if (filterItemIds.Count == 0)
        {
            countsOfFilterItemsByFilter = [];
        }
        else
        {
            var dataSet = await storageDataSetResolver.Resolve(query.SubjectId);
            var filterItems = await dataSet.ListFilterItems(filterItemIds);

            // The ids are distinct, so fewer filter items than ids means some ids don't exist in the data set
            if (filterItems.Count != filterItemIds.Count)
            {
                return ValidationUtils.ValidationResult(FilterItemsNotFound);
            }

            countsOfFilterItemsByFilter = filterItems
                .GroupBy(filterItem => filterItem.FilterGroup.FilterId)
                .Select(grouping => grouping.Count())
                .ToList();
        }

        return TableBuilderUtils.MaximumTableCellCount(
            countOfIndicators: query.Indicators.Count(),
            countOfLocations: query.LocationIds.Count,
            countOfTimePeriods: TimePeriodUtil.Range(query.TimePeriod).Count,
            countsOfFilterItemsByFilter: countsOfFilterItemsByFilter
        );
    }

    public async Task<Either<ActionResult, bool>> IsCroppingRequired(FullTableQuery query)
    {
        return await GetMaximumTableCellCount(query)
            .OnSuccess(maxCells => maxCells > options.Value.MaxTableCellsAllowed);
    }

    public async Task<Either<ActionResult, FullTableQuery>> CropQuery(
        FullTableQuery query,
        CancellationToken cancellationToken
    )
    {
        if (query.TimePeriod is not null)
        {
            var timePeriods = TimePeriodUtil.Range(query.TimePeriod);

            if (timePeriods.Count > MaxSelections)
            {
                var croppedTimePeriods = timePeriods.TakeLast(MaxSelections);
                query.TimePeriod.StartYear = croppedTimePeriods.First().Year;
                query.TimePeriod.StartCode = croppedTimePeriods.First().TimeIdentifier;
                query.TimePeriod.EndYear = croppedTimePeriods.Last().Year;
                query.TimePeriod.EndCode = croppedTimePeriods.Last().TimeIdentifier;

                return await IsCroppingRequired(query)
                    .OnSuccess(async croppingRequired =>
                        croppingRequired ? await CropLocations(query, cancellationToken) : query
                    );
            }
        }

        return await CropLocations(query, cancellationToken);
    }

    private async Task<FullTableQuery> CropLocations(FullTableQuery query, CancellationToken cancellationToken)
    {
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

        var dataSet = await storageDataSetResolver.Resolve(query.SubjectId, cancellationToken);
        var locations = await dataSet.ListLocations(query.LocationIds, cancellationToken);

        var croppedLocationIds = locations
            .GroupBy(l => l.GeographicLevel)
            .SelectMany(g => g.OrderBy(l => l.Id).Take(maxLocationsPerGeographicLevel[g.Key]).Select(l => l.Id))
            .Take(MaxSelections)
            .ToList();

        query.LocationIds = croppedLocationIds;

        return query;
    }
}
