using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;

public record PublicationInfoDto
{
    public Guid? PublicationId { get; init; }
    public string? PublicationSlug { get; init; }
    public ReleaseInfoDto? LatestPublishedRelease { get; init; }

    public PublicationInfo ToModel() =>
        IsValid
            ? new PublicationInfo
            {
                PublicationSlug = PublicationSlug,
                LatestReleaseSlug = LatestPublishedRelease.ReleaseSlug!,
            }
            : throw new Exception(
                $"Invalid PublicationInfo. Data is missing. Has the ContentAPI contract changed? Dto: {this}"
            );

    [MemberNotNullWhen(returnValue: true, nameof(PublicationSlug))]
    [MemberNotNullWhen(returnValue: true, nameof(LatestPublishedRelease))]
    private bool IsValid =>
        !string.IsNullOrEmpty(PublicationSlug)
        && LatestPublishedRelease != null
        && !string.IsNullOrEmpty(LatestPublishedRelease.ReleaseSlug);
}
