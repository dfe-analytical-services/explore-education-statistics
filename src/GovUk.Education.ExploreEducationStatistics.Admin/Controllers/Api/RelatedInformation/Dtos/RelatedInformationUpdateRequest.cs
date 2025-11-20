#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RelatedInformation.Dtos;

public record RelatedInformationUpdateRequest
{
    public required string Title { get; init; }

    public required string Url { get; init; }
}
