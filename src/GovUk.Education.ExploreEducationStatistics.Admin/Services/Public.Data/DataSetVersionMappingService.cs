#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Public.Data.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ErrorViewModel = GovUk.Education.ExploreEducationStatistics.Common.ViewModels.ErrorViewModel;
using ValidationUtils = GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public class DataSetVersionMappingService(
    IPostgreSqlRepository postgreSqlRepository,
    IUserService userService,
    PublicDataDbContext publicDataDbContext,
    ContentDbContext contentDbContext,
    IMappingTypesRepository mappingTypesRepository)
    : IDataSetVersionMappingService
{
    private static readonly MappingType[] IncompleteMappingTypes =
    [
        MappingType.AutoNone
    ];

    private static readonly MappingType[] NoMappingTypes =
    [
        MappingType.ManualNone,
        MappingType.AutoNone
    ];

    public Task<Either<ActionResult, LocationMappingPlan>> GetLocationMappings(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        return userService
            .CheckIsBauUser()
            .OnSuccess(() => CheckMappingExists(nextDataSetVersionId, cancellationToken))
            .OnSuccess(mapping => mapping.LocationMappingPlan);
    }

    public Task<Either<ActionResult, BatchLocationMappingUpdatesResponseViewModel>> ApplyBatchLocationMappingUpdates(
        Guid nextDataSetVersionId,
        BatchLocationMappingUpdatesRequest request,
        CancellationToken cancellationToken = default)
    {
        return ApplyBatchMappingUpdates(
            nextDataSetVersionId: nextDataSetVersionId,
            validateCandidatesFn: () =>
                ValidateLocationOptionCandidates(nextDataSetVersionId, request, cancellationToken),
            applyUpdatesFn: () => UpdateLocationOptionMappingsBatch(nextDataSetVersionId, request, cancellationToken),
            createViewModelFn: updates =>
                new BatchLocationMappingUpdatesResponseViewModel { Updates = updates },
            cancellationToken: cancellationToken);
    }

    public Task<Either<ActionResult, FilterMappingPlan>> GetFilterMappings(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        return userService
            .CheckIsBauUser()
            .OnSuccess(() => CheckMappingExists(nextDataSetVersionId, cancellationToken))
            .OnSuccess(mapping => mapping.FilterMappingPlan);
    }
    
    public async Task<MappingStatusViewModel?> GetMappingStatus(Guid dataSetVersionId, CancellationToken cancellationToken = default)
    {
        var locationOptionMappingTypes = await mappingTypesRepository.GetLocationOptionMappingTypes(
            dataSetVersionId, 
            cancellationToken);

        var filterAndOptionMappingTypes = await mappingTypesRepository.GetFilterOptionMappingTypes(
            dataSetVersionId, 
            cancellationToken);
        
       var majorChangesStatus = await GetMajorChangesStatus(dataSetVersionId, locationOptionMappingTypes, filterAndOptionMappingTypes, cancellationToken);
       
        return await publicDataDbContext
            .DataSetVersionMappings
            .Where(mapping => mapping.TargetDataSetVersionId == dataSetVersionId)
            .Select(mapping => new MappingStatusViewModel
            {
                LocationsComplete = mapping.LocationMappingsComplete,
                FiltersComplete = mapping.FilterMappingsComplete,
                LocationsHaveMajorChange = majorChangesStatus.LocationsHaveMajorChange,
                FiltersHaveMajorChange = majorChangesStatus.FiltersHaveMajorChange,
                HasDeletionChanges = majorChangesStatus.HasDeletionChanges
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<Either<ActionResult, BatchFilterOptionMappingUpdatesResponseViewModel>>
        ApplyBatchFilterOptionMappingUpdates(
            Guid nextDataSetVersionId,
            BatchFilterOptionMappingUpdatesRequest request,
            CancellationToken cancellationToken = default)
    {
        return ApplyBatchMappingUpdates(
            nextDataSetVersionId: nextDataSetVersionId,
            validateCandidatesFn: () =>
                ValidateFilterOptionCandidates(nextDataSetVersionId, request, cancellationToken),
            applyUpdatesFn: () => UpdateFilterOptionMappingsBatch(nextDataSetVersionId, request, cancellationToken),
            createViewModelFn: updates => new BatchFilterOptionMappingUpdatesResponseViewModel { Updates = updates },
            cancellationToken: cancellationToken);
    }

    private async Task<Either<ActionResult, TBatchMappingUpdatesResponseViewModel>>
        ApplyBatchMappingUpdates<TMappingUpdateResponse, TBatchMappingUpdatesResponseViewModel, TMappingCandidate>(
            Guid nextDataSetVersionId,
            Func<Task<List<Either<ErrorViewModel, TMappingCandidate>>>> validateCandidatesFn,
            Func<Task<List<Either<ErrorViewModel, TMappingUpdateResponse>>>> applyUpdatesFn,
            Func<List<TMappingUpdateResponse>, TBatchMappingUpdatesResponseViewModel> createViewModelFn,
            CancellationToken cancellationToken)
    {
        return await publicDataDbContext.RequireTransaction(async () =>
            await userService
                .CheckIsBauUser()
                .OnSuccess(() => CheckMappingExists(nextDataSetVersionId, cancellationToken))
                .OnSuccess(_ => validateCandidatesFn())
                .OnSuccess(validationResults =>
                {
                    // Take all the candidate validation results from this batch of updates, and check to make sure
                    // they all succeeded.  If any have failed, we return the appropriate validation errors to the
                    // front end.
                    var aggregatedResult = validationResults.AggregateSuccessesAndFailures();

                    return aggregatedResult
                        .OnFailure<ActionResult>(errors => ValidationUtils.ValidationResult(errors));
                })
                .OnSuccess(applyUpdatesFn)
                .OnSuccess(updateSuccessesAndFailures =>
                {
                    // Take all the update results from this batch of updates, and check to make sure they all
                    // succeeded.  If any have failed, we fail the entire transaction and inform the front end of the
                    // particular update requests that failed. 
                    var aggregatedResult = updateSuccessesAndFailures.AggregateSuccessesAndFailures();

                    return aggregatedResult
                        .OnFailure<ActionResult>(errors => ValidationUtils.ValidationResult(errors));
                })
                .OnSuccessDo(() => UpdateMappingsCompleteAndVersion(nextDataSetVersionId, cancellationToken))
                .OnSuccess(createViewModelFn));
    }

    private async Task UpdateMappingsCompleteAndVersion(Guid nextDataSetVersionId, CancellationToken cancellationToken)
    {
        var locationOptionMappingTypes = await mappingTypesRepository.GetLocationOptionMappingTypes(
            nextDataSetVersionId, 
            cancellationToken);

        var filterAndOptionMappingTypes = await mappingTypesRepository.GetFilterOptionMappingTypes(
            nextDataSetVersionId, 
            cancellationToken);


        await UpdateMappingCompleteFlags(
            nextDataSetVersionId: nextDataSetVersionId,
            locationLevelAndOptionMappingTypes: locationOptionMappingTypes,
            filterAndOptionMappingTypes: filterAndOptionMappingTypes,
            cancellationToken: cancellationToken);

        await UpdateVersionNumber(
            nextDataSetVersionId: nextDataSetVersionId,
            locationMappingTypes: locationOptionMappingTypes,
            filterMappingTypes: filterAndOptionMappingTypes,
            cancellationToken: cancellationToken);
    }

    private async Task UpdateVersionNumber(
        Guid nextDataSetVersionId,
        List<LocationMappingTypes> locationMappingTypes,
        List<FilterMappingTypes> filterMappingTypes,
        CancellationToken cancellationToken)
    {
        var sourceDataSetVersion = await publicDataDbContext
            .DataSetVersionMappings
            .Where(mapping => mapping.TargetDataSetVersionId == nextDataSetVersionId)
            .Select(nextVersion => nextVersion.SourceDataSetVersion)
            .SingleAsync(cancellationToken);

        var targetDataSetVersion = await publicDataDbContext
            .DataSetVersionMappings
            .Where(mapping => mapping.TargetDataSetVersionId == nextDataSetVersionId)
            .Select(nextVersion => nextVersion.TargetDataSetVersion)
            .SingleAsync(cancellationToken);

        var majorChangesStatus = await GetMajorChangesStatus(
            nextDataSetVersionId,
            locationMappingTypes,
            filterMappingTypes,
            cancellationToken);

        var isReplacingApiDataFile = await publicDataDbContext.DataSetVersionImports
            .AnyAsync(i => i.DataSetVersionToReplaceId != null 
                           && i.DataSetVersionToReplaceId == sourceDataSetVersion.Id,
                cancellationToken);

        if (!isReplacingApiDataFile)
        {
            UpgradeToNewMajorOrMinorVersion(
                majorChangesStatus.IsMajorVersionUpdate, 
                targetDataSetVersion, 
                sourceDataSetVersion);
            await publicDataDbContext.SaveChangesAsync(cancellationToken);
        }


        var releaseFile = await contentDbContext
            .ReleaseFiles
            .Where(rf => rf.Id == targetDataSetVersion.Release.ReleaseFileId)
            .SingleAsync(cancellationToken);

        releaseFile.PublicApiDataSetVersion = targetDataSetVersion.SemVersion();

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    private static void UpgradeToNewMajorOrMinorVersion(
        bool isMajorVersionUpdate,
        DataSetVersion targetDataSetVersion,
        DataSetVersion sourceDataSetVersion)
    {
        if (isMajorVersionUpdate)
        {
            targetDataSetVersion.VersionMajor = sourceDataSetVersion.VersionMajor + 1;
            targetDataSetVersion.VersionMinor = 0;
            targetDataSetVersion.VersionPatch = 0;
        }
        else
        {
            targetDataSetVersion.VersionMajor = sourceDataSetVersion.VersionMajor;
            targetDataSetVersion.VersionMinor = sourceDataSetVersion.VersionMinor + 1;
            targetDataSetVersion.VersionPatch = 0;
        }
    }

    public async Task<MajorChangesStatus> GetMajorChangesStatus(
        Guid dataSetVersionId, 
        List<LocationMappingTypes> locationMappingTypes,
        List<FilterMappingTypes> filterMappingTypes,
        CancellationToken cancellationToken = default)
    {
        var majorChangesStatus = new MajorChangesStatus
        {
            FiltersHaveMajorChange = filterMappingTypes
                .Any(types => NoMappingTypes.Contains(types.Filter) 
                              ||  NoMappingTypes.Contains(types.FilterOption)),
            LocationsHaveMajorChange = locationMappingTypes
                .Any(types => NoMappingTypes.Contains(types.LocationLevel) 
                              || NoMappingTypes.Contains(types.LocationOption)),
            HasDeletionChanges = await HasDeletionChanges(
                dataSetVersionId, 
                cancellationToken)
        };
        return majorChangesStatus;
    }
    /// <summary>
    /// Checks if there are any major version changes due to deletions in indicators, geographic levels, or time periods
    /// for a specific target dataset version.
    /// </summary>
    /// <param name="targetDataSetVersionId">The ID of the target dataset version to check</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>True if there are any deletion-based major version changes, false otherwise</returns>
    public async Task<bool> HasDeletionChanges(
        Guid targetDataSetVersionId, 
        CancellationToken cancellationToken = default)
    {
        return await publicDataDbContext.DataSetVersionMappings
            .Where(mapping => mapping.TargetDataSetVersionId == targetDataSetVersionId)
            .Select(mapping => mapping.HasDeletedIndicators
                               || mapping.HasDeletedGeographicLevels
                               || mapping.HasDeletedTimePeriods)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task UpdateMappingCompleteFlags(
        Guid nextDataSetVersionId,
        List<LocationMappingTypes> locationLevelAndOptionMappingTypes,
        List<FilterMappingTypes> filterAndOptionMappingTypes,
        CancellationToken cancellationToken)
    {
        // Find any location options that have a mapping type that indicates the user
        // still needs to take action in order to resolve the mapping.
        // We omit options for location levels that are mapped as `AutoNone` as these
        // means the entire location level has been deleted and cannot be mapped.
        var locationMappingsComplete = !locationLevelAndOptionMappingTypes
            .Where(types => types.LocationLevel != MappingType.AutoNone)
            .Any(types => IncompleteMappingTypes.Contains(types.LocationOption));

        // Find any filter options that that indicates the user still needs to take action
        // in order to resolve the mapping. If any exist, mappings are not yet complete.
        // We omit options for filters that are mapped as `AutoNone` as this
        // means the entire filter has been deleted and cannot be mapped.
        var filterMappingsComplete = !filterAndOptionMappingTypes
            .Where(types => types.Filter != MappingType.AutoNone)
            .Any(types => IncompleteMappingTypes.Contains(types.FilterOption));

        // Update the mapping complete flags.
        await publicDataDbContext
            .DataSetVersionMappings
            .Where(mapping => mapping.TargetDataSetVersionId == nextDataSetVersionId)
            .ExecuteUpdateAsync(
                setPropertyCalls: s => s
                    .SetProperty(mapping => mapping.LocationMappingsComplete, locationMappingsComplete)
                    .SetProperty(mapping => mapping.FilterMappingsComplete, filterMappingsComplete),
                cancellationToken: cancellationToken);
    }
    
    /// <summary>
    /// Given a batch of Location mapping update requests, this method will validate that the chosen mapping candidates
    /// exist and return a list of either success or failure responses for each candidate.
    /// </summary>
    private async Task<List<Either<ErrorViewModel, Unit>>> ValidateLocationOptionCandidates(
        Guid nextDataSetVersionId,
        BatchLocationMappingUpdatesRequest request,
        CancellationToken cancellationToken)
    {
        return await request
            .Updates
            .ToAsyncEnumerable()
            .SelectAwait(async (updateRequest, index) => updateRequest.CandidateKey is null
                ? Unit.Instance
                : await ValidateMappingCandidate(
                    nextDataSetVersionId: nextDataSetVersionId,
                    index: index,
                    jsonbColumnName: nameof(DataSetVersionMapping.LocationMappingPlan),
                    jsonPathSegments:
                    [
                        nameof(DataSetVersionMapping.LocationMappingPlan.Levels),
                        updateRequest.Level!.Value.ToString(),
                        nameof(LocationLevelMappings.Candidates)
                    ],
                    updateRequest.CandidateKey,
                    cancellationToken))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Given a batch of Location mapping update requests, this method will return a list of either success or failure
    /// responses for each update.
    /// </summary>
    private async Task<List<Either<ErrorViewModel, LocationMappingUpdateResponseViewModel>>>
        UpdateLocationOptionMappingsBatch(
            Guid nextDataSetVersionId,
            BatchLocationMappingUpdatesRequest request,
            CancellationToken cancellationToken)
    {
        return await request
            .Updates
            .ToAsyncEnumerable()
            .SelectAwait(async (updateRequest, index) => await UpdateMapping
            <LocationOptionMapping, MappableLocationOption, LocationMappingUpdateRequest,
                LocationMappingUpdateResponseViewModel>(
                nextDataSetVersionId: nextDataSetVersionId,
                updateRequest: updateRequest,
                index: index,
                jsonbColumnName: nameof(DataSetVersionMapping.LocationMappingPlan),
                jsonPathSegments:
                [
                    nameof(DataSetVersionMapping.LocationMappingPlan.Levels),
                    updateRequest.Level!.Value.ToString(),
                    nameof(LocationLevelMappings.Mappings),
                    updateRequest.SourceKey
                ],
                createSuccessfulResponseFn: mappingUpdate => new LocationMappingUpdateResponseViewModel
                {
                    Level = updateRequest.Level!.Value,
                    SourceKey = updateRequest.SourceKey,
                    Mapping = mappingUpdate
                },
                cancellationToken))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Given a batch of filter option mapping update requests, this method will validate that the chosen mapping
    /// candidates exist and return a list of either success or failure responses for each candidate.
    /// </summary>
    private async Task<List<Either<ErrorViewModel, Unit>>> ValidateFilterOptionCandidates(
        Guid nextDataSetVersionId,
        BatchFilterOptionMappingUpdatesRequest request,
        CancellationToken cancellationToken)
    {
        var filterMappings = await GetFilterCandidateMappings(
            nextDataSetVersionId: nextDataSetVersionId,
            request: request,
            cancellationToken: cancellationToken);

        return await request
            .Updates
            .ToAsyncEnumerable()
            .SelectAwait(async (updateRequest, index) =>
            {
                var candidateFilterForOwningFilter = filterMappings[updateRequest.FilterKey];

                if (candidateFilterForOwningFilter is null)
                {
                    return new ErrorViewModel
                    {
                        Code = ValidationMessages.OwningFilterNotMapped.Code,
                        Message = ValidationMessages.OwningFilterNotMapped.Message,
                        Path =
                            $"{nameof(BatchMappingUpdatesRequest<FilterOptionMappingUpdateRequest>.Updates).ToLowerFirst()}[{index}]." +
                            $"{nameof(FilterOptionMappingUpdateRequest.FilterKey).ToLowerFirst()}"
                    };
                }

                return updateRequest.CandidateKey is null
                    ? Unit.Instance
                    : await ValidateMappingCandidate(
                        nextDataSetVersionId: nextDataSetVersionId,
                        index: index,
                        jsonbColumnName: nameof(DataSetVersionMapping.FilterMappingPlan),
                        jsonPathSegments:
                        [
                            nameof(DataSetVersionMapping.FilterMappingPlan.Candidates),
                            candidateFilterForOwningFilter,
                            nameof(FilterMappingCandidate.Options)
                        ],
                        updateRequest.CandidateKey,
                        cancellationToken);
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// This method finds the mappings between top-level source filters and their candidate filters.
    /// If a filter has not been mapped, it will return a null "candidate" string for that filter.
    /// </summary>
    private async Task<Dictionary<string, string?>> GetFilterCandidateMappings(
        Guid nextDataSetVersionId,
        BatchFilterOptionMappingUpdatesRequest request,
        CancellationToken cancellationToken)
    {
        var filterKeys = request
            .Updates
            .Select(update => update.FilterKey)
            .Distinct();

        return await filterKeys
            .ToAsyncEnumerable()
            .ToDictionaryAwaitAsync(
                keySelector: ValueTask.FromResult,
                elementSelector: async filterKey =>
                {
                    var candidateKeyPath = new JsonbPathRequest<Guid>
                    {
                        TableName = nameof(PublicDataDbContext.DataSetVersionMappings),
                        IdColumnName = nameof(DataSetVersionMapping.TargetDataSetVersionId),
                        JsonbColumnName = nameof(DataSetVersionMapping.FilterMappingPlan),
                        RowId = nextDataSetVersionId,
                        PathSegments =
                        [
                            nameof(FilterMappingPlan.Mappings),
                            filterKey,
                            nameof(FilterMapping.CandidateKey)
                        ]
                    };

                    return await postgreSqlRepository
                        .GetJsonbFromPath<PublicDataDbContext, Guid, string>(
                            context: publicDataDbContext,
                            request: candidateKeyPath,
                            cancellationToken: cancellationToken);
                },
                cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Given a batch of Filter Option mapping update requests, this method will return a list of either success or
    /// failure responses for each update.
    /// </summary>
    private async Task<List<Either<ErrorViewModel, FilterOptionMappingUpdateResponseViewModel>>>
        UpdateFilterOptionMappingsBatch(
            Guid nextDataSetVersionId,
            BatchFilterOptionMappingUpdatesRequest request,
            CancellationToken cancellationToken)
    {
        return await request
            .Updates
            .ToAsyncEnumerable()
            .SelectAwait(async (updateRequest, index) => await UpdateMapping
            <FilterOptionMapping, MappableFilterOption, FilterOptionMappingUpdateRequest,
                FilterOptionMappingUpdateResponseViewModel>(
                nextDataSetVersionId: nextDataSetVersionId,
                updateRequest: updateRequest,
                index: index,
                jsonbColumnName: nameof(DataSetVersionMapping.FilterMappingPlan),
                jsonPathSegments:
                [
                    nameof(DataSetVersionMapping.FilterMappingPlan.Mappings),
                    updateRequest.FilterKey,
                    nameof(FilterMapping.OptionMappings),
                    updateRequest.SourceKey
                ],
                createSuccessfulResponseFn: mappingUpdate => new FilterOptionMappingUpdateResponseViewModel
                {
                    FilterKey = updateRequest.FilterKey,
                    SourceKey = updateRequest.SourceKey,
                    Mapping = mappingUpdate
                },
                cancellationToken))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Given a mapping update request, this method will return either a success or failure response.
    /// </summary>
    private async Task<Either<ErrorViewModel, Unit>>
        ValidateMappingCandidate(
            Guid nextDataSetVersionId,
            int index,
            string jsonbColumnName,
            string[] jsonPathSegments,
            string candidateKey,
            CancellationToken cancellationToken)
    {
        var jsonRequest = new JsonbPathRequest<Guid>
        {
            TableName = nameof(PublicDataDbContext.DataSetVersionMappings),
            IdColumnName = nameof(DataSetVersionMapping.TargetDataSetVersionId),
            JsonbColumnName = jsonbColumnName,
            RowId = nextDataSetVersionId,
            PathSegments = jsonPathSegments
        };

        var candidateExists = await postgreSqlRepository
            .KeyExistsAtJsonbPath(
                publicDataDbContext,
                jsonRequest,
                candidateKey,
                cancellationToken: cancellationToken);

        return candidateExists
            ? Unit.Instance
            : new ErrorViewModel
            {
                Code = ValidationMessages.DataSetVersionMappingCandidatePathDoesNotExist.Code,
                Message = ValidationMessages.DataSetVersionMappingCandidatePathDoesNotExist.Message,
                Path =
                    $"{nameof(BatchMappingUpdatesRequest<LocationMappingUpdateRequest>.Updates).ToLowerFirst()}[{index}]." +
                    $"{nameof(MappingUpdateRequest.CandidateKey).ToLowerFirst()}"
            };
    }

    /// <summary>
    /// Given a mapping update request, this method will return either a success or failure response.
    /// </summary>
    private async Task<Either<ErrorViewModel, TMappingUpdateResponse>>
        UpdateMapping<TMapping, TMappableElement, TMappingUpdateRequest, TMappingUpdateResponse>(
            Guid nextDataSetVersionId,
            TMappingUpdateRequest updateRequest,
            int index,
            string jsonbColumnName,
            string[] jsonPathSegments,
            Func<TMapping, TMappingUpdateResponse> createSuccessfulResponseFn,
            CancellationToken cancellationToken)
        where TMapping : Mapping<TMappableElement>
        where TMappableElement : MappableElement
        where TMappingUpdateRequest : MappingUpdateRequest
        where TMappingUpdateResponse : MappingUpdateResponseViewModel<TMapping, TMappableElement>
    {
        var updateJsonRequest = new JsonbPathRequest<Guid>
        {
            TableName = nameof(PublicDataDbContext.DataSetVersionMappings),
            IdColumnName = nameof(DataSetVersionMapping.TargetDataSetVersionId),
            JsonbColumnName = jsonbColumnName,
            RowId = nextDataSetVersionId,
            PathSegments = jsonPathSegments
        };

        return await postgreSqlRepository
            .UpdateJsonbAtPath(
                publicDataDbContext,
                updateJsonRequest,
                (TMapping? mapping) => Task.FromResult(mapping is not null
                    ? mapping with
                    {
                        Type = updateRequest.Type!.Value,
                        CandidateKey = updateRequest.CandidateKey
                    }
                    : new Either<ErrorViewModel, TMapping?>(new ErrorViewModel
                    {
                        Code = ValidationMessages.DataSetVersionMappingSourcePathDoesNotExist.Code,
                        Message = ValidationMessages.DataSetVersionMappingSourcePathDoesNotExist.Message,
                        Path =
                            $"{nameof(BatchMappingUpdatesRequest<TMappingUpdateRequest>.Updates).ToLowerFirst()}[{index}]." +
                            $"{nameof(MappingUpdateRequest.SourceKey).ToLowerFirst()}"
                    })),
                cancellationToken: cancellationToken)!
            .OnSuccess(createSuccessfulResponseFn);
    }

    private async Task<Either<ActionResult, DataSetVersionMapping>> CheckMappingExists(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .DataSetVersionMappings
            .AsNoTracking()
            .SingleOrNotFoundAsync(
                mapping => mapping.TargetDataSetVersionId == nextDataSetVersionId,
                cancellationToken);
    }
}
