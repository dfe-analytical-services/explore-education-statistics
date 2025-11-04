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
            YearTitle = YearTitle,
            CoverageTitle = CoverageTitle,
            Published = Published,
            Type = Type,
            IsLatestRelease = LatestRelease,
            PublicationId = Publication?.Id,
            PublicationTitle = Publication?.Title,
            PublicationSlug = Publication?.Slug,
        };
}

public record ReleaseSummaryPublicationDto
{
    public string? Id { get; init; }
    public string? Title { get; init; }
    public string? Slug { get; init; }
}
