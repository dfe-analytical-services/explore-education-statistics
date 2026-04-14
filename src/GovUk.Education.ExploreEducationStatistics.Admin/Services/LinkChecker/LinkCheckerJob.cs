using GovUk.Education.ExploreEducationStatistics.Admin.Models;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.LinkChecker;

public enum LinkCheckerJobStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Canceled,
}

public class LinkCheckerJob
{
    public required Guid Id { get; init; }
    public LinkCheckerJobStatus Status { get; set; } = LinkCheckerJobStatus.Pending;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public List<LinksCsvItem> Results { get; init; } = new();
    public CancellationTokenSource CancellationTokenSource { get; } = new();

    public bool CancellationRequested => CancellationTokenSource.IsCancellationRequested;

    public int TotalLinks => Results.Count;
    public int BrokenLinks => Results.Count(r => r.StatusCode != 200);

    public void Cancel()
    {
        if (Status is LinkCheckerJobStatus.Completed or LinkCheckerJobStatus.Failed or LinkCheckerJobStatus.Canceled)
        {
            return;
        }

        Status = LinkCheckerJobStatus.Canceled;
        ErrorMessage = "Canceled by user.";
        CancellationTokenSource.Cancel();
    }
}
