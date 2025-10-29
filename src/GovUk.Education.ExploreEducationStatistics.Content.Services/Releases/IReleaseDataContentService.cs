using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;

public interface IReleaseDataContentService
{
    Task<Either<ActionResult, ReleaseDataContentDto>> GetReleaseDataContent(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    );
}
