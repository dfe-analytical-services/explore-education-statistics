using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public class SearchableDocumentChecker(IBlobNameLister blobNameLister, IReleaseSummaryRetriever releaseSummaryRetriever)
{
    public async Task<CheckSearchableDocumentsReport> RunCheck(CancellationToken cancellationToken = default)
    {
        // Get a list of all blobs
        var blobNames = await blobNameLister.ListBlobsInContainer(cancellationToken);

        // Retrieve the latest published release version summaries for the latest published releases of all published publications
        var releaseVersionSummaries = await releaseSummaryRetriever.GetAllPublishedReleaseVersionSummaries(
            cancellationToken
        );

        var expectedReleaseIds = releaseVersionSummaries.Select(rs => rs.ReleaseId).ToArray();

        // Compare the list of release id's with the list of blob names
        var (leftOnly, both, rightOnly) = expectedReleaseIds.Diff(blobNames);

        return new CheckSearchableDocumentsReport
        {
            OkBlobCount = both.Length,
            TotalBlobCount = blobNames.Count,
            TotalPublicationCount = releaseVersionSummaries.Count,
            MissingBlobs = leftOnly,
            ExtraneousBlobs = rightOnly,
            MissingBlobSummaries = leftOnly
                .Select(id => releaseVersionSummaries.First(rs => rs.ReleaseId == id))
                .Select(ReleaseVersionSummaryViewModel.FromModel)
                .ToArray(),
        };
    }
}
