#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.RelatedInformation.Dtos;

public record RelatedInformationDto
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Url { get; init; }

    public static RelatedInformationDto FromModel(Link link) =>
        new()
        {
            Id = link.Id,
            Title = link.Description,
            Url = link.Url,
        };
}
