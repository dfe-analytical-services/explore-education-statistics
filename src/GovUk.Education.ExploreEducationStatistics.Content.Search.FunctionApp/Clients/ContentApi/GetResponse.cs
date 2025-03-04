using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

internal abstract record GetResponse
{
    public record NotFound : GetResponse;
    public record Error(string ErrorMessage) : GetResponse;
    public record Successful(ReleaseSearchableDocument ReleaseSearchableDocument) : GetResponse;
}
