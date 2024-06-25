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
                .Mappings
                .Any(optionMapping => IncompleteMappingTypes.Contains(optionMapping.Type)));

        mappings.FilterMappingsComplete = !mappings
            .FilterMappingPlan
            .Mappings
            .Any(filterMapping =>
                filterMapping.Type == MappingType.None
                || filterMapping
                    .OptionMappings
                    .Any(optionMapping => IncompleteMappingTypes.Contains(optionMapping.Type)));

        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private static void AutoMapLocations(LocationMappingPlan locationPlan)
    {
        locationPlan
            .Levels
            .ForEach(level => level
                .Mappings
                .ForEach(locationMapping => AutoMapElement(
                    mapping: locationMapping,
                    candidates: level.Candidates)));
    }

    private static void AutoMapFilters(FilterMappingPlan filtersPlan)
    {
        filtersPlan
            .Mappings
            .ForEach(filterMapping => AutoMapParentAndOptions(
                parentMapping: filterMapping,
                parentCandidates: filtersPlan.Candidates,
                candidateOptionsSupplier: autoMappedCandidate => autoMappedCandidate.Options));
    }

    private static TCandidate? AutoMapElement<TMappableElement, TCandidate>(
        Mapping<TMappableElement> mapping,
        List<TCandidate> candidates)
        where TMappableElement : MappableElement
        where TCandidate : MappableElement
    {
        if (candidates.Count == 0)
        {
            mapping.Type = MappingType.AutoNone;
            return null;
        }

        var matchingCandidate = candidates
            .SingleOrDefault(candidate =>
                candidate.Key == mapping.Source.Key);

        if (matchingCandidate is not null)
        {
            mapping.CandidateKey = matchingCandidate.Key;
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
        ParentMapping<TMappableParent, TMappableOption, TOptionMapping> parentMapping,
        List<TParentCandidate> parentCandidates,
        Func<TParentCandidate, List<TMappableOption>> candidateOptionsSupplier)
        where TMappableParent : MappableElement
        where TParentCandidate : MappableElement
        where TMappableOption : MappableElement
        where TOptionMapping : Mapping<TMappableOption>
    {
        var autoMapCandidate = AutoMapElement(parentMapping, parentCandidates);

        if (autoMapCandidate is not null)
        {
            var candidateOptions = candidateOptionsSupplier.Invoke(autoMapCandidate);

            parentMapping
                .OptionMappings
                .ForEach(optionMapping => AutoMapElement(
                    mapping: optionMapping,
                    candidates: candidateOptions));
        }
        else
        {
            parentMapping
                .OptionMappings
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
            .OrderBy(level => level.Level)
            .Select(level =>
            {
                var candidatesForLevel = targetLocationMeta
                    .Any(entry => entry.Key.Level == level.Level)
                    ? targetLocationMeta
                        .Single(entry => entry.Key.Level == level.Level)
                        .Value
                    : [];

                return new LocationLevelMappings
                {
                    Level = level.Level,
                    Mappings = level
                        .Options
                        .OrderBy(option => option.Label)
                        .Select(option => new LocationOptionMapping
                        {
                            Source = new LocationOption
                            {
                                Key = $"{option.Label} :: {option.ToRow().GetRowKey()}",
                                Label = option.Label,
                            },
                            Type = MappingType.AutoNone
                        })
                        .ToList(),
                    Candidates = candidatesForLevel
                        .OrderBy(option => option.Label)
                        .Select(option => new LocationOption
                        {
                            Key = $"{option.Label} :: {option.GetRowKey()}",
                            Label = option.Label
                        })
                        .ToList()
                };
            })
            .ToList();

        var sourceLevels = sourceMappingsByLevel
            .Select(level => level.Level)
            .Distinct();

        // Additionally find any Geographic Levels that appear in the target data set version but not in the source,
        // and create mappings by level for them.
        var onlyTargetMappingsByLevel = targetLocationMeta
            .Where(entry => !sourceLevels.Contains(entry.Key))
            .Select(meta => (
                levelMeta: meta.Key,
                optionsMeta: meta.Value))
            .OrderBy(meta => meta.levelMeta.Level)
            .Select(meta => new LocationLevelMappings
            {
                Level = meta.levelMeta.Level,
                Mappings = [],
                Candidates = meta
                    .optionsMeta
                    .OrderBy(option => option.Label)
                    .Select(option => new LocationOption
                    {
                        Key = $"{option.Label} :: {option.GetRowKey()}",
                        Label = option.Label
                    })
                    .ToList()
            })
            .ToList();

        return new LocationMappingPlan
        {
            Levels =
            [
                ..sourceMappingsByLevel,
                ..onlyTargetMappingsByLevel
            ]
        };
    }

    private FilterMappingPlan CreateFilterMappings(
        List<FilterMeta> sourceFilterMeta,
        IDictionary<FilterMeta, List<FilterOptionMeta>> targetFilterMeta)
    {
        var filterMappings = sourceFilterMeta
            .Select(filter => new FilterMapping
            {
                Source = new Filter
                {
                    Key = filter.Label,
                    Label = filter.Label
                },
                OptionMappings = filter
                    .Options
                    .Select(option => new FilterOptionMapping
                    {
                        Source = new FilterOption
                        {
                            Key = option.Label,
                            Label = option.Label
                        }
                    })
                    .ToList()
            })
            .ToList();

        var filterTargets = targetFilterMeta
            .Select(meta => (
                filterMeta: meta.Key,
                optionsMeta: meta.Value))
            .Select(meta => new FilterMappingCandidate
            {
                Key = meta.filterMeta.Label,
                Label = meta.filterMeta.Label,
                Options = meta.optionsMeta
                    .Select(option => new FilterOption
                    {
                        Key = option.Label,
                        Label = option.Label
                    })
                    .ToList()
            })
            .ToList();

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
