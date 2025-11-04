using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation;

public interface IRelatedInformationService
{
    Task<Either<ActionResult, RelatedInformationDto[]>> GetRelatedInformationForRelease(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    );
}
