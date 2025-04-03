using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IDataSetVersionService
{
    Task<Either<ActionResult, Guid>> CreateInitialVersion(
        Guid dataSetId,
        Guid releaseFileId,
        Guid instanceId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Guid>> CreateNextVersion(
        Guid dataSetId,
        Guid releaseFileId,
        Guid instanceId,
        PatchVersionConfigs? patchVersionConfig,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Unit>> BulkDeleteVersions(
        Guid releaseVersionId,
        bool forceDeleteAll = false,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Unit>> DeleteVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default);
}
