#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IPublicDataApiClient
{
    Task<Either<ActionResult, HttpResponseMessage>> GetDataSetVersionChanges(
        Guid dataSetId,
        string dataSetVersion,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, DataSetQueryPaginatedResultsViewModel>> RunQuery(
        Guid dataSetId,
        string dataSetVersion,
        string queryBody,
        CancellationToken cancellationToken = default
    );
}
