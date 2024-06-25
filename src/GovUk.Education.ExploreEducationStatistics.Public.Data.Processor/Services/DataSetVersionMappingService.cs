using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

internal class DataSetVersionMappingService(
    IDataSetMetaService dataSetMetaService,
    PublicDataDbContext publicDataDbContext)
    : IDataSetVersionMappingService
{
    public static readonly MappingType[] IncompleteMappingTypes =
    [
        MappingType.None,
        MappingType.AutoNone
    ];

    public async Task<Either<ActionResult, Unit>> CreateMappings(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        var liveVersion = (await publicDataDbContext
            .DataSetVersions
            .Where(dsv => dsv.Id == nextDataSetVersionId)
            .Select(dsv => dsv.DataSet.LatestLiveVersion)
            .SingleAsync(cancellationToken))!;

        var nextVersion = await publicDataDbContext
            .DataSetVersions
            .SingleAsync(dsv => dsv.Id == nextDataSetVersionId, cancellationToken);

        var nextVersionMeta = await dataSetMetaService.ReadDataSetVersionMetaForMappings(
            dataSetVersionId: nextDataSetVersionId,
            cancellationToken);

        var sourceLocationMeta =
            await GetLocationMeta(liveVersion.Id, cancellationToken);

        var locationMappings = CreateLocationMappings(
            sourceLocationMeta,
            nextVersionMeta.Locations);

        var sourceFilterMeta =
            await GetFilterMeta(liveVersion.Id, cancellationToken);

        var filterMappings = CreateFilterMappings(
            sourceFilterMeta,
            nextVersionMeta.Filters);

        nextVersion.MetaSummary = nextVersionMeta.MetaSummary;

        publicDataDbContext
            .DataSetVersionMappings
            .Add(new DataSetVersionMapping
            {
                SourceDataSetVersionId = liveVersion.Id,
                TargetDataSetVersionId = nextDataSetVersionId,
                LocationMappingPlan = locationMappings,
                FilterMappingPlan = filterMappings
            });

        await publicDataDbContext.SaveChangesAsync(cancellationToken);
        return Unit.Instance;
    }

    public async Task ApplyAutoMappings(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        var mappings = await publicDataDbContext
            .DataSetVersionMappings
            .SingleAsync(
                mapping => mapping.TargetDataSetVersionId == nextDataSetVersionId,
                cancellationToken);

        AutoMapLocations(mappings.LocationMappingPlan);
        AutoMapFilters(mappings.FilterMappingPlan);

        mappings.LocationMappingsComplete = !mappings
            .LocationMappingPlan
            .Levels
            .Any(level => level
                .Value
                .Mappings
                .Any(optionMapping =>
                    IncompleteMappingTypes.Contains(optionMapping.Value.Type)));

        mappings.FilterMappingsComplete = !mappings
            .FilterMappingPlan
            .Mappings
            .Any(filterMapping =>
                IncompleteMappingTypes.Contains(filterMapping.Value.Type)
                || filterMapping
                    .Value
                    .OptionMappings
                    .Any(optionMapping => IncompleteMappingTypes.Contains(optionMapping.Value.Type)));

        publicDataDbContext.Update(mappings);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private static void AutoMapLocations(LocationMappingPlan locationPlan)
    {
        locationPlan
            .Levels
            .ForEach(level => level.Value.Mappings
                .ForEach(locationMapping => AutoMapElement(
                    sourceKey: locationMapping.Key,
                    mapping: locationMapping.Value,
                    candidates: level
                        .Value
                        .Candidates)));
    }

    private static void AutoMapFilters(FilterMappingPlan filtersPlan)
    {
        filtersPlan
            .Mappings
            .ForEach(filterMapping => AutoMapParentAndOptions(
                sourceParentKey: filterMapping.Key,
                parentMapping: filterMapping.Value,
                parentCandidates: filtersPlan.Candidates,
                candidateOptionsSupplier: autoMappedCandidate => autoMappedCandidate.Options));
    }

    private static TCandidate? AutoMapElement<TMappableElement, TCandidate>(
        string sourceKey,
        Mapping<TMappableElement> mapping,
        Dictionary<string, TCandidate> candidates)
        where TMappableElement : MappableElement
        where TCandidate : MappableElement
    {
        if (candidates.Count == 0)
        {
            mapping.Type = MappingType.AutoNone;
            return null;
        }

        var matchingCandidate = candidates.GetValueOrDefault(sourceKey);    

        if (matchingCandidate is not null)
        {
            mapping.CandidateKey = sourceKey;
            mapping.Type = MappingType.AutoMapped;
        }
        else
        {
            mapping.CandidateKey = null;
            mapping.Type = MappingType.AutoNone;
        }

        return matchingCandidate;
    }

    private static void AutoMapParentAndOptions<TMappableParent, TParentCandidate, TMappableOption, TOptionMapping>(
        string sourceParentKey,
        ParentMapping<TMappableParent, TMappableOption, TOptionMapping> parentMapping,
        Dictionary<string, TParentCandidate> parentCandidates,
        Func<TParentCandidate, Dictionary<string, TMappableOption>> candidateOptionsSupplier)
        where TMappableParent : MappableElement
        where TParentCandidate : MappableElement
        where TMappableOption : MappableElement
        where TOptionMapping : Mapping<TMappableOption>
    {
        var autoMapCandidate = AutoMapElement(
            sourceKey: sourceParentKey,
            mapping: parentMapping,
            candidates: parentCandidates);

        if (autoMapCandidate is not null)
        {
            var candidateOptions = candidateOptionsSupplier.Invoke(autoMapCandidate);

            parentMapping
                .OptionMappings
                .ForEach(optionMapping => AutoMapElement(
                    sourceKey: optionMapping.Key,
                    mapping: optionMapping.Value,
                    candidates: candidateOptions));
        }
        else
        {
            parentMapping
                .OptionMappings
                .Select(optionMapping => optionMapping.Value)
                .ForEach(optionMapping =>
                {
                    optionMapping.CandidateKey = null;
                    optionMapping.Type = parentMapping.Type;
                });
        }
    }

    private LocationMappingPlan CreateLocationMappings(
        List<LocationMeta> sourceLocationMeta,
        IDictionary<LocationMeta, List<LocationOptionMetaRow>> targetLocationMeta)
    {
        // Create mappings by level for each Geographic Level that appeared in the source data set version.
        var sourceMappingsByLevel = sourceLocationMeta
            .ToDictionary(
                keySelector: level => level.Level,
                elementSelector: level =>
                {
                    var candidatesForLevel = targetLocationMeta
                        .Any(entry => entry.Key.Level == level.Level)
                        ? targetLocationMeta
                            .Single(entry => entry.Key.Level == level.Level)
                            .Value
                        : [];

                    return new LocationLevelMappings
                    {
                        Mappings = level
                            .Options
                            .ToDictionary(
                                keySelector: option => $"{option.Label} :: {option.ToRow().GetRowKey()}",
                                elementSelector: option => new LocationOptionMapping
                                {
                                    Source = new LocationOption
                                    {
                                        Label = option.Label,
                                    },
                                    Type = MappingType.AutoNone
                                }),
                        Candidates = candidatesForLevel
                            .ToDictionary(
                                keySelector: option => $"{option.Label} :: {option.GetRowKey()}",
                                elementSelector: option => new LocationOption
                                {
                                    Label = option.Label
                                })
                    };
                });

        var sourceLevels = sourceMappingsByLevel
            .Select(level => level.Key)
            .Distinct();

        // Additionally find any Geographic Levels that appear in the target data set version but not in the source,
        // and create mappings by level for them.
        var onlyTargetMappingsByLevel = targetLocationMeta
            .Where(entry => !sourceLevels.Contains(entry.Key))
            .Select(meta => (
                levelMeta: meta.Key,
                optionsMeta: meta.Value))
            .ToDictionary(
                keySelector: meta => meta.levelMeta.Level,
                elementSelector: meta => new LocationLevelMappings
                {
                    Mappings = [],
                    Candidates = meta
                        .optionsMeta
                        .ToDictionary(
                            keySelector: option => $"{option.Label} :: {option.GetRowKey()}",
                            elementSelector: option => 
                                new LocationOption
                                {
                                    Label = option.Label
                                })
                });

        return new LocationMappingPlan
        {
            Levels = sourceMappingsByLevel
                .Concat(onlyTargetMappingsByLevel)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value)
        };
    }

    private FilterMappingPlan CreateFilterMappings(
        List<FilterMeta> sourceFilterMeta,
        IDictionary<FilterMeta, List<FilterOptionMeta>> targetFilterMeta)
    {
        var filterMappings = sourceFilterMeta
            .ToDictionary(
                keySelector: filter => filter.Label,
                elementSelector: filter => 
                    new FilterMapping
                    {
                        Source = new Filter
                        {
                            Label = filter.Label
                        },
                        OptionMappings = filter
                            .Options
                            .ToDictionary(
                                keySelector: option => option.Label,
                                elementSelector: option => 
                                    new FilterOptionMapping
                                    {
                                        Source = new FilterOption
                                        {
                                            Label = option.Label
                                        }
                                    })
                    });

        var filterTargets = targetFilterMeta
            .Select(meta => (
                filterMeta: meta.Key,
                optionsMeta: meta.Value))
            .ToDictionary(
                keySelector: meta => meta.filterMeta.Label,
                elementSelector: meta => 
                    new FilterMappingCandidate
                    {
                        Label = meta.filterMeta.Label,
                        Options = meta.optionsMeta
                            .ToDictionary(
                                keySelector: option => option.Label,
                                elementSelector: option => 
                                    new FilterOption
                                    {
                                        Label = option.Label
                                    })
                    });

        var filters = new FilterMappingPlan
        {
            Mappings = filterMappings,
            Candidates = filterTargets
        };

        return filters;
    }

    private async Task<List<LocationMeta>> GetLocationMeta(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .LocationMetas
            .AsNoTracking()
            .Include(levelMeta => levelMeta.Options)
            .Where(meta => meta.DataSetVersionId == dataSetVersionId)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<FilterMeta>> GetFilterMeta(Guid sourceVersionId, CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .FilterMetas
            .AsNoTracking()
            .Include(filterMeta => filterMeta.Options)
            .Where(meta => meta.DataSetVersionId == sourceVersionId)
            .ToListAsync(cancellationToken);
    }
}
