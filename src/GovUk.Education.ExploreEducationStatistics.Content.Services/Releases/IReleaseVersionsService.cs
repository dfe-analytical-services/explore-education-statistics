using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;

public interface IReleaseVersionsService
{
    Task<Either<ActionResult, ReleaseVersionSummaryDto>> GetReleaseVersionSummary(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default);
}
