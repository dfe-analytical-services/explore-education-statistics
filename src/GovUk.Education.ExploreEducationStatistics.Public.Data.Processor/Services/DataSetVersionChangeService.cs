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
        var oldFilterMetas = await GetFilterMetas(previousVersionId, cancellationToken);
        var newFilterMetas = await GetFilterMetas(nextVersionId, cancellationToken);

        var filterMetaChanges = new List<FilterMetaChange>();
        var filterOptionMetaChanges = new List<FilterOptionMetaChange>();

        foreach (var (filterPublicId, oldFilterTuple) in oldFilterMetas)
        {
            if (newFilterMetas.TryGetValue(filterPublicId, out var newFilterTuple))
            {
                // Filter changed
                if (!IsFilterEqual(newFilterTuple.FilterMeta, oldFilterTuple.FilterMeta))
                {
                    filterMetaChanges.Add(new FilterMetaChange
                    {
                        DataSetVersionId = nextVersionId,
                        PreviousStateId = oldFilterTuple.FilterMeta.Id,
                        CurrentStateId = newFilterTuple.FilterMeta.Id
                    });
                }

                foreach (var (optionPublicId, oldOptionLink) in oldFilterTuple.OptionLinks)
                {
                    if (!newFilterTuple.OptionLinks.TryGetValue(optionPublicId, out var newOptionLink))
                    {
                        // Filter option deleted
                        filterOptionMetaChanges.Add(new FilterOptionMetaChange
                        {
                            DataSetVersionId = nextVersionId,
                            PreviousState = FilterOptionMetaChange.State.Create(oldOptionLink)
                        });
                    }
                    // Filter option changed
                    else if (!IsFilterOptionEqual(newOptionLink.Option, oldOptionLink.Option))
                    {
                        filterOptionMetaChanges.Add(new FilterOptionMetaChange
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
                filterMetaChanges.Add(new FilterMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    PreviousStateId = oldFilterTuple.FilterMeta.Id,
                });
            }
        }

        foreach (var (filterPublicId, newFilterTuple) in newFilterMetas)
        {
            if (!oldFilterMetas.TryGetValue(filterPublicId, out var oldFilterTuple))
            {
                // Filter added
                filterMetaChanges.Add(new FilterMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    CurrentStateId = newFilterTuple.FilterMeta.Id
                });

                foreach (var (_, newOptionLink) in newFilterTuple.OptionLinks)
                {
                    // Filter option added
                    filterOptionMetaChanges.Add(new FilterOptionMetaChange
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
                        filterOptionMetaChanges.Add(new FilterOptionMetaChange
                        {
                            DataSetVersionId = nextVersionId,
                            CurrentState = FilterOptionMetaChange.State.Create(newOptionLink)
                        });
                    }
                }
            }
        }

        publicDataDbContext.FilterMetaChanges.AddRange(filterMetaChanges);
        publicDataDbContext.FilterOptionMetaChanges.AddRange(filterOptionMetaChanges);
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
        var oldLocationMetas = await GetLocationMetas(previousVersionId, cancellationToken);
        var newLocationMetas = await GetLocationMetas(nextVersionId, cancellationToken);

        var locationMetaChanges = new List<LocationMetaChange>();
        var locationOptionMetaChanges = new List<LocationOptionMetaChange>();

        foreach (var (locationLevel, oldLocationTuple) in oldLocationMetas)
        {
            if (newLocationMetas.TryGetValue(locationLevel, out var newLocationTuple))
            {
                foreach (var (optionPublicId, oldOptionLink) in oldLocationTuple.OptionLinks)
                {
                    if (!newLocationTuple.OptionLinks.TryGetValue(optionPublicId, out var newOptionLink))
                    {
                        // Location option deleted
                        locationOptionMetaChanges.Add(new LocationOptionMetaChange
                        {
                            DataSetVersionId = nextVersionId,
                            PreviousState = LocationOptionMetaChange.State.Create(oldOptionLink)
                        });
                    }
                    // Location option changed
                    else if (!IsLocationOptionEqual(newOptionLink.Option, oldOptionLink.Option))
                    {
                        locationOptionMetaChanges.Add(new LocationOptionMetaChange
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
                locationMetaChanges.Add(new LocationMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    PreviousStateId = oldLocationTuple.LocationMeta.Id,
                });
            }
        }

        foreach (var (locationLevel, newLocationTuple) in newLocationMetas)
        {
            if (!oldLocationMetas.TryGetValue(locationLevel, out var oldLocationTuple))
            {
                // Location added
                locationMetaChanges.Add(new LocationMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    CurrentStateId = newLocationTuple.LocationMeta.Id
                });

                foreach (var (_, newOptionLink) in newLocationTuple.OptionLinks)
                {
                    // Location option added
                    locationOptionMetaChanges.Add(new LocationOptionMetaChange
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
                        locationOptionMetaChanges.Add(new LocationOptionMetaChange
                        {
                            DataSetVersionId = nextVersionId,
                            CurrentState = LocationOptionMetaChange.State.Create(newOptionLink)
                        });
                    }
                }
            }
        }

        publicDataDbContext.LocationMetaChanges.AddRange(locationMetaChanges);
        publicDataDbContext.LocationOptionMetaChanges.AddRange(locationOptionMetaChanges);
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
        var oldIndicatorMetas = await GetIndicatorMetas(previousVersionId, cancellationToken);
        var newIndicatorMetas = await GetIndicatorMetas(nextVersionId, cancellationToken);

        var indicatorMetaChanges = new List<IndicatorMetaChange>();

        foreach (var (indicatorPublicId, oldIndicator) in oldIndicatorMetas)
        {
            if (newIndicatorMetas.TryGetValue(indicatorPublicId, out var newIndicator))
            {
                // Indicator changed
                if (!IsIndicatorEqual(newIndicator, oldIndicator))
                {
                    indicatorMetaChanges.Add(new IndicatorMetaChange
                    {
                        DataSetVersionId = nextVersionId,
                        PreviousStateId = oldIndicator.Id,
                        CurrentStateId = newIndicator.Id
                    });
                }
            }
            else
            {
                // Indicator deleted
                indicatorMetaChanges.Add(new IndicatorMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    PreviousStateId = oldIndicator.Id,
                });
            }
        }

        foreach (var (indicatorPublicId, newIndicator) in newIndicatorMetas)
        {
            if (!oldIndicatorMetas.TryGetValue(indicatorPublicId, out var oldIndicator))
            {
                // Indicator added
                indicatorMetaChanges.Add(new IndicatorMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    CurrentStateId = newIndicator.Id
                });
            }
        }

        publicDataDbContext.IndicatorMetaChanges.AddRange(indicatorMetaChanges);
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
        var filterMetas = await publicDataDbContext
            .FilterMetas
            .Include(m => m.OptionLinks.OrderBy(ol => ol.Option.Label))
            .ThenInclude(l => l.Option)
            .Where(m => m.DataSetVersionId == dataSetVersionId)
            .ToListAsync(cancellationToken);
        
        return filterMetas
            .NaturalOrderBy(m => m.Label)
            .ToDictionary(
                m => m.PublicId,
                m => (
                    FilterMeta: m,
                    OptionLinks: m.OptionLinks
                        .NaturalOrderBy(ol => ol.Option.Label)
                        .ToDictionary(l => l.PublicId)
                ));
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
        var indicatorMetas = await publicDataDbContext
            .IndicatorMetas
            .Where(m => m.DataSetVersionId == dataSetVersionId)
            .ToListAsync(cancellationToken);

        return indicatorMetas
            .NaturalOrderBy(m => m.Label)
            .ToDictionary(i => i.PublicId);
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
            .OrderBy(lm => lm.Level)
            .ToDictionaryAsync(
                lm => lm.Level,
                lm =>
                (
                    LocationMeta: lm,
                    OptionLinks: OrderLocationOptionLinks(lm.Level, lm.OptionLinks)
                        .ToDictionary(loml => loml.PublicId)
                ),
                cancellationToken);
    }

    private static IOrderedEnumerable<LocationOptionMetaLink> OrderLocationOptionLinks(
        GeographicLevel level, 
        IReadOnlyList<LocationOptionMetaLink> optionLinks)
    {
        return level switch
        {
            GeographicLevel.LocalAuthority => optionLinks
                .NaturalOrderBy(ol => ((LocationLocalAuthorityOptionMeta)ol.Option).Label)
                .NaturalThenBy(ol => ((LocationLocalAuthorityOptionMeta)ol.Option).Code)
                .NaturalThenBy(ol => ((LocationLocalAuthorityOptionMeta)ol.Option).OldCode),
            GeographicLevel.Provider => optionLinks
                .NaturalOrderBy(ol => ((LocationProviderOptionMeta)ol.Option).Label)
                .NaturalThenBy(ol => ((LocationProviderOptionMeta)ol.Option).Ukprn),
            GeographicLevel.RscRegion => optionLinks
                .NaturalOrderBy(ol => ((LocationRscRegionOptionMeta)ol.Option).Label),
            GeographicLevel.School => optionLinks
                .NaturalOrderBy(ol => ((LocationSchoolOptionMeta)ol.Option).Label)
                .NaturalThenBy(ol => ((LocationSchoolOptionMeta)ol.Option).Urn)
                .NaturalThenBy(ol => ((LocationSchoolOptionMeta)ol.Option).LaEstab),
            _ => optionLinks
                .NaturalOrderBy(ol => ((LocationCodedOptionMeta)ol.Option).Label)
                .NaturalThenBy(ol => ((LocationCodedOptionMeta)ol.Option).Code)
        };
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
