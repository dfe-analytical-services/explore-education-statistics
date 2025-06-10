using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public class SearchableDocumentChecker(IBlobNameLister blobNameLister, IReleaseSummaryRetriever releaseSummaryRetriever)
{
    public async Task<CheckSearchableDocumentsReport> RunCheck(CancellationToken cancellationToken = default)
    {
        // Get a list of all blobs
        var blobNames = await blobNameLister.ListBlobsInContainer(cancellationToken);

        // Retrieve the release summaries for all published publications
        var releaseSummaries = await releaseSummaryRetriever.GetAllPublishedReleaseSummaries(cancellationToken);
        
        var expectedReleaseIds = releaseSummaries.Select(rs => rs.ReleaseId).ToArray();
        
        // Compare the list of release summaries with the list of blobs
        var (leftOnly, both, rightOnly) = expectedReleaseIds.Diff(blobNames);

        return new CheckSearchableDocumentsReport
        {
            OkBlobCount = both.Length,
            TotalBlobCount = blobNames.Count,
            TotalPublicationCount = releaseSummaries.Count,
            MissingBlobs = leftOnly,
            ExtraneousBlobs = rightOnly,
            MissingBlobSummaries = leftOnly
                .Select(id => releaseSummaries.First(rs => rs.ReleaseId == id))
                .Select(ReleaseSummaryViewModel.FromModel)
                .ToArray(),
        };
    }
}
