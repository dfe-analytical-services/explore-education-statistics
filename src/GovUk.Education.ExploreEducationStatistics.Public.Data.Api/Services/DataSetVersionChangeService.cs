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
            .AsNoTracking()
            .FindByVersion(
                dataSetId: dataSetId,
                version: dataSetVersion,
                cancellationToken: cancellationToken)
            .OnSuccessDo(userService.CheckCanViewDataSetVersion)
            .OnSuccess(dsv => LoadChanges(dsv, cancellationToken))
            .OnSuccess(MapChanges);
    }

    private async Task<DataSetVersion> LoadChanges(DataSetVersion dataSetVersion, CancellationToken cancellationToken)
    {
        // Using split query rather than multiple explicit loads for readability.
        var loadedDataSetVersion = await publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .AsSplitQuery()
            .Include(dsv => dsv.FilterMetaChanges)
            .Include(dsv => dsv.FilterOptionMetaChanges)
            .Include(dsv => dsv.IndicatorMetaChanges)
            .Include(dsv => dsv.LocationMetaChanges)
            .Include(dsv => dsv.LocationOptionMetaChanges)
            .Include(dsv => dsv.TimePeriodMetaChanges)
            .Where(dsv => dsv.Id == dataSetVersion.Id)
            .SingleAsync(cancellationToken);

        // Load geographic level meta change separately as EF won't split query this properly.
        // One-to-one relationships get joined in automatically, but this results in
        // an additional join to the geographic level meta change for EACH split query
        // on the other change collection relationships which is very inefficient.
        await publicDataDbContext.Entry(loadedDataSetVersion)
            .Reference(dsv => dsv.GeographicLevelMetaChange)
            .LoadAsync(cancellationToken);

        return loadedDataSetVersion;
    }

    private static DataSetVersionChangesViewModel MapChanges(DataSetVersion dataSetVersion)
    {
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
                TimePeriods = timePeriodChanges?.GetValueOrDefault(ChangeType.Major)
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

    private static Dictionary<ChangeType, List<FilterOptionChangesViewModel>>? GetFilterOptionChanges(
        List<FilterOptionMetaChange> changes)
    {
        return changes.Count != 0
            ? changes
                .Select(c => (
                    Meta: (c.CurrentState?.Meta ?? c.PreviousState?.Meta)!,
                    Option: new FilterOptionChangeViewModel
                    {
                        CurrentState = c.CurrentState is not null
                            ? FilterOptionViewModel.Create(c.CurrentState)
                            : null,
                        PreviousState = c.PreviousState is not null
                            ? FilterOptionViewModel.Create(c.PreviousState)
                            : null,
                    }
                ))
                .GroupBy(tuple => tuple.Option.IsMajor() ? ChangeType.Major : ChangeType.Minor)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping
                        .GroupBy(t => (t.Meta.PublicId, t.Meta.Label), t => t.Option)
                        .Select(metaGroup => new FilterOptionChangesViewModel
                        {
                            Filter = new FilterViewModel
                            {
                                Id = metaGroup.Key.PublicId,
                                Label = metaGroup.Key.Label
                            },
                            Options = metaGroup.ToList()
                        })
                        .ToList()
                )
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
                    CurrentState = GeographicLevelViewModel.Create(level),
                }),
            ..previousLevels
                .Where(level => !currentLevels.Contains(level))
                .Select(level => new GeographicLevelOptionChangeViewModel
                {
                    PreviousState = GeographicLevelViewModel.Create(level),
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

    private static Dictionary<ChangeType, List<LocationOptionChangesViewModel>>? GetLocationOptionChanges(
        List<LocationOptionMetaChange> changes)
    {
        return changes.Count != 0
            ? changes
                .Select(c => (
                    Meta: (c.CurrentState?.Meta ?? c.PreviousState?.Meta)!,
                    Option: new LocationOptionChangeViewModel
                    {
                        CurrentState = c.CurrentState is not null
                            ? LocationOptionViewModel.Create(c.CurrentState.Option, c.CurrentState.PublicId)
                            : null,
                        PreviousState = c.PreviousState is not null
                            ? LocationOptionViewModel.Create(c.PreviousState.Option, c.PreviousState.PublicId)
                            : null,
                    })
                )
                .GroupBy(c => c.Option.IsMajor() ? ChangeType.Major : ChangeType.Minor)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .GroupBy(t => t.Meta.Level, t => t.Option)
                        .Select(metaGroup => new LocationOptionChangesViewModel
                        {
                            Level = GeographicLevelViewModel.Create(metaGroup.Key),
                            Options = metaGroup.ToList()
                        })
                        .ToList()
                )
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
                        ? TimePeriodOptionViewModel.Create(c.CurrentState)
                        : null,
                    PreviousState = c.PreviousState is not null
                        ? TimePeriodOptionViewModel.Create(c.PreviousState)
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
