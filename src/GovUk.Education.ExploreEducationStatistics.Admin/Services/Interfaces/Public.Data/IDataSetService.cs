#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IDataSetService
{
    Task<Either<ActionResult, PaginatedListViewModel<DataSetSummaryViewModel>>> ListDataSets(
        int page,
        int pageSize,
        Guid publicationId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, DataSetViewModel>> GetDataSet(
        Guid dataSetId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, DataSetViewModel>> CreateDataSet(
        Guid releaseFileId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, bool>> HasDraftVersion(Guid dataSetId, CancellationToken cancellationToken = default);
}
