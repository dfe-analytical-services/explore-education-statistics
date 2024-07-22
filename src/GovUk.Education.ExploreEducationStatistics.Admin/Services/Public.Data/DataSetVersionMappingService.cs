#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ErrorViewModel = GovUk.Education.ExploreEducationStatistics.Common.ViewModels.ErrorViewModel;
using ValidationUtils = GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public class DataSetVersionMappingService(
    IPostgreSqlRepository postgreSqlRepository,
    IUserService userService,
    PublicDataDbContext publicDataDbContext)
    : IDataSetVersionMappingService
{
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
            updateMappingsCompleteFn: () => UpdateLocationMappingsComplete(nextDataSetVersionId, cancellationToken),
            createViewModelFn: updates => new BatchLocationMappingUpdatesResponseViewModel { Updates = updates },
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
            updateMappingsCompleteFn: () => UpdateFilterMappingsComplete(nextDataSetVersionId, cancellationToken),
            createViewModelFn: updates => new BatchFilterOptionMappingUpdatesResponseViewModel { Updates = updates },
            cancellationToken: cancellationToken);
    }

    private async Task<Either<ActionResult, TBatchMappingUpdatesResponseViewModel>>
        ApplyBatchMappingUpdates<TMappingUpdateResponse, TBatchMappingUpdatesResponseViewModel, TMappingCandidate>(
            Guid nextDataSetVersionId,
            Func<Task<List<Either<ErrorViewModel, TMappingCandidate>>>> validateCandidatesFn,
            Func<Task<List<Either<ErrorViewModel, TMappingUpdateResponse>>>> applyUpdatesFn,
            Func<Task> updateMappingsCompleteFn,
            Func<List<TMappingUpdateResponse>, TBatchMappingUpdatesResponseViewModel> createViewModelFn,
            CancellationToken cancellationToken = default)
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
                .OnSuccessDo(updateMappingsCompleteFn)
                .OnSuccess(createViewModelFn));
    }

    private async Task UpdateLocationMappingsComplete(Guid nextDataSetVersionId, CancellationToken cancellationToken)
    {
        var mapping = await publicDataDbContext
            .DataSetVersionMappings
            .SingleAsync(
                mapping => mapping.TargetDataSetVersionId == nextDataSetVersionId,
                cancellationToken);

        mapping.LocationMappingsComplete = await LocationMappingsComplete(nextDataSetVersionId, cancellationToken);

        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

#pragma warning disable EF1002
    private async Task<bool> LocationMappingsComplete(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken)
    {
        var targetDataSetVersionIdParam = new NpgsqlParameter("targetDataSetVersionId", nextDataSetVersionId);

        // Query to find any mappings under any levels that have a mapping type of "None" or "AutoNone".
        // This query iterates through each top-level LocationLevelMappings in the LocationMappingPlan and
        // for each of the Level's LocationOptionMappings, checks to see if any exist with mapping type
        // "None" or "AutoNone".
        //
        // If any exist, mappings are not yet complete.
        var unmappedLocationOptionsExist = await publicDataDbContext
            .Set<JsonInt>()
            .FromSqlRaw(sql:
                $"""
                 SELECT 1 "{nameof(JsonInt.IntValue)}" FROM (
                     SELECT OptionMappingType FROM "{nameof(PublicDataDbContext.DataSetVersionMappings)}" Mapping,
                     jsonb_each(Mapping."{nameof(DataSetVersionMapping.LocationMappingPlan)}" -> '{nameof(LocationMappingPlan.Levels)}') Level,
                     jsonb_each(Level.value -> '{nameof(LocationLevelMappings.Mappings)}') OptionMapping,
                     jsonb_extract_path_text(OptionMapping.value, '{nameof(LocationOptionMapping.Type)}') OptionMappingType
                     WHERE "{nameof(DataSetVersionMapping.TargetDataSetVersionId)}" = @targetDataSetVersionId
                 ) 
                 WHERE OptionMappingType IN ('{nameof(MappingType.None)}','{nameof(MappingType.AutoNone)}')
                 LIMIT 1
                 """,
                parameters: [targetDataSetVersionIdParam])
            .SingleOrDefaultAsync(cancellationToken);

        return unmappedLocationOptionsExist is null;
    }
#pragma warning restore EF1002

    private async Task UpdateFilterMappingsComplete(Guid nextDataSetVersionId, CancellationToken cancellationToken)
    {
        var mapping = await GetDataSetVersionMapping(nextDataSetVersionId, cancellationToken);

        mapping.FilterMappingsComplete = await FilterMappingsComplete(nextDataSetVersionId, cancellationToken);

        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<DataSetVersionMapping> GetDataSetVersionMapping(Guid nextDataSetVersionId, CancellationToken cancellationToken) =>
        await publicDataDbContext
            .DataSetVersionMappings
            .SingleAsync(
                mapping => mapping.TargetDataSetVersionId == nextDataSetVersionId,
                cancellationToken);

#pragma warning disable EF1002
    private async Task<bool> FilterMappingsComplete(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken)
    {
        var targetDataSetVersionIdParam = new NpgsqlParameter("targetDataSetVersionId", nextDataSetVersionId);

        // Query to find any filter options under any filters that have a mapping type of "None" or "AutoNone".
        // The query iterates through each top-level FilterMapping in the FilterMappingPlan, then iterates through
        // each one's set of FilterOptionMappings, and checks to see if any exist with mapping type "None" or
        // "AutoNone".
        //
        // If any exist, mappings are not yet complete.
        //
        // Notably we omit checking filters that have a mapping of AutoNone, as currently there is no way 
        // within the UI for the users to handle the resolving of these unmapped filters.
        var unmappedFilterOptionsExist = await publicDataDbContext
            .Set<JsonInt>()
            .FromSqlRaw(sql:
                $"""
                 SELECT 1 "{nameof(JsonInt.IntValue)}" FROM (
                     SELECT FilterMappingType, OptionMappingType FROM "{nameof(PublicDataDbContext.DataSetVersionMappings)}" Mapping,
                     jsonb_each(Mapping."{nameof(DataSetVersionMapping.FilterMappingPlan)}" -> 'Mappings') FilterMapping,
                     jsonb_each(FilterMapping.value -> '{nameof(FilterMapping.OptionMappings)}') OptionMapping,
                     jsonb_extract_path_text(FilterMapping.value, '{nameof(FilterMapping.Type)}') FilterMappingType,
                     jsonb_extract_path_text(OptionMapping.value, '{nameof(FilterOptionMapping.Type)}') OptionMappingType
                     WHERE "{nameof(DataSetVersionMapping.TargetDataSetVersionId)}" = @targetDataSetVersionId
                 ) 
                 WHERE FilterMappingType != '{nameof(MappingType.AutoNone)}' 
                 AND OptionMappingType IN ('{nameof(MappingType.None)}','{nameof(MappingType.AutoNone)}')
                 LIMIT 1
                 """,
                parameters: [targetDataSetVersionIdParam])
            .SingleOrDefaultAsync(cancellationToken);
        
        return unmappedFilterOptionsExist is null;
    }
#pragma warning restore EF1002

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
            CancellationToken cancellationToken = default)
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
            CancellationToken cancellationToken = default)
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
