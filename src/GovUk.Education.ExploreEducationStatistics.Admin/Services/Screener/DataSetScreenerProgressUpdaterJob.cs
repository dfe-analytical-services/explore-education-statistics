using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Screener;

public class DataSetScreenerProgressUpdaterJob(
    IServiceProvider serviceProvider,
    IOptions<DataScreenerOptions> options,
    IDatabaseHelper databaseHelper,
    ILogger<DataSetScreenerProgressUpdaterJob> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.ScreenerProgressUpdateIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ContentDbContext>();

            // Perform screener progress updates with an exclusive lock to prevent multiple
            // instances of the Admin app service from performing updates in parallel.
            // The DataSetUploads.ScreenerProgressLastChecked dates for any data sets
            // currently undergoing screening will be updated by the instance that successfully
            // obtained the lock, and the check against this ScreenerProgressLastChecked date in
            // DataSetScreenerService.UpdateScreeningProgress() will prevent a subsequent execution
            // of this job by another instance from finding any data sets to update (because they've
            // been updated very recently and therefore do not require another update yet).
            await databaseHelper.ExecuteWithExclusiveLock(
                dbContext: dbContext,
                lockName: $"Admin_{nameof(DataSetScreenerProgressUpdaterJob)}",
                _ => UpdateProgress(serviceScope: scope, cancellationToken: stoppingToken)
            );

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task UpdateProgress(IServiceScope serviceScope, CancellationToken cancellationToken)
    {
        try
        {
            var dataSetScreenerService = serviceScope.ServiceProvider.GetRequiredService<IDataSetScreenerService>();

            var updates = await dataSetScreenerService.UpdateScreeningProgress(cancellationToken: cancellationToken);

            if (updates.Count > 0)
            {
                logger.LogInformation(
                    "{Count} data sets had screener progress updates at {Time} (UTC)",
                    updates.Count,
                    DateTimeOffset.UtcNow
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update screener progress at {Time} (UTC)", DateTimeOffset.UtcNow);
        }
    }
}
