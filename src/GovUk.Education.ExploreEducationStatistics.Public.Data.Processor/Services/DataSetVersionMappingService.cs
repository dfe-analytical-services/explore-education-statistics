using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

internal class DataSetVersionMappingService(
    IDataSetMetaService dataSetMetaService,
    PublicDataDbContext publicDataDbContext)
    : IDataSetVersionMappingService
{
    private static readonly MappingType[] IncompleteMappingTypes =
    [
        MappingType.None,
        MappingType.AutoNone
    ];

    private static Func<LocationOptionMetaRow, string> LocationOptionKeyGenerator =>
        option => $"{option.Label} :: {option.GetRowKey()}";

    private static Func<FilterMeta, string> FilterKeyGenerator =>
        filter => filter.PublicId;

    private static Func<FilterOptionMeta, string> FilterOptionKeyGenerator =>
        option => option.Label;

    public async Task<Either<ActionResult, Unit>> CreateMappings(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        var nextVersion = await publicDataDbContext
            .DataSetVersions
            .Include(dsv => dsv.DataSet)
            .ThenInclude(ds => ds.LatestLiveVersion)
            .SingleAsync(dsv => dsv.Id == nextDataSetVersionId, cancellationToken);

        var liveVersion = nextVersion.DataSet.LatestLiveVersion!;

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

    public Task<Either<ActionResult, Tuple<DataSetVersion, DataSetVersionImport>>> GetManualMappingVersionAndImport(
        NextDataSetVersionCompleteImportRequest request,
        CancellationToken cancellationToken = default)
    {
        return GetNextDataSetVersionInMappingStatus(request, cancellationToken)
            .OnSuccessDo(_ => GetCompletedDataSetVersionMapping(request, cancellationToken))
            .OnSuccessCombineWith(nextDataSetVersion =>
                GetImportInManualMappingStage(request, nextDataSetVersion));
    }
    
    private static Either<ActionResult, DataSetVersionImport> GetImportInManualMappingStage(
        NextDataSetVersionCompleteImportRequest request,
        DataSetVersion nextDataSetVersion)
    {
        var importToContinue = nextDataSetVersion
            .Imports
            .SingleOrDefault(import => import.DataSetVersionId == nextDataSetVersion.Id
                                       && import.Stage == DataSetVersionImportStage.ManualMapping);

        return importToContinue is null
            ? CreateDataSetVersionIdError(
                message: ValidationMessages.ImportInManualMappingStateNotFound,
                dataSetVersionId: request.DataSetVersionId,
                nameof(NextDataSetVersionCompleteImportRequest.DataSetVersionId).ToLowerFirst())
            : importToContinue;
    }

    private async Task<Either<ActionResult, DataSetVersion>> GetNextDataSetVersionInMappingStatus(
        NextDataSetVersionCompleteImportRequest request,
        CancellationToken cancellationToken)
    {
        var nextVersion = await publicDataDbContext
            .DataSetVersions
            .AsNoTracking()
            .Include(dataSetVersion => dataSetVersion.Imports)
            .SingleOrDefaultAsync(
                dataSetVersion => dataSetVersion.Id == request.DataSetVersionId,
                cancellationToken);

        if (nextVersion is null)
        {
            return ValidationUtils.NotFoundResult<DataSetVersion, Guid>(
                request.DataSetVersionId, 
                nameof(NextDataSetVersionCompleteImportRequest.DataSetVersionId).ToLowerFirst());
        }

        if (nextVersion.Status != DataSetVersionStatus.Mapping)
        {
            return CreateDataSetVersionIdError(
                message: ValidationMessages.DataSetVersionNotInMappingStatus,
                dataSetVersionId: request.DataSetVersionId,
                nameof(NextDataSetVersionCompleteImportRequest.DataSetVersionId).ToLowerFirst());
        }

        return nextVersion;
    }

    private async Task<Either<ActionResult, DataSetVersionMapping>> GetCompletedDataSetVersionMapping(
        NextDataSetVersionCompleteImportRequest request,
        CancellationToken cancellationToken)
    {
        var mapping = await publicDataDbContext
            .DataSetVersionMappings
            .AsNoTracking()
            .SingleOrDefaultAsync(
                mapping => mapping.TargetDataSetVersionId == request.DataSetVersionId,
                cancellationToken);

        if (mapping is null)
        {
            return CreateDataSetVersionIdError(
                message: ValidationMessages.DataSetVersionMappingNotFound,
                dataSetVersionId: request.DataSetVersionId,
                nameof(NextDataSetVersionCompleteImportRequest.DataSetVersionId).ToLowerFirst());
        }

        if (!mapping.FilterMappingsComplete || !mapping.LocationMappingsComplete)
        {
            return CreateDataSetVersionIdError(
                message: ValidationMessages.DataSetVersionMappingsNotComplete,
                dataSetVersionId: request.DataSetVersionId,
                nameof(NextDataSetVersionCompleteImportRequest.DataSetVersionId).ToLowerFirst());
        }

        return mapping;
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
                            .Select(option => option.ToRow())
                            .ToDictionary(
                                keySelector: MappingKeyFunctions.LocationOptionKeyGenerator,
                                elementSelector: option => new LocationOptionMapping
                                {
                                    Source = CreateLocationOptionFromMetaRow(option)
                                }),
                        Candidates = candidatesForLevel
                            .ToDictionary(
                                keySelector: MappingKeyFunctions.LocationOptionKeyGenerator,
                                elementSelector: CreateLocationOptionFromMetaRow)
                    };
                });

        var sourceLevels = sourceMappingsByLevel.Select(level => level.Key);

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
                            keySelector: MappingKeyFunctions.LocationOptionKeyGenerator,
                            elementSelector: CreateLocationOptionFromMetaRow)
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
                keySelector: MappingKeyFunctions.FilterKeyGenerator,
                elementSelector: filter =>
                    new FilterMapping
                    {
                        Source = new MappableFilter(filter.Label),
                        OptionMappings = filter
                            .Options
                            .ToDictionary(
                                keySelector: MappingKeyFunctions.FilterOptionKeyGenerator,
                                elementSelector: option =>
                                    new FilterOptionMapping { Source = CreateFilterOptionFromMetaRow(option) })
                    });

        var filterTargets = targetFilterMeta
            .Select(meta => (
                filterMeta: meta.Key,
                optionsMeta: meta.Value))
            .ToDictionary(
                keySelector: meta => MappingKeyFunctions.FilterKeyGenerator(meta.filterMeta),
                elementSelector: meta =>
                    new FilterMappingCandidate(meta.filterMeta.Label)
                    {
                        Options = meta.optionsMeta
                            .ToDictionary(
                                keySelector: MappingKeyFunctions.FilterOptionKeyGenerator,
                                elementSelector: CreateFilterOptionFromMetaRow)
                    });

        var filters = new FilterMappingPlan
        {
            Mappings = filterMappings,
            Candidates = filterTargets
        };

        return filters;
    }

    private static MappableLocationOption CreateLocationOptionFromMetaRow(LocationOptionMetaRow option)
    {
        return new MappableLocationOption(option.Label)
        {
            Code = option.Code,
            OldCode = option.OldCode,
            Ukprn = option.Ukprn,
            Urn = option.Urn,
            LaEstab = option.LaEstab
        };
    }

    private static MappableFilterOption CreateFilterOptionFromMetaRow(FilterOptionMeta option)
    {
        return new MappableFilterOption(option.Label);
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

    private static BadRequestObjectResult CreateDataSetVersionIdError(
        LocalizableMessage message,
        Guid dataSetVersionId,
        string parameterPath)
    {
        return ValidationUtils.ValidationResult(new ErrorViewModel
        {
            Code = message.Code,
            Message = message.Message,
            Path = parameterPath,
            Detail = new InvalidErrorDetail<Guid>(dataSetVersionId)
        });
    }
}
