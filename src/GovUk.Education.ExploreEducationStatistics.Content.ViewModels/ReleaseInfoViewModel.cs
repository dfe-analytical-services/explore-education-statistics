namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

/// <summary>
/// Response type used only by the Content.Search.FunctionApp (Search Docs Function App)
/// via the <c>GET /api/publicationInfos</c> endpoint.
/// </summary>
public record ReleaseInfoViewModel
{
    public required Guid ReleaseId { get; init; }
    public required string ReleaseSlug { get; init; }
}
