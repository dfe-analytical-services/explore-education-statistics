using GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases;

public interface IReleaseDataContentService
{
    Task<Either<ActionResult, ReleaseDataContentDto>> GetReleaseDataContent(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    );
}
