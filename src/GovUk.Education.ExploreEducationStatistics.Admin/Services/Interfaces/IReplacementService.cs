#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReplacementService
{
    Task<Either<ActionResult, Unit>> Replace(
        Guid releaseVersionId,
        Guid originalFileId,
        CancellationToken cancellationToken = default
    );
}
