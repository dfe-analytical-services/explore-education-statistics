namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;

public interface IFullDataSetDocumentResetter
{
    /// <summary>
    /// Delete all Data Set Documents from Azure storage, and return a list of the publications which have data set
    /// documents. Nothing is deleted if that list cannot be retrieved.
    /// </summary>
    Task<FullDataSetDocumentResetResponse> PerformReset(CancellationToken cancellationToken = default);
}
