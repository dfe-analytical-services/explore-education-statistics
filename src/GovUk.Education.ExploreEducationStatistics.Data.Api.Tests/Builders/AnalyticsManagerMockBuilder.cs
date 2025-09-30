using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Builders;

public class AnalyticsManagerMockBuilder
{
    private readonly Mock<IAnalyticsManager> _mock = new(MockBehavior.Strict);

    public IAnalyticsManager Build()
    {
        _mock
            .Setup(m => m.Add(It.IsAny<IAnalyticsCaptureRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return _mock.Object;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IAnalyticsManager> mock)
    {
        public void RequestAdded(IAnalyticsCaptureRequest expected)
        {
            mock.Verify(m => m.Add(ItIs.DeepEqualTo(expected), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
