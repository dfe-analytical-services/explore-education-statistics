#nullable enable
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.RelatedInformation.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.RelatedInformation;

[Route("api")]
[ApiController]
public class RelatedInformationController(IRelatedInformationService relatedInformationService) : ControllerBase
{
    [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/related-information")]
    public Task<Results<Ok<GetRelatedInformationForReleaseResponse>, ProblemHttpResult>>
        GetRelatedInformationForRelease(
            string publicationSlug,
            string releaseSlug,
            CancellationToken cancellationToken = default)
    {
        return relatedInformationService.GetRelatedInformationForRelease(
                publicationSlug: publicationSlug,
                releaseSlug: releaseSlug,
                cancellationToken: cancellationToken)
            .Map(GetRelatedInformationForReleaseResponse.From)
            .ToOkHttpResult<GetRelatedInformationForReleaseResponse>();
    }
}
