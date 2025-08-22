#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.RelatedInformation.Dtos;

public record GetRelatedInformationForReleaseResponse
{
    public required RelatedInformationDto[] RelatedInformation { get; init; }

    public static GetRelatedInformationForReleaseResponse From(
        RelatedInformationDto[] relatedInformation)
    {
        return new GetRelatedInformationForReleaseResponse { RelatedInformation = relatedInformation };
    }
}
