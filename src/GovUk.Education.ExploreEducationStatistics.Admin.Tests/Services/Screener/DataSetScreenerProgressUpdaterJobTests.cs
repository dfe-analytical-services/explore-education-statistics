#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Screener;

public class DataSetScreenerProgressUpdaterJobTests
{
    private const int ScreenerProgressUpdateIntervalSeconds = 5;

    [Fact]
    public async Task ExecuteTask_SingleJobExecution_Success()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var periodicTimer = new Mock<IPeriodicTimer>(MockBehavior.Strict);

        periodicTimer
            .Setup(p => p.WaitForNextTickAsync(It.IsAny<CancellationToken>()))
            .Callback(() => cancellationTokenSource.Cancel())
            .Returns(ValueTask.FromResult(true));

        periodicTimer.Setup(p => p.Dispose());

        var dataSetScreenerService = new Mock<IDataSetScreenerService>(MockBehavior.Strict);

        dataSetScreenerService.Setup(s => s.UpdateScreeningProgress(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        dataSetScreenerService
            .Setup(s => s.MarkDataSetsWithoutProgressAsFailed(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var job = BuildService(
            dataSetScreenerService: dataSetScreenerService.Object,
            periodicTimer: periodicTimer.Object
        );

        await job.StartAsync(cancellationToken);

        await job.ExecuteTask!;

        VerifyAllMocks(periodicTimer, dataSetScreenerService);
    }

    [Fact]
    public async Task ExecuteTask_MultipleJobExecutions_Success()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var periodicTimer = new Mock<IPeriodicTimer>(MockBehavior.Strict);

        // Allow "WaitForNextTickAsync" to be called twice before cancelling the job execution.
        // This way we can test that the looping in the job is working correctly.
        var iterations = 0;

        periodicTimer
            .Setup(p => p.WaitForNextTickAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                if (++iterations == 2)
                {
                    cancellationTokenSource.Cancel();
                }
            })
            .Returns(ValueTask.FromResult(true));

        periodicTimer.Setup(p => p.Dispose());

        var dataSetScreenerService = new Mock<IDataSetScreenerService>(MockBehavior.Strict);

        dataSetScreenerService.Setup(s => s.UpdateScreeningProgress(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        dataSetScreenerService
            .Setup(s => s.MarkDataSetsWithoutProgressAsFailed(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var job = BuildService(
            dataSetScreenerService: dataSetScreenerService.Object,
            periodicTimer: periodicTimer.Object
        );

        await job.StartAsync(cancellationToken);

        await job.ExecuteTask!;

        periodicTimer.Verify(p => p.WaitForNextTickAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));

        dataSetScreenerService.Verify(
            s => s.MarkDataSetsWithoutProgressAsFailed(It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );

        dataSetScreenerService.Verify(s => s.UpdateScreeningProgress(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private DataSetScreenerProgressUpdaterJob BuildService(
        IDataSetScreenerService? dataSetScreenerService = null,
        IPeriodicTimer? periodicTimer = null
    )
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(dataSetScreenerService ?? Mock.Of<IDataSetScreenerService>(MockBehavior.Strict));
        services.AddSingleton(Mock.Of<ContentDbContext>());

        var timerFactory = new Mock<Func<TimeSpan, IPeriodicTimer>>(MockBehavior.Strict);
        timerFactory
            .Setup(f => f.Invoke(TimeSpan.FromSeconds(ScreenerProgressUpdateIntervalSeconds)))
            .Returns(periodicTimer ?? Mock.Of<IPeriodicTimer>(MockBehavior.Strict));

        return new DataSetScreenerProgressUpdaterJob(
            serviceProvider: services.BuildServiceProvider(),
            options: new DataScreenerOptions
            {
                ScreenerProgressUpdateIntervalSeconds = ScreenerProgressUpdateIntervalSeconds,
            }.ToOptionsWrapper(),
            databaseHelper: new InMemoryDatabaseHelper(new InMemoryDbContextSupplier()),
            timerFactory: timerFactory.Object,
            logger: Mock.Of<ILogger<DataSetScreenerProgressUpdaterJob>>()
        );
    }
}
