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
            applyUpdatesFn: () => UpdateLocationOptionMappingsBatch(nextDataSetVersionId, request, cancellationToken),
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
    
    public Task<Either<ActionResult, BatchFilterOptionMappingUpdatesResponseViewModel>> ApplyBatchFilterOptionMappingUpdates(
        Guid nextDataSetVersionId,
        BatchFilterOptionMappingUpdatesRequest request,
        CancellationToken cancellationToken = default)
    {
        return ApplyBatchMappingUpdates(
            nextDataSetVersionId: nextDataSetVersionId,
            applyUpdatesFn: () => UpdateFilterOptionMappingsBatch(nextDataSetVersionId, request, cancellationToken),
            createViewModelFn: updates => new BatchFilterOptionMappingUpdatesResponseViewModel { Updates = updates },
            cancellationToken: cancellationToken);
    }

    private async Task<Either<ActionResult, TBatchMappingUpdatesResponseViewModel>> 
        ApplyBatchMappingUpdates<TMappingUpdateResponse, TBatchMappingUpdatesResponseViewModel>(
        Guid nextDataSetVersionId,
        Func<Task<List<Either<ErrorViewModel, TMappingUpdateResponse>>>> applyUpdatesFn,
        Func<List<TMappingUpdateResponse>, TBatchMappingUpdatesResponseViewModel> createViewModelFn,
        CancellationToken cancellationToken = default)
    {
        return await publicDataDbContext.RequireTransaction(async () =>
            await userService
                .CheckIsBauUser()
                .OnSuccess(() => CheckMappingExists(nextDataSetVersionId, cancellationToken))
                .OnSuccess(_ => applyUpdatesFn())
                .OnSuccess(updateSuccessesAndFailures =>
                {
                    // Take all the update results from this batch of updates, and check to make sure they all
                    // succeeded.  If any have failed, we fail the entire transaction and inform the front end of the
                    // particular update requests that failed. 
                    var aggregatedResult = updateSuccessesAndFailures.AggregateSuccessesAndFailures();

                    return aggregatedResult
                        .OnSuccess(createViewModelFn)
                        .OnFailure<ActionResult>(errors => ValidationUtils.ValidationResult(errors));
                }));
    }

    /// <summary>
    /// Given a batch of Location mapping update requests, this method will return a list of either success or failure
    /// responses for each update.
    /// </summary>
    private async Task<List<Either<ErrorViewModel, LocationMappingUpdateResponseViewModel>>> UpdateLocationOptionMappingsBatch(
        Guid nextDataSetVersionId,
        BatchLocationMappingUpdatesRequest request,
        CancellationToken cancellationToken)
    {
        return await request
            .Updates
            .ToAsyncEnumerable()
            .SelectAwait(async (updateRequest, index) => await UpdateMapping
                <LocationOptionMapping, MappableLocationOption, LocationMappingUpdateRequest, LocationMappingUpdateResponseViewModel>(
                    nextDataSetVersionId: nextDataSetVersionId,
                    updateRequest: updateRequest,
                    index: index,
                    jsonbColumnName: nameof(DataSetVersionMapping.LocationMappingPlan),
                    jsonPathSegments: [
                        nameof(DataSetVersionMapping.LocationMappingPlan.Levels),
                        updateRequest.Level.ToString(),
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
    /// Given a batch of Filter Option mapping update requests, this method will return a list of either success or
    /// failure responses for each update.
    /// </summary>
    private async Task<List<Either<ErrorViewModel, FilterOptionMappingUpdateResponseViewModel>>> UpdateFilterOptionMappingsBatch(
        Guid nextDataSetVersionId,
        BatchFilterOptionMappingUpdatesRequest request,
        CancellationToken cancellationToken)
    {
        return await request
            .Updates
            .ToAsyncEnumerable()
            .SelectAwait(async (updateRequest, index) => await UpdateMapping
                <FilterOptionMapping, MappableFilterOption, FilterOptionMappingUpdateRequest, FilterOptionMappingUpdateResponseViewModel>(
                    nextDataSetVersionId: nextDataSetVersionId,
                    updateRequest: updateRequest,
                    index: index,
                    jsonbColumnName: nameof(DataSetVersionMapping.FilterMappingPlan),
                    jsonPathSegments: [
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
        where TMappingUpdateRequest: MappingUpdateRequest
        where TMappingUpdateResponse: MappingUpdateResponseViewModel<TMapping, TMappableElement>
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
                (TMapping mapping) => Task.FromResult(mapping is not null
                    ? mapping with
                    {
                        Type = updateRequest.Type!.Value,
                        CandidateKey = updateRequest.CandidateKey
                    }
                    : new Either<ErrorViewModel, TMapping>(new ErrorViewModel
                    {
                        Code = ValidationMessages.DataSetVersionMappingPathDoesNotExist.Code,
                        Message = ValidationMessages.DataSetVersionMappingPathDoesNotExist.Message,
                        Path =
                            $"{nameof(BatchMappingUpdatesRequest<TMappingUpdateRequest>.Updates).ToLowerFirst()}[{index}]." +
                            $"{nameof(MappingUpdateRequest.SourceKey).ToLowerFirst()}"
                    })),
                cancellationToken: cancellationToken)
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
