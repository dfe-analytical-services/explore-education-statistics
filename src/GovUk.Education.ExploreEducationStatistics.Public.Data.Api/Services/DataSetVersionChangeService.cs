using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class DataSetVersionChangeService(
    PublicDataDbContext publicDataDbContext,
    IUserService userService)
    : IDataSetVersionChangeService
{
    public Task<Either<ActionResult, DataSetVersionChangesViewModel>> GetChanges(
        Guid dataSetId,
        string dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        return publicDataDbContext.DataSetVersions
            .FindByVersion(
                dataSetId: dataSetId,
                version: dataSetVersion,
                appendQuery: query => query
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(dsv => dsv.FilterMetaChanges)
                    .Include(dsv => dsv.FilterOptionMetaChanges)
                    .Include(dsv => dsv.IndicatorMetaChanges)
                    .Include(dsv => dsv.LocationMetaChanges)
                    .Include(dsv => dsv.LocationOptionMetaChanges)
                    .Include(dsv => dsv.TimePeriodMetaChanges),
                cancellationToken: cancellationToken)
            .OnSuccessDo(userService.CheckCanViewDataSetVersion)
            .OnSuccess(MapChanges);
    }

    private async Task<DataSetVersionChangesViewModel> MapChanges(DataSetVersion dataSetVersion)
    {
        // Load geographic level meta change separately as EF won't split query this properly.
        // One-to-one relationships get joined in automatically, but this results in
        // an additional join to the geographic level meta change for EACH split query
        // on the other change collection relationships which is very inefficient.
        await publicDataDbContext.Entry(dataSetVersion)
            .Reference(dsv => dsv.GeographicLevelMetaChange)
            .LoadAsync();

        var filterChanges =
            GetFilterChanges(dataSetVersion.FilterMetaChanges);
        var filterOptionChanges =
            GetFilterOptionChanges(dataSetVersion.FilterOptionMetaChanges);
        var geographicLevelOptionChange =
            GetGeographicLevelOptionChange(dataSetVersion.GeographicLevelMetaChange);
        var indicatorChanges =
            GetIndicatorChanges(dataSetVersion.IndicatorMetaChanges);
        var locationGroupChanges =
            GetLocationGroupChanges(dataSetVersion.LocationMetaChanges);
        var locationOptionChanges =
            GetLocationOptionChanges(dataSetVersion.LocationOptionMetaChanges);
        var timePeriodChanges =
            GetTimePeriodChanges(dataSetVersion.TimePeriodMetaChanges);

        return new DataSetVersionChangesViewModel
        {
            MajorChanges = new ChangeSetViewModel
            {
                Filters = filterChanges?.GetValueOrDefault(ChangeType.Major),
                FilterOptions = filterOptionChanges?.GetValueOrDefault(ChangeType.Major),
                GeographicLevels = geographicLevelOptionChange?.GetValueOrDefault(ChangeType.Major),
                Indicators = indicatorChanges?.GetValueOrDefault(ChangeType.Major),
                LocationGroups = locationGroupChanges?.GetValueOrDefault(ChangeType.Major),
                LocationOptions = locationOptionChanges?.GetValueOrDefault(ChangeType.Major),
                TimePeriods = timePeriodChanges?.GetValueOrDefault(ChangeType.Minor)
            },
            MinorChanges = new ChangeSetViewModel
            {
                Filters = filterChanges?.GetValueOrDefault(ChangeType.Minor),
                FilterOptions = filterOptionChanges?.GetValueOrDefault(ChangeType.Minor),
                GeographicLevels = geographicLevelOptionChange?.GetValueOrDefault(ChangeType.Minor),
                Indicators = indicatorChanges?.GetValueOrDefault(ChangeType.Minor),
                LocationGroups = locationGroupChanges?.GetValueOrDefault(ChangeType.Minor),
                LocationOptions = locationOptionChanges?.GetValueOrDefault(ChangeType.Minor),
                TimePeriods = timePeriodChanges?.GetValueOrDefault(ChangeType.Minor)
            }
        };
    }

    private static Dictionary<ChangeType, List<IndicatorChangeViewModel>>? GetIndicatorChanges(
        List<IndicatorMetaChange> changes)
    {
        return changes.Count != 0
            ? changes
                .Select(c => new IndicatorChangeViewModel
                {
                    CurrentState = c.CurrentState is not null
                        ? IndicatorViewModel.Create(c.CurrentState)
                        : null,
                    PreviousState = c.PreviousState is not null
                        ? IndicatorViewModel.Create(c.PreviousState)
                        : null,
                })
                .GroupBy(c => c.IsMajor() ? ChangeType.Major : ChangeType.Minor)
                .ToDictionary(g => g.Key, g => g.ToList())
            : null;
    }

    private static Dictionary<ChangeType, List<FilterChangeViewModel>>? GetFilterChanges(List<FilterMetaChange> changes)
    {
        return changes.Count != 0
            ? changes
                .Select(c => new FilterChangeViewModel
                {
                    CurrentState = c.CurrentState is not null
                        ? FilterViewModel.Create(c.CurrentState)
                        : null,
                    PreviousState = c.PreviousState is not null
                        ? FilterViewModel.Create(c.PreviousState)
                        : null,
                })
                .GroupBy(c => c.IsMajor() ? ChangeType.Major : ChangeType.Minor)
                .ToDictionary(g => g.Key, g => g.ToList())
            : null;
    }

    private static Dictionary<ChangeType, List<FilterOptionChangeViewModel>>? GetFilterOptionChanges(
        List<FilterOptionMetaChange> changes)
    {
        return changes.Count != 0
            ? changes
                .Select(c => new FilterOptionChangeViewModel
                {
                    CurrentState = c.CurrentState is not null
                        ? FilterOptionViewModel.Create(c.CurrentState)
                        : null,
                    PreviousState = c.PreviousState is not null
                        ? FilterOptionViewModel.Create(c.PreviousState)
                        : null,
                })
                .GroupBy(c => c.IsMajor() ? ChangeType.Major : ChangeType.Minor)
                .ToDictionary(g => g.Key, g => g.ToList())
            : null;
    }

    private static Dictionary<ChangeType, List<GeographicLevelOptionChangeViewModel>>? GetGeographicLevelOptionChange(
        GeographicLevelMetaChange? change)
    {
        if (change is null)
        {
            return null;
        }

        var currentLevels = (change.CurrentState?.Levels ?? []).ToHashSet();
        var previousLevels = (change.PreviousState?.Levels ?? []).ToHashSet();

        if (currentLevels.SetEquals(previousLevels))
        {
            return null;
        }

        List<GeographicLevelOptionChangeViewModel> changes =
        [
            ..currentLevels
                .Where(level => !previousLevels.Contains(level))
                .Select(level => new GeographicLevelOptionChangeViewModel
                {
                    CurrentState = GeographicLevelOptionViewModel.Create(level),
                }),
            ..previousLevels
                .Where(level => !currentLevels.Contains(level))
                .Select(level => new GeographicLevelOptionChangeViewModel
                {
                    PreviousState = GeographicLevelOptionViewModel.Create(level),
                }),
        ];

        return changes
            .GroupBy(c => c.IsMajor() ? ChangeType.Major : ChangeType.Minor)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private static Dictionary<ChangeType, List<LocationGroupChangeViewModel>>? GetLocationGroupChanges(
        List<LocationMetaChange> changes)
    {
        return changes.Count != 0
            ? changes
                .Select(c => new LocationGroupChangeViewModel
                {
                    CurrentState = c.CurrentState is not null
                        ? LocationGroupViewModel.Create(c.CurrentState)
                        : null,
                    PreviousState = c.PreviousState is not null
                        ? LocationGroupViewModel.Create(c.PreviousState)
                        : null,
                })
                .GroupBy(c => c.IsMajor() ? ChangeType.Major : ChangeType.Minor)
                .ToDictionary(g => g.Key, g => g.ToList())
            : null;
    }

    private static Dictionary<ChangeType, List<LocationOptionChangeViewModel>>? GetLocationOptionChanges(
        List<LocationOptionMetaChange> changes)
    {
        return changes.Count != 0
            ? changes
                .Select(c => new LocationOptionChangeViewModel
                {
                    CurrentState = c.CurrentState is not null
                        ? LocationOptionViewModel.Create(c.CurrentState.Option)
                        : null,
                    PreviousState = c.PreviousState is not null
                        ? LocationOptionViewModel.Create(c.PreviousState.Option)
                        : null,
                })
                .GroupBy(c => c.IsMajor() ? ChangeType.Major : ChangeType.Minor)
                .ToDictionary(g => g.Key, g => g.ToList())
            : null;
    }

    private static Dictionary<ChangeType, List<TimePeriodOptionChangeViewModel>>? GetTimePeriodChanges(
        List<TimePeriodMetaChange> changes)
    {
        return changes.Count != 0
            ? changes
                .Select(c => new TimePeriodOptionChangeViewModel
                {
                    CurrentState = c.CurrentState is not null
                        ? TimePeriodViewModel.Create(c.CurrentState)
                        : null,
                    PreviousState = c.PreviousState is not null
                        ? TimePeriodViewModel.Create(c.PreviousState)
                        : null,
                })
                .GroupBy(c => c.IsMajor() ? ChangeType.Major : ChangeType.Minor)
                .ToDictionary(g => g.Key, g => g.ToList())
            : null;
    }

    private enum ChangeType
    {
        Major,
        Minor
    }
}
