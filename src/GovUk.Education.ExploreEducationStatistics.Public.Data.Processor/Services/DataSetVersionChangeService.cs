using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

internal class DataSetVersionChangeService(PublicDataDbContext publicDataDbContext) : IDataSetVersionChangeService
{
    public async Task CreateChanges(Guid nextDataSetVersionId, CancellationToken cancellationToken = default)
    {
        var nextVersion = await publicDataDbContext
            .DataSetVersions.Include(dsv => dsv.DataSet)
            .SingleAsync(dsv => dsv.Id == nextDataSetVersionId, cancellationToken);

        var previousVersionId = nextVersion.DataSet.LatestLiveVersionId!.Value;

        await publicDataDbContext.RequireTransaction(async () =>
        {
            await CreateFilterChanges(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken
            );

            await CreateLocationChanges(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken
            );

            await CreateGeographicLevelChange(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken
            );

            await CreateIndicatorChanges(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken
            );

            await CreateTimePeriodChanges(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken
            );
        });
    }

    private async Task CreateFilterChanges(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken
    )
    {
        var oldMetas = await GetFilterMetas(previousVersionId, cancellationToken);
        var newMetas = await GetFilterMetas(nextVersionId, cancellationToken);

        var metaDeletions = new List<FilterMetaChange>();
        var optionMetaDeletions = new List<FilterOptionMetaChange>();
        var metaUpdates = new List<FilterMetaChange>();
        var optionMetaUpdates = new List<FilterOptionMetaChange>();
        var metaAdditions = new List<FilterMetaChange>();
        var optionMetaAdditions = new List<FilterOptionMetaChange>();

        foreach (var (filterPublicId, oldFilterTuple) in oldMetas)
        {
            if (newMetas.TryGetValue(filterPublicId, out var newFilterTuple))
            {
                // Filter updated
                if (!IsFilterEqual(newFilterTuple.FilterMeta, oldFilterTuple.FilterMeta))
                {
                    metaUpdates.Add(
                        new FilterMetaChange
                        {
                            DataSetVersionId = nextVersionId,
                            PreviousState = oldFilterTuple.FilterMeta,
                            PreviousStateId = oldFilterTuple.FilterMeta.Id,
                            CurrentState = newFilterTuple.FilterMeta,
                            CurrentStateId = newFilterTuple.FilterMeta.Id,
                        }
                    );
                }

                foreach (var (optionPublicId, oldOptionLink) in oldFilterTuple.OptionLinks)
                {
                    if (!newFilterTuple.OptionLinks.TryGetValue(optionPublicId, out var newOptionLink))
                    {
                        // Filter option deleted
                        optionMetaDeletions.Add(
                            new FilterOptionMetaChange
                            {
                                DataSetVersionId = nextVersionId,
                                PreviousState = FilterOptionMetaChange.State.Create(oldOptionLink),
                            }
                        );
                    }
                    // Filter option updated
                    else if (!IsFilterOptionEqual(newOptionLink.Option, oldOptionLink.Option))
                    {
                        optionMetaUpdates.Add(
                            new FilterOptionMetaChange
                            {
                                DataSetVersionId = nextVersionId,
                                PreviousState = FilterOptionMetaChange.State.Create(oldOptionLink),
                                CurrentState = FilterOptionMetaChange.State.Create(newOptionLink),
                            }
                        );

                        // Improving performance by removing from the new meta to avoid having to loop
                        // over it again when finding the additions
                        newFilterTuple.OptionLinks.Remove(optionPublicId);
                    }
                }
            }
            else
            {
                // Filter deleted
                metaDeletions.Add(
                    new FilterMetaChange
                    {
                        DataSetVersionId = nextVersionId,
                        PreviousState = oldFilterTuple.FilterMeta,
                        PreviousStateId = oldFilterTuple.FilterMeta.Id,
                    }
                );
            }
        }

        foreach (var (filterPublicId, newFilterTuple) in newMetas)
        {
            if (oldMetas.TryGetValue(filterPublicId, out var oldFilterTuple))
            {
                foreach (var (optionPublicId, newOptionLink) in newFilterTuple.OptionLinks)
                {
                    if (!oldFilterTuple.OptionLinks.ContainsKey(optionPublicId))
                    {
                        // Filter option added
                        optionMetaAdditions.Add(
                            new FilterOptionMetaChange
                            {
                                DataSetVersionId = nextVersionId,
                                CurrentState = FilterOptionMetaChange.State.Create(newOptionLink),
                            }
                        );
                    }
                }
            }
            else
            {
                // Filter added
                metaAdditions.Add(
                    new FilterMetaChange
                    {
                        DataSetVersionId = nextVersionId,
                        CurrentState = newFilterTuple.FilterMeta,
                        CurrentStateId = newFilterTuple.FilterMeta.Id,
                    }
                );
            }
        }

        // The meta changes are inserted into the DB in the following order:
        // - Deletions
        // - Updates
        // - Additions
        publicDataDbContext.FilterMetaChanges.AddRange(
            [
                .. metaDeletions.NaturalOrderBy(c => c.PreviousState!.Label),
                .. metaUpdates.NaturalOrderBy(c => c.PreviousState!.Label),
                .. metaAdditions.NaturalOrderBy(c => c.CurrentState!.Label),
            ]
        );

        publicDataDbContext.FilterOptionMetaChanges.AddRange(
            [
                .. optionMetaDeletions.NaturalOrderBy(c => c.PreviousState!.Option.Label),
                .. optionMetaUpdates.NaturalOrderBy(c => c.PreviousState!.Option.Label),
                .. optionMetaAdditions.NaturalOrderBy(c => c.CurrentState!.Option.Label),
            ]
        );

        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsFilterEqual(FilterMeta filterMeta1, FilterMeta filterMeta2)
    {
        return filterMeta1.Column == filterMeta2.Column
            && filterMeta1.Label == filterMeta2.Label
            && filterMeta1.Hint == filterMeta2.Hint;
    }

    private static bool IsFilterOptionEqual(FilterOptionMeta filterOptionMeta1, FilterOptionMeta filterOptionMeta2)
    {
        return filterOptionMeta1.Label == filterOptionMeta2.Label;
    }

    private async Task CreateLocationChanges(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken
    )
    {
        var oldMetas = await GetLocationMetas(previousVersionId, cancellationToken);
        var newMetas = await GetLocationMetas(nextVersionId, cancellationToken);

        var metaDeletions = new List<LocationMetaChange>();
        var optionMetaDeletions = new List<LocationOptionMetaChange>();
        var optionMetaUpdates = new List<LocationOptionMetaChange>();
        var metaAdditions = new List<LocationMetaChange>();
        var optionMetaAdditions = new List<LocationOptionMetaChange>();

        foreach (var (locationLevel, oldLocationTuple) in oldMetas)
        {
            if (newMetas.TryGetValue(locationLevel, out var newLocationTuple))
            {
                foreach (var (optionPublicId, oldOptionLink) in oldLocationTuple.OptionLinks)
                {
                    if (!newLocationTuple.OptionLinks.TryGetValue(optionPublicId, out var newOptionLink))
                    {
                        // Location option deleted
                        optionMetaDeletions.Add(
                            new LocationOptionMetaChange
                            {
                                DataSetVersionId = nextVersionId,
                                PreviousState = LocationOptionMetaChange.State.Create(oldOptionLink),
                            }
                        );
                    }
                    // Location option updated
                    else if (!IsLocationOptionEqual(newOptionLink.Option, oldOptionLink.Option))
                    {
                        optionMetaUpdates.Add(
                            new LocationOptionMetaChange
                            {
                                DataSetVersionId = nextVersionId,
                                PreviousState = LocationOptionMetaChange.State.Create(oldOptionLink),
                                CurrentState = LocationOptionMetaChange.State.Create(newOptionLink),
                            }
                        );

                        // Improving performance by removing from the new meta to avoid having to loop
                        // over it again when finding the additions
                        newLocationTuple.OptionLinks.Remove(optionPublicId);
                    }
                }
            }
            else
            {
                // Location deleted
                metaDeletions.Add(
                    new LocationMetaChange
                    {
                        DataSetVersionId = nextVersionId,
                        PreviousState = oldLocationTuple.LocationMeta,
                        PreviousStateId = oldLocationTuple.LocationMeta.Id,
                    }
                );
            }
        }

        foreach (var (locationLevel, newLocationTuple) in newMetas)
        {
            if (oldMetas.TryGetValue(locationLevel, out var oldLocationTuple))
            {
                foreach (var (optionPublicId, newOptionLink) in newLocationTuple.OptionLinks)
                {
                    if (!oldLocationTuple.OptionLinks.ContainsKey(optionPublicId))
                    {
                        // Location option added
                        optionMetaAdditions.Add(
                            new LocationOptionMetaChange
                            {
                                DataSetVersionId = nextVersionId,
                                CurrentState = LocationOptionMetaChange.State.Create(newOptionLink),
                            }
                        );
                    }
                }
            }
            else
            {
                // Location added
                metaAdditions.Add(
                    new LocationMetaChange
                    {
                        DataSetVersionId = nextVersionId,
                        CurrentState = newLocationTuple.LocationMeta,
                        CurrentStateId = newLocationTuple.LocationMeta.Id,
                    }
                );
            }
        }

        // The meta changes are inserted into the DB in the following order:
        // - Deletions
        // - Updates
        // - Additions
        publicDataDbContext.LocationMetaChanges.AddRange(
            [
                .. metaDeletions.NaturalOrderBy(c => c.PreviousState!.Level.GetEnumLabel()),
                .. metaAdditions.NaturalOrderBy(c => c.CurrentState!.Level.GetEnumLabel()),
            ]
        );

        publicDataDbContext.LocationOptionMetaChanges.AddRange(
            [
                .. optionMetaDeletions.NaturalOrderBy(c => c.PreviousState!.Option.Label),
                .. optionMetaUpdates.NaturalOrderBy(c => c.PreviousState!.Option.Label),
                .. optionMetaAdditions.NaturalOrderBy(c => c.CurrentState!.Option.Label),
            ]
        );

        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsLocationOptionEqual(
        LocationOptionMeta locationOptionMeta1,
        LocationOptionMeta locationOptionMeta2
    )
    {
        // We need to check if the label, and any codes have changed.
        // However, the LocationOptionMeta table is constrained so that every row is unique and we only create new options
        // when something has changed. This means we can get away with just checking that the ids are equal.
        return locationOptionMeta1.Id == locationOptionMeta2.Id;
    }

    private async Task CreateGeographicLevelChange(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken
    )
    {
        var oldGeographicLevelMeta = await GetGeographicLevelMeta(previousVersionId, cancellationToken);
        var newGeographicLevelMeta = await GetGeographicLevelMeta(nextVersionId, cancellationToken);

        var levelsAreEqual = newGeographicLevelMeta.Levels.Order().SequenceEqual(oldGeographicLevelMeta.Levels.Order());

        if (!levelsAreEqual)
        {
            publicDataDbContext.GeographicLevelMetaChanges.Add(
                new GeographicLevelMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    PreviousStateId = oldGeographicLevelMeta.Id,
                    CurrentStateId = newGeographicLevelMeta.Id,
                }
            );
            await publicDataDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task CreateIndicatorChanges(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken
    )
    {
        var oldMetas = await GetIndicatorMetas(previousVersionId, cancellationToken);
        var newMetas = await GetIndicatorMetas(nextVersionId, cancellationToken);

        var metaDeletions = new List<IndicatorMetaChange>();
        var metaUpdates = new List<IndicatorMetaChange>();
        var metaAdditions = new List<IndicatorMetaChange>();

        foreach (var (indicatorPublicId, oldIndicator) in oldMetas)
        {
            if (newMetas.TryGetValue(indicatorPublicId, out var newIndicator))
            {
                // Indicator updated
                if (!IsIndicatorEqual(newIndicator, oldIndicator))
                {
                    metaUpdates.Add(
                        new IndicatorMetaChange
                        {
                            DataSetVersionId = nextVersionId,
                            PreviousState = oldIndicator,
                            PreviousStateId = oldIndicator.Id,
                            CurrentState = newIndicator,
                            CurrentStateId = newIndicator.Id,
                        }
                    );
                }
            }
            else
            {
                // Indicator deleted
                metaDeletions.Add(
                    new IndicatorMetaChange
                    {
                        DataSetVersionId = nextVersionId,
                        PreviousState = oldIndicator,
                        PreviousStateId = oldIndicator.Id,
                    }
                );
            }
        }

        foreach (var (indicatorPublicId, newIndicator) in newMetas)
        {
            if (!oldMetas.TryGetValue(indicatorPublicId, out _))
            {
                // Indicator added
                metaAdditions.Add(
                    new IndicatorMetaChange
                    {
                        DataSetVersionId = nextVersionId,
                        CurrentState = newIndicator,
                        CurrentStateId = newIndicator.Id,
                    }
                );
            }
        }

        // The meta changes are inserted into the DB in the following order:
        // - Deletions
        // - Updates
        // - Additions
        publicDataDbContext.IndicatorMetaChanges.AddRange(
            [
                .. metaDeletions.NaturalOrderBy(c => c.PreviousState!.Label),
                .. metaUpdates.NaturalOrderBy(c => c.PreviousState!.Label),
                .. metaAdditions.NaturalOrderBy(c => c.CurrentState!.Label),
            ]
        );

        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsIndicatorEqual(IndicatorMeta indicatorMeta1, IndicatorMeta indicatorMeta2)
    {
        return indicatorMeta1.Column == indicatorMeta2.Column
            && indicatorMeta1.Label == indicatorMeta2.Label
            && indicatorMeta1.Unit == indicatorMeta2.Unit
            && indicatorMeta1.DecimalPlaces == indicatorMeta2.DecimalPlaces;
    }

    private async Task CreateTimePeriodChanges(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken
    )
    {
        var oldTimePeriodMetas = await GetTimePeriodMetas(previousVersionId, cancellationToken);
        var newTimePeriodMetas = await GetTimePeriodMetas(nextVersionId, cancellationToken);

        var timePeriodMetaDeletions = oldTimePeriodMetas
            .Except(newTimePeriodMetas, TimePeriodMeta.CodePeriodComparer)
            .NaturalOrderBy(timePeriodMeta => timePeriodMeta.Period)
            .ThenBy(timePeriodMeta => timePeriodMeta.Code)
            .Select(timePeriodMeta => new TimePeriodMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = timePeriodMeta.Id,
                CurrentStateId = null,
            })
            .ToList();

        var timePeriodMetaAdditions = newTimePeriodMetas
            .Except(oldTimePeriodMetas, TimePeriodMeta.CodePeriodComparer)
            .NaturalOrderBy(timePeriodMeta => timePeriodMeta.Period)
            .ThenBy(timePeriodMeta => timePeriodMeta.Code)
            .Select(timePeriodMeta => new TimePeriodMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = null,
                CurrentStateId = timePeriodMeta.Id,
            })
            .ToList();

        publicDataDbContext.TimePeriodMetaChanges.AddRange([.. timePeriodMetaDeletions, .. timePeriodMetaAdditions]);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<
        Dictionary<string, (FilterMeta FilterMeta, Dictionary<string, FilterOptionMetaLink> OptionLinks)>
    > GetFilterMetas(Guid dataSetVersionId, CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .FilterMetas.Include(m => m.OptionLinks)
            .ThenInclude(l => l.Option)
            .Where(m => m.DataSetVersionId == dataSetVersionId)
            .ToDictionaryAsync(
                m => m.PublicId,
                m => (FilterMeta: m, OptionLinks: m.OptionLinks.ToDictionary(l => l.PublicId)),
                cancellationToken
            );
    }

    private async Task<GeographicLevelMeta> GetGeographicLevelMeta(
        Guid dataSetVersionId,
        CancellationToken cancellationToken
    )
    {
        return await publicDataDbContext
            .GeographicLevelMetas.AsNoTracking()
            .SingleAsync(m => m.DataSetVersionId == dataSetVersionId, cancellationToken);
    }

    private async Task<Dictionary<string, IndicatorMeta>> GetIndicatorMetas(
        Guid dataSetVersionId,
        CancellationToken cancellationToken
    )
    {
        return await publicDataDbContext
            .IndicatorMetas.Where(m => m.DataSetVersionId == dataSetVersionId)
            .ToDictionaryAsync(i => i.PublicId, cancellationToken);
    }

    private async Task<
        Dictionary<GeographicLevel, (LocationMeta LocationMeta, Dictionary<string, LocationOptionMetaLink> OptionLinks)>
    > GetLocationMetas(Guid previousVersionId, CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .LocationMetas.Include(lm => lm.OptionLinks)
            .ThenInclude(oml => oml.Option)
            .Where(lm => lm.DataSetVersionId == previousVersionId)
            .ToDictionaryAsync(
                lm => lm.Level,
                lm => (LocationMeta: lm, OptionLinks: lm.OptionLinks.ToDictionary(loml => loml.PublicId)),
                cancellationToken
            );
    }

    private async Task<IReadOnlyList<TimePeriodMeta>> GetTimePeriodMetas(
        Guid dataSetVersionId,
        CancellationToken cancellationToken
    )
    {
        return await publicDataDbContext
            .TimePeriodMetas.AsNoTracking()
            .Where(tpm => tpm.DataSetVersionId == dataSetVersionId)
            .ToListAsync(cancellationToken);
    }
}
