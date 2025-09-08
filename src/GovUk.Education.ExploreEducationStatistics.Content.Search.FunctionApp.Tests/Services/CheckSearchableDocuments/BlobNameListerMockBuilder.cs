using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.CheckSearchableDocuments;

public class BlobNameListerMockBuilder
{
    private readonly Mock<IBlobNameLister> _mock = new(MockBehavior.Strict);

    private IList<string> _blobNames = new List<string>();

    public IBlobNameLister Build()
    {
        _mock.Setup(m => m.ListBlobsInContainer(It.IsAny<CancellationToken>())).ReturnsAsync(_blobNames);

        return _mock.Object;
    }

    public BlobNameListerMockBuilder WhereBlobNamesReturnedAre(IList<string> blobNames)
    {
        _blobNames = blobNames;
        return this;
    }
}
