using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Utils;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Builders;

public class AnalyticsPathResolverMockBuilder
{
    private readonly Mock<IAnalyticsPathResolver> _mock = new(MockBehavior.Strict);
    private string _permaLinkTableDownloadCallsDirectoryPath = "the permalink table download calls directory path";
    private string _tableToolDownloadCallsDirectoryPath = "the table tool download calls directory path";

    public IAnalyticsPathResolver Build()
    {
        _mock
            .Setup(m => m.GetPermaLinkTableDownloadCallsDirectoryPath())
            .Returns(() => _permaLinkTableDownloadCallsDirectoryPath);
        
        _mock
            .Setup(m => m.GetTableToolDownloadCallsDirectoryPath())
            .Returns(() => _tableToolDownloadCallsDirectoryPath);
        
        return _mock.Object;
    }

    public Asserter Assert => new(_mock);
    
    public class Asserter(Mock<IAnalyticsPathResolver> mock)
    {
        public void GetPermaLinkTableDownloadCallsDirectoryPathRequested()
        {
            mock.Verify(m => m.GetPermaLinkTableDownloadCallsDirectoryPath(), Times.Once);;
        }
        public void GetTableToolDownloadCallsDirectoryPathRequested()
        {
            mock.Verify(m => m.GetTableToolDownloadCallsDirectoryPath(), Times.Once);;
        }
    }
}
