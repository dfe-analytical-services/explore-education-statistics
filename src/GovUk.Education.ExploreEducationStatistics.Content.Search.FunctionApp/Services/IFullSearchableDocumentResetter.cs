namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

public interface IFullSearchableDocumentResetter
{
    /// <summary>
    /// Delete all Searchable Documents from Azure storage, and return a list of all live publications.
    /// </summary>
    Task<PerformResetResponse> PerformReset(CancellationToken cancellationToken = default);
}