using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Functions;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Functions;

public class ConsumeAnalyticsRequestFilesFunctionTests
{
    [Fact]
    public async Task ConsumeAnalyticsRequestFilesFunction_MultipleStrategies()
    {
        var strategy1 = new Mock<IRequestFileProcessor>(MockBehavior.Strict);
        strategy1.Setup(s => s.Process()).Returns(Task.CompletedTask);

        var strategy2 = new Mock<IRequestFileProcessor>(MockBehavior.Strict);
        strategy2.Setup(s => s.Process()).Returns(Task.CompletedTask);

        var function = BuildFunction([strategy1, strategy2]);

        await function.Run(new TimerInfo());

        strategy1.Verify(s => s.Process(), Times.Once);
        strategy2.Verify(s => s.Process(), Times.Once);
    }

    private ConsumeAnalyticsRequestFilesFunction BuildFunction(List<Mock<IRequestFileProcessor>> processorServices)
    {
        return new(processorServices.Select(s => s.Object), Mock.Of<ILogger<ConsumeAnalyticsRequestFilesFunction>>());
    }
}
