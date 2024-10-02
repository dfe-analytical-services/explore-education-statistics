using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

internal class DataSetVersionChangeService(PublicDataDbContext publicDataDbContext)
    : IDataSetVersionChangeService
{
    public async Task CreateChanges(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        var nextVersion = await publicDataDbContext
            .DataSetVersions
            .Include(dsv => dsv.DataSet)
            .SingleAsync(dsv => dsv.Id == nextDataSetVersionId, cancellationToken);

        var previousVersionId = nextVersion.DataSet.LatestLiveVersionId!.Value;

        await publicDataDbContext.RequireTransaction(async () =>
        {
            await CreateFilterChanges(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken);

            await CreateLocationChanges(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken);

            await CreateGeographicLevelChange(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken);

            await CreateIndicatorChanges(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken);

            await CreateTimePeriodChanges(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken);
        });
    }

    private async Task CreateFilterChanges(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken)
    {
        var oldMetas = await GetFilterMetas(previousVersionId, cancellationToken);
        var newMetas = await GetFilterMetas(nextVersionId, cancellationToken);

        var metaDeletionsAndChanges = new List<FilterMetaChange>();
        var optionMetaDeletionsAndChanges = new List<FilterOptionMetaChange>();
        var metaAdditions = new List<FilterMetaChange>();
        var optionMetaAdditions = new List<FilterOptionMetaChange>();

        foreach (var (filterPublicId, oldFilterTuple) in oldMetas)
        {
            if (newMetas.TryGetValue(filterPublicId, out var newFilterTuple))
            {
                // Filter changed
                if (!IsFilterEqual(newFilterTuple.FilterMeta, oldFilterTuple.FilterMeta))
                {
                    metaDeletionsAndChanges.Add(new FilterMetaChange
                        {
                            DataSetVersionId = nextVersionId,
                            PreviousState = oldFilterTuple.FilterMeta,
                            PreviousStateId = oldFilterTuple.FilterMeta.Id,
                            CurrentStateId = newFilterTuple.FilterMeta.Id
                        });
                }

                foreach (var (optionPublicId, oldOptionLink) in oldFilterTuple.OptionLinks)
                {
                    if (!newFilterTuple.OptionLinks.TryGetValue(optionPublicId, out var newOptionLink))
                    {
                        // Filter option deleted
                        optionMetaDeletionsAndChanges.Add(new FilterOptionMetaChange
                            {
                                DataSetVersionId = nextVersionId,
                                PreviousState = FilterOptionMetaChange.State.Create(oldOptionLink)
                            });
                    }
                    // Filter option changed
                    else if (!IsFilterOptionEqual(newOptionLink.Option, oldOptionLink.Option))
                    {
                        optionMetaDeletionsAndChanges.Add(new FilterOptionMetaChange
                            {
                                DataSetVersionId = nextVersionId,
                                PreviousState = FilterOptionMetaChange.State.Create(oldOptionLink),
                                CurrentState = FilterOptionMetaChange.State.Create(newOptionLink)
                            });
                    }
                }
            }
            else
            {
                // Filter deleted
                metaDeletionsAndChanges.Add(new FilterMetaChange
                    {
                        DataSetVersionId = nextVersionId,
                        PreviousState = oldFilterTuple.FilterMeta,
                        PreviousStateId = oldFilterTuple.FilterMeta.Id,
                    });
            }
        }

        foreach (var (filterPublicId, newFilterTuple) in newMetas)
        {
            if (!oldMetas.TryGetValue(filterPublicId, out var oldFilterTuple))
            {
                // Filter added
                metaAdditions.Add(new FilterMetaChange
                    {
                        DataSetVersionId = nextVersionId,
                        CurrentState = newFilterTuple.FilterMeta,
                        CurrentStateId = newFilterTuple.FilterMeta.Id
                    });

                foreach (var (_, newOptionLink) in newFilterTuple.OptionLinks)
                {
                    // Filter option added
                    optionMetaAdditions.Add(new FilterOptionMetaChange
                        {
                            DataSetVersionId = nextVersionId,
                            CurrentState = FilterOptionMetaChange.State.Create(newOptionLink)
                        });
                }
            }
            else
            {
                foreach (var (optionPublicId, newOptionLink) in newFilterTuple.OptionLinks)
                {
                    if (!oldFilterTuple.OptionLinks.ContainsKey(optionPublicId))
                    {
                        // Filter option added
                        optionMetaAdditions.Add(new FilterOptionMetaChange
                            {
                                DataSetVersionId = nextVersionId,
                                CurrentState = FilterOptionMetaChange.State.Create(newOptionLink)
                            });
                    }
                }
            }
        }

        metaDeletionsAndChanges = [.. metaDeletionsAndChanges.NaturalOrderBy(c => c.PreviousState!.Label)];

        optionMetaDeletionsAndChanges = [.. optionMetaDeletionsAndChanges.NaturalOrderBy(c => c.PreviousState!.Option.Label)];

        // Additions are inserted into the DB last
        metaAdditions = [.. metaAdditions.NaturalOrderBy(c => c.CurrentState!.Label)];

        optionMetaAdditions = [.. optionMetaAdditions.NaturalOrderBy(c => c.CurrentState!.Option.Label)];

        publicDataDbContext.FilterMetaChanges.AddRange([.. metaDeletionsAndChanges, .. metaAdditions]);
        publicDataDbContext.FilterOptionMetaChanges.AddRange([.. optionMetaDeletionsAndChanges, .. optionMetaAdditions]);
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
        CancellationToken cancellationToken)
    {
        var oldMetas = await GetLocationMetas(previousVersionId, cancellationToken);
        var newMetas = await GetLocationMetas(nextVersionId, cancellationToken);

        var metaDeletions = new List<LocationMetaChange>();
        var optionMetaDeletionsAndChanges = new List<LocationOptionMetaChange>();
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
                        optionMetaDeletionsAndChanges.Add(new LocationOptionMetaChange
                        {
                            DataSetVersionId = nextVersionId,
                            PreviousState = LocationOptionMetaChange.State.Create(oldOptionLink)
                        });
                    }
                    // Location option changed
                    else if (!IsLocationOptionEqual(newOptionLink.Option, oldOptionLink.Option))
                    {
                        optionMetaDeletionsAndChanges.Add(new LocationOptionMetaChange
                        {
                            DataSetVersionId = nextVersionId,
                            PreviousState = LocationOptionMetaChange.State.Create(oldOptionLink),
                            CurrentState = LocationOptionMetaChange.State.Create(newOptionLink)
                        });
                    }
                }
            }
            else
            {
                // Location deleted
                metaDeletions.Add(new LocationMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    PreviousState = oldLocationTuple.LocationMeta,
                    PreviousStateId = oldLocationTuple.LocationMeta.Id,
                });
            }
        }

        foreach (var (locationLevel, newLocationTuple) in newMetas)
        {
            if (!oldMetas.TryGetValue(locationLevel, out var oldLocationTuple))
            {
                // Location added
                metaAdditions.Add(new LocationMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    CurrentState = newLocationTuple.LocationMeta,
                    CurrentStateId = newLocationTuple.LocationMeta.Id
                });

                foreach (var (_, newOptionLink) in newLocationTuple.OptionLinks)
                {
                    // Location option added
                    optionMetaAdditions.Add(new LocationOptionMetaChange
                    {
                        DataSetVersionId = nextVersionId,
                        CurrentState = LocationOptionMetaChange.State.Create(newOptionLink)
                    });
                }
            }
            else
            {
                foreach (var (optionPublicId, newOptionLink) in newLocationTuple.OptionLinks)
                {
                    if (!oldLocationTuple.OptionLinks.ContainsKey(optionPublicId))
                    {
                        // Location option added
                        optionMetaAdditions.Add(new LocationOptionMetaChange
                        {
                            DataSetVersionId = nextVersionId,
                            CurrentState = LocationOptionMetaChange.State.Create(newOptionLink)
                        });
                    }
                }
            }
        }

        metaDeletions = [.. metaDeletions.NaturalOrderBy(c => c.PreviousState!.Level.GetEnumLabel())];

        optionMetaDeletionsAndChanges = [.. optionMetaDeletionsAndChanges.NaturalOrderBy(c => c.PreviousState!.Option.Label)];

        // Additions are inserted into the DB last
        metaAdditions = [.. metaAdditions.NaturalOrderBy(c => c.CurrentState!.Level.GetEnumLabel())];

        optionMetaAdditions = [.. optionMetaAdditions.NaturalOrderBy(c => c.CurrentState!.Option.Label)];

        publicDataDbContext.LocationMetaChanges.AddRange([.. metaDeletions, .. metaAdditions]);
        publicDataDbContext.LocationOptionMetaChanges.AddRange([.. optionMetaDeletionsAndChanges, .. optionMetaAdditions]);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsLocationOptionEqual(LocationOptionMeta locationOptionMeta1, LocationOptionMeta locationOptionMeta2)
    {
        return locationOptionMeta1.Label == locationOptionMeta2.Label;
    }

    private async Task CreateGeographicLevelChange(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken)
    {
        var oldGeographicLevelMeta = await GetGeographicLevelMeta(previousVersionId, cancellationToken);
        var newGeographicLevelMeta = await GetGeographicLevelMeta(nextVersionId, cancellationToken);

        var levelsAreEqual =
            newGeographicLevelMeta.Levels.Order().SequenceEqual(oldGeographicLevelMeta.Levels.Order());

        if (!levelsAreEqual)
        {
            publicDataDbContext.GeographicLevelMetaChanges.Add(new GeographicLevelMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = oldGeographicLevelMeta.Id,
                CurrentStateId = newGeographicLevelMeta.Id
            });
            await publicDataDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task CreateIndicatorChanges(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken)
    {
        var oldMetas = await GetIndicatorMetas(previousVersionId, cancellationToken);
        var newMetas = await GetIndicatorMetas(nextVersionId, cancellationToken);

        var metaDeletionsAndChanges = new List<IndicatorMetaChange>();
        var metaAdditions = new List<IndicatorMetaChange>();

        foreach (var (indicatorPublicId, oldIndicator) in oldMetas)
        {
            if (newMetas.TryGetValue(indicatorPublicId, out var newIndicator))
            {
                // Indicator changed
                if (!IsIndicatorEqual(newIndicator, oldIndicator))
                {
                    metaDeletionsAndChanges.Add(new IndicatorMetaChange
                    {
                        DataSetVersionId = nextVersionId,
                        PreviousState = oldIndicator,
                        PreviousStateId = oldIndicator.Id,
                        CurrentStateId = newIndicator.Id
                    });
                }
            }
            else
            {
                // Indicator deleted
                metaDeletionsAndChanges.Add(new IndicatorMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    PreviousState = oldIndicator,
                    PreviousStateId = oldIndicator.Id,
                });
            }
        }

        foreach (var (indicatorPublicId, newIndicator) in newMetas)
        {
            if (!oldMetas.TryGetValue(indicatorPublicId, out var oldIndicator))
            {
                // Indicator added
                metaAdditions.Add(new IndicatorMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    CurrentState = newIndicator,
                    CurrentStateId = newIndicator.Id
                });
            }
        }

        metaDeletionsAndChanges = [.. metaDeletionsAndChanges.NaturalOrderBy(c => c.PreviousState!.Label)];

        // Additions are inserted into the DB last
        metaAdditions = [.. metaAdditions.NaturalOrderBy(c => c.CurrentState!.Label)];

        publicDataDbContext.IndicatorMetaChanges.AddRange([.. metaDeletionsAndChanges, .. metaAdditions]);
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
        CancellationToken cancellationToken)
    {
        var oldTimePeriodMetas = await GetTimePeriodMetas(previousVersionId, cancellationToken);
        var newTimePeriodMetas = await GetTimePeriodMetas(nextVersionId, cancellationToken);

        var timePeriodMetaDeletions = oldTimePeriodMetas
            .Except(newTimePeriodMetas, TimePeriodMeta.CodePeriodComparer)
            .OrderBy(timePeriodMeta => timePeriodMeta.Period)
            .ThenBy(timePeriodMeta => timePeriodMeta.Code)
            .Select(timePeriodMeta => new TimePeriodMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = timePeriodMeta.Id,
                CurrentStateId = null
            })
            .ToList();

        var timePeriodMetaAdditions = newTimePeriodMetas
            .Except(oldTimePeriodMetas, TimePeriodMeta.CodePeriodComparer)
            .OrderBy(timePeriodMeta => timePeriodMeta.Period)
            .ThenBy(timePeriodMeta => timePeriodMeta.Code)
            .Select(timePeriodMeta => new TimePeriodMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = null,
                CurrentStateId = timePeriodMeta.Id
            })
            .ToList();

        publicDataDbContext.TimePeriodMetaChanges.AddRange([.. timePeriodMetaDeletions, .. timePeriodMetaAdditions]);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async
        Task<Dictionary<string, (FilterMeta FilterMeta, Dictionary<string, FilterOptionMetaLink> OptionLinks)>>
        GetFilterMetas(
            Guid dataSetVersionId,
            CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .FilterMetas
            .Include(m => m.OptionLinks)
            .ThenInclude(l => l.Option)
            .Where(m => m.DataSetVersionId == dataSetVersionId)
            .ToDictionaryAsync(
                m => m.PublicId,
                m => (
                    FilterMeta: m,
                    OptionLinks: m.OptionLinks.ToDictionary(l => l.PublicId)
                ),
                cancellationToken);
    }

    private async Task<GeographicLevelMeta> GetGeographicLevelMeta(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .GeographicLevelMetas
            .AsNoTracking()
            .SingleAsync(m => m.DataSetVersionId == dataSetVersionId, cancellationToken);
    }

    private async Task<IReadOnlyDictionary<string, IndicatorMeta>> GetIndicatorMetas(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .IndicatorMetas
            .Where(m => m.DataSetVersionId == dataSetVersionId)
            .ToDictionaryAsync(
                i => i.PublicId,
                cancellationToken);
    }

    private async Task<Dictionary<GeographicLevel,
        (LocationMeta LocationMeta, Dictionary<string, LocationOptionMetaLink> OptionLinks)>> GetLocationMetas(
        Guid previousVersionId, CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .LocationMetas
            .Include(lm => lm.OptionLinks)
            .ThenInclude(oml => oml.Option)
            .Where(lm => lm.DataSetVersionId == previousVersionId)
            .ToDictionaryAsync(
                lm => lm.Level,
                lm =>
                (
                    LocationMeta: lm,
                    OptionLinks: lm.OptionLinks.ToDictionary(loml => loml.PublicId)
                ),
                cancellationToken);
    }

    private async Task<IReadOnlyList<TimePeriodMeta>> GetTimePeriodMetas(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .TimePeriodMetas
            .Where(tpm => tpm.DataSetVersionId == dataSetVersionId)
            .ToListAsync(cancellationToken);
    }
}
