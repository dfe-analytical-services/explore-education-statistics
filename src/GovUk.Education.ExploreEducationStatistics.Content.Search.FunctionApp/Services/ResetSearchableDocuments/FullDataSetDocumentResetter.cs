using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;

public class FullDataSetDocumentResetter(
    IContentApiClient contentApiClient,
    IDataSetDocumentRemover dataSetDocumentRemover,
    ILogger<FullDataSetDocumentResetter> logger
) : IFullDataSetDocumentResetter
{
    public async Task<FullDataSetDocumentResetResponse> PerformReset(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing Full Data Set Document Reset");

        // Get the publications which have data set documents before deleting anything, so that a failure to get them
        // does not leave an empty container with nothing queued to fill it again
        var publications = await contentApiClient.GetDataSetDocumentPublications(cancellationToken);

        // Delete all data set documents
        var removed = await dataSetDocumentRemover.RemoveAllDataSetDocuments(cancellationToken);

        // Return the list of publications to send to the Data Set Document creator
        return new FullDataSetDocumentResetResponse
        {
            Publications = publications,
            RemovedBlobCount = removed.RemovedBlobNames.Count,
        };
    }
}
