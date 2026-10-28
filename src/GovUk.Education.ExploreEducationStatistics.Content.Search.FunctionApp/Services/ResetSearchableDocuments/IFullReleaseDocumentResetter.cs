namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;

public interface IFullReleaseDocumentResetter
{
    /// <summary>
    /// Delete all Searchable Documents from Azure storage, and return a list of all live publications.
    /// </summary>
    Task<PerformResetResponse> PerformReset(CancellationToken cancellationToken = default);
}
