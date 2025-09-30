#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IDataSetVersionService
{
    Task<
        Either<ActionResult, PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>>
    > ListLiveVersions(
        Guid dataSetId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, DataSetVersionInfoViewModel>> GetDataSetVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, DataSetVersion>> GetDataSetVersion(
        Guid dataSetId,
        SemVersion version,
        CancellationToken cancellationToken = default
    );

    Task<List<DataSetVersionStatusSummary>> GetStatusesForReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, Unit>> DeleteVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, DataSetVersionSummaryViewModel>> CreateNextVersion(
        Guid releaseFileId,
        Guid dataSetId,
        Guid? dataSetVersionToReplaceId = null,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, DataSetVersionSummaryViewModel>> CompleteNextVersionImport(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, HttpResponseMessage>> GetVersionChanges(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, DataSetDraftVersionViewModel>> UpdateVersion(
        Guid dataSetVersionId,
        DataSetVersionUpdateRequest updateRequest,
        CancellationToken cancellationToken = default
    );

    Task UpdateVersionsForReleaseVersion(
        Guid releaseVersionId,
        string releaseSlug,
        string releaseTitle,
        CancellationToken cancellationToken = default
    );
}
