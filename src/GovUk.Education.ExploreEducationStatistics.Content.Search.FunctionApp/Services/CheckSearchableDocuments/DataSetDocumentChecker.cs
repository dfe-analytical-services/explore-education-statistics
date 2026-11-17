using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public class DataSetDocumentChecker(
    IBlobNameLister blobNameLister,
    Func<IContentApiClient> contentApiClientFactory,
    IOptions<AppOptions> appOptions
)
{
    public async Task<DataSetDocumentsCheckReport> RunCheck(CancellationToken cancellationToken = default)
    {
        // Get a list of all blobs
        var blobNames = await blobNameLister.ListBlobsInContainer(
            appOptions.Value.DataSetDocumentsContainerName,
            cancellationToken
        );

        // Retrieve the keys of the data set documents expected for the latest published version of every release
        var dataSetDocumentKeys = await contentApiClientFactory().GetDataSetDocumentKeys(cancellationToken);

        var expectedBlobNames = dataSetDocumentKeys.Select(key => key.BlobName).ToArray();

        // Compare the list of expected blob names with the list of actual blob names
        var (leftOnly, both, rightOnly) = expectedBlobNames.Diff(blobNames);

        return new DataSetDocumentsCheckReport
        {
            OkBlobCount = both.Length,
            TotalBlobCount = blobNames.Count,
            TotalExpectedCount = expectedBlobNames.Length,
            MissingBlobs = leftOnly,
            ExtraneousBlobs = rightOnly,
        };
    }
}
