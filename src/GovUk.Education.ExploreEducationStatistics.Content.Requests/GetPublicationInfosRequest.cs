namespace GovUk.Education.ExploreEducationStatistics.Content.Requests;

/// <summary>
/// Request type used only by the Content.Search.FunctionApp (Search Docs Function App)
/// via the <c>GET /api/publicationInfos</c> endpoint.
/// </summary>
public record GetPublicationInfosRequest(Guid? ThemeId);
