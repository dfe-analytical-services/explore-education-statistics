using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public interface IReleaseSlugValidator
{
    Task<Either<ActionResult, Unit>> ValidateNewSlug(
        string newReleaseSlug,
        Guid publicationId,
        Guid? releaseId = null,
        CancellationToken cancellationToken = default);
}
