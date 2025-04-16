namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

public class RemovePublicationSearchableDocumentsResponse
{
    public Dictionary<Guid, bool> ReleaseIdToDeletionResult { get; } = [];
}
