using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Semver;

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
        SemVersion? dataSetVersionToReplace = null,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Unit>> BulkDeleteVersions(
        Guid releaseVersionId,
        bool forceDeleteAll = false,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Unit>> DeleteVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default);
}
