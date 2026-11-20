using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.CheckSearchableDocuments;

public class BlobNameListerMockBuilder
{
    private readonly Mock<IBlobNameLister> _mock = new(MockBehavior.Strict);

    private IList<string> _blobNames = new List<string>();

    public BlobNameListerMockBuilder()
    {
        Assert = new Asserter(_mock);
    }

    public IBlobNameLister Build()
    {
        _mock
            .Setup(m => m.ListBlobsInContainer(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_blobNames);

        return _mock.Object;
    }

    public BlobNameListerMockBuilder WhereBlobNamesReturnedAre(IList<string> blobNames)
    {
        _blobNames = blobNames;
        return this;
    }

    public Asserter Assert { get; }

    public class Asserter(Mock<IBlobNameLister> mock)
    {
        public void BlobsWereListedForContainer(string containerName)
        {
            mock.Verify(m => m.ListBlobsInContainer(containerName, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
