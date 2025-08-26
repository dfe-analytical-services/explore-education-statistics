#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation.Dtos;

public record RelatedInformationDto
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Url { get; init; }

    public static RelatedInformationDto FromLink(Link link) =>
        new()
        {
            Id = link.Id,
            Title = link.Description,
            Url = link.Url
        };
}
