using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;

public record ReleaseSummaryDto
{
    public string? Id { get; init; }
    public string? ReleaseId { get; init; }
    public string? Title { get; init; }
    public string? Slug { get; init; }
    public string? YearTitle { get; init; }
    public string? CoverageTitle { get; init; }
    public DateTimeOffset? Published { get; init; }
    public string? Type { get; init; }
    public bool? LatestRelease { get; init; }
    public ReleaseSummaryPublicationDto? Publication { get; init; }

    public ReleaseSummary ToModel() =>
        new()
        {
            Id = Id.ThrowIfBlank(nameof(Id)),
            ReleaseId = ReleaseId.ThrowIfBlank(nameof(ReleaseId)),
            Title = Title.ThrowIfBlank(nameof(Title)),
            Slug = Slug.ThrowIfBlank(nameof(Slug)),
            YearTitle = YearTitle.ThrowIfBlank(nameof(YearTitle)),
            CoverageTitle = CoverageTitle.ThrowIfBlank(nameof(CoverageTitle)),
            Published = Published.ThrowIfBlank(nameof(Published)),
            Type = Type.ThrowIfBlank(nameof(Type)),
            LatestRelease = LatestRelease.ThrowIfBlank(nameof(LatestRelease)),
            PublicationId = Publication.ThrowIfBlank(nameof(Publication)).Id.ThrowIfBlank("Publication Id"),
            PublicationTitle = Publication.ThrowIfBlank(nameof(Publication)).Title.ThrowIfBlank("Publication Title"),
            PublicationSlug = Publication.ThrowIfBlank(nameof(Publication)).Slug.ThrowIfBlank("Publication Slug")
        };
}

public record ReleaseSummaryPublicationDto
{
    public string? Id { get; init; }
    public string? Title { get; init; }
    public string? Slug { get; init; }
}
