using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.LinkChecker;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.LinkChecker;

public class LinkCheckerBackgroundService(
    ILogger<LinkCheckerBackgroundService> logger,
    ILinkCheckerQueue queue,
    IServiceProvider serviceProvider,
    ILinksChecker checker
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Link checking background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            LinkCheckerJob job;

            try
            {
                job = await queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (job.Status == LinkCheckerJobStatus.Canceled || job.CancellationRequested)
            {
                job.CompletedAt ??= DateTimeOffset.UtcNow;
                logger.LogInformation("Link check job {JobId} was canceled before execution.", job.Id);
                continue;
            }

            job.Status = LinkCheckerJobStatus.Running;
            job.StartedAt = DateTimeOffset.UtcNow;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                stoppingToken,
                job.CancellationTokenSource.Token
            );

            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                var currentEnvironment =
                    env.IsDevelopment() ? CurrentEnvironment.Local
                    : env.IsStaging() ? CurrentEnvironment.Staging
                    : env.IsProduction() ? CurrentEnvironment.Prod
                    : CurrentEnvironment.Local;

                var extractedLinks = await checker.ExtractReleaseLinksAsync(context, linkedCts.Token);

                var failedLinks = (
                    await checker.TestReleaseLinksAsync(extractedLinks, linkedCts.Token, currentEnvironment)
                )
                    .Where(tl => tl.StatusCode != 200 || tl.AnchorExists is false)
                    .ToList();

                job.Results.AddRange(failedLinks);

                if (job.Status != LinkCheckerJobStatus.Canceled)
                {
                    job.Status = LinkCheckerJobStatus.Completed;
                    job.CompletedAt = DateTimeOffset.UtcNow;
                    logger.LogInformation(
                        "Link check job {JobId} completed with {TotalLinks} links.",
                        job.Id,
                        job.TotalLinks
                    );
                }
            }
            catch (OperationCanceledException)
            {
                job.Status = LinkCheckerJobStatus.Canceled;
                job.ErrorMessage ??= "Canceled by user.";
                job.CompletedAt = DateTimeOffset.UtcNow;
                logger.LogInformation("Link check job {JobId} was canceled during execution.", job.Id);
            }
            catch (Exception ex)
            {
                job.Status = LinkCheckerJobStatus.Failed;
                job.ErrorMessage = ex.Message;
                job.CompletedAt = DateTimeOffset.UtcNow;
                logger.LogError(ex, "Link check job {JobId} failed.", job.Id);
            }
        }

        logger.LogInformation("Link checking background service stopping.");
    }
}
