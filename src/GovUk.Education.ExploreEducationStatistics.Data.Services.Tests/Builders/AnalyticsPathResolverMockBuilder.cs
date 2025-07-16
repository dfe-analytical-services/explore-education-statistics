using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Utils;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Builders;

public class AnalyticsPathResolverMockBuilder
{
    private readonly Mock<IAnalyticsPathResolver> _mock = new(MockBehavior.Strict);
    private string _outputDirectoryPathReturned = "c:\\temp\\test\\directory";

    public IAnalyticsPathResolver Build()
    {
        _mock
            .Setup(m => m.BuildOutputDirectory(It.IsAny<string[]>()))
            .Returns(() => _outputDirectoryPathReturned);
        
        return _mock.Object;
    }

    public AnalyticsPathResolverMockBuilder WhereOutputDirectoryIs(string outputDirectoryPath)
    {
        _outputDirectoryPathReturned = outputDirectoryPath;
        return this;
    }

    public Asserter Assert => new(_mock);
    
    public class Asserter(Mock<IAnalyticsPathResolver> mock)
    {
        public void BuildOutputDirectoryCalled(string[] expectedSubPaths)
        {
            mock.Verify(m => m.BuildOutputDirectory(It.Is<string[]>(actual => actual.SequenceEqual(expectedSubPaths))), Times.Once);
        }
    }
}
