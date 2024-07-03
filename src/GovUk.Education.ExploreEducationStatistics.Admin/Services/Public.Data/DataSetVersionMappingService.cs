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

    public async Task<Either<ActionResult, BatchLocationMappingUpdatesResponseViewModel>> ApplyBatchMappingUpdates(
        Guid nextDataSetVersionId,
        BatchLocationMappingUpdatesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await publicDataDbContext.RequireTransaction(async () =>
            await userService
                .CheckIsBauUser()
                .OnSuccess(() => CheckMappingExists(nextDataSetVersionId, cancellationToken))
                .OnSuccess(_ => UpdateLocationOptionMappingsBatch(nextDataSetVersionId, request, cancellationToken))
                .OnSuccess(updateSuccessesAndFailures =>
                {
                    // Take all the update results from this batch of updates, and check to make sure they all
                    // succeeded.  If any have failed, we fail the entire transaction and inform the front end of the
                    // particular update requests that failed. 
                    var aggregatedResult = updateSuccessesAndFailures.AggregateSuccessesAndFailures();

                    return aggregatedResult
                        .OnSuccess(updates =>
                            new BatchLocationMappingUpdatesResponseViewModel { Updates = updates })
                        .OnFailure<ActionResult>(errors => ValidationUtils.ValidationResult(errors));
                }));
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
            .SelectAwait(async (updateRequest, index) => await UpdateLocationOptionMapping(
                nextDataSetVersionId,
                updateRequest,
                index,
                cancellationToken))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Given a Location mapping update request, this method will return either a success or failure response.
    /// </summary>
    private async Task<Either<ErrorViewModel, LocationMappingUpdateResponseViewModel>> UpdateLocationOptionMapping(
        Guid nextDataSetVersionId,
        LocationMappingUpdateRequest updateRequest,
        int index,
        CancellationToken cancellationToken = default)
    {
        var updateJsonRequest = new JsonbPathRequest<Guid>
        {
            TableName = nameof(PublicDataDbContext.DataSetVersionMappings),
            IdColumnName = nameof(DataSetVersionMapping.TargetDataSetVersionId),
            JsonbColumnName = nameof(DataSetVersionMapping.LocationMappingPlan),
            RowId = nextDataSetVersionId,
            PathSegments =
            [
                "Levels",
                updateRequest.Level.ToString(),
                "Mappings",
                updateRequest.SourceKey
            ]
        };

        return await postgreSqlRepository
            .UpdateJsonbAtPath(
                publicDataDbContext,
                updateJsonRequest, (LocationOptionMapping mapping) => Task.FromResult(
                    mapping is not null 
                        ? mapping with
                        {
                            Type = updateRequest.Type!.Value,
                            CandidateKey = updateRequest.CandidateKey
                        }
                        : new Either<ErrorViewModel, LocationOptionMapping>(new ErrorViewModel
                        {
                            Code = ValidationMessages.DataSetVersionMappingPathDoesNotExist.Code,
                            Message = ValidationMessages.DataSetVersionMappingPathDoesNotExist.Message,
                            Path =
                                $"{nameof(BatchLocationMappingUpdatesRequest.Updates).ToLowerFirst()}[{index}]." +
                                $"{nameof(LocationMappingUpdateRequest.SourceKey).ToLowerFirst()}"
                        })),
                cancellationToken: cancellationToken)
            .OnSuccess(mappingUpdate => new LocationMappingUpdateResponseViewModel
            {
                Level = updateRequest.Level!.Value,
                SourceKey = updateRequest.SourceKey!,
                Mapping = mappingUpdate
            });
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
