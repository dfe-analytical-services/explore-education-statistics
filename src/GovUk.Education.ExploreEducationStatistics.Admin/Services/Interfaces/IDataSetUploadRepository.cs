#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataSetUploadRepository
{
    Task<Either<ActionResult, List<DataSetUploadViewModel>>> ListAll(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Unit>> Delete(
        Guid releaseVersionId,
        Guid dataSetUploadId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Unit>> DeleteAll(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default);
}
