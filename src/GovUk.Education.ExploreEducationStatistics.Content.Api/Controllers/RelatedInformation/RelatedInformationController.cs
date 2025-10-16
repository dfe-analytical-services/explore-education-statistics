#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation;
using GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.RelatedInformation;

[Route("api")]
[ApiController]
public class RelatedInformationController(IRelatedInformationService relatedInformationService) : ControllerBase
{
    [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/related-information")]
    public Task<Results<Ok<RelatedInformationDto[]>, ProblemHttpResult>> GetRelatedInformationForRelease(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    ) =>
        relatedInformationService
            .GetRelatedInformationForRelease(
                publicationSlug: publicationSlug,
                releaseSlug: releaseSlug,
                cancellationToken
            )
            .ToOkHttpResult<RelatedInformationDto[]>();
}
