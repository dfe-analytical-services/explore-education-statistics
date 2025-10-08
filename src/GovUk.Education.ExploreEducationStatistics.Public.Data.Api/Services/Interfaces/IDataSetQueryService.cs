using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IDataSetQueryService
{
    Task<Either<ActionResult, DataSetQueryPaginatedResultsViewModel>> Query(
        Guid dataSetId,
        DataSetGetQueryRequest request,
        string? dataSetVersion = null,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, DataSetQueryPaginatedResultsViewModel>> Query(
        Guid dataSetId,
        DataSetQueryRequest request,
        string? dataSetVersion,
        CancellationToken cancellationToken = default
    );
}
