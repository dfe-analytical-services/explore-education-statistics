using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IDataSetService
{
    Task<Either<ActionResult, DataSetViewModel>> GetDataSet(
        Guid dataSetId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, FileStreamResult>> DownloadDataSet(
        Guid dataSetId,
        string? dataSetVersion,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, DataSetPaginatedListViewModel>> ListDataSets(
        int page,
        int pageSize,
        Guid publicationId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, DataSetVersionViewModel>> GetVersion(
        Guid dataSetId,
        string dataSetVersion,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, DataSetVersionPaginatedListViewModel>> ListVersions(
        Guid dataSetId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, DataSetMetaViewModel>> GetMeta(
        Guid dataSetId,
        string? dataSetVersion = null,
        IReadOnlySet<DataSetMetaType>? types = null,
        CancellationToken cancellationToken = default
    );
}
