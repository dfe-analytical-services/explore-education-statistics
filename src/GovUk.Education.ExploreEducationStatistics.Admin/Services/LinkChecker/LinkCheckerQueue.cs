using System.Collections.Concurrent;
using System.Threading.Channels;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.LinkChecker;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.LinkChecker;

public class LinkCheckerQueue : ILinkCheckerQueue
{
    private readonly Channel<LinkCheckerJob> _channel = Channel.CreateBounded<LinkCheckerJob>(
        new BoundedChannelOptions(100)
    );

    private readonly ConcurrentDictionary<Guid, LinkCheckerJob> _jobs = new();

    public Guid EnqueueJob()
    {
        var job = new LinkCheckerJob { Id = Guid.NewGuid() };

        _jobs[job.Id] = job;
        _channel.Writer.TryWrite(job);

        return job.Id;
    }

    public bool TryGetJob(Guid jobId, out LinkCheckerJob? job) => _jobs.TryGetValue(jobId, out job);

    public bool TryCancelJob(Guid jobId)
    {
        if (!_jobs.TryGetValue(jobId, out var job))
        {
            return false;
        }

        if (
            job.Status is LinkCheckerJobStatus.Completed or LinkCheckerJobStatus.Failed or LinkCheckerJobStatus.Canceled
        )
        {
            return false;
        }

        var wasPending = job.Status == LinkCheckerJobStatus.Pending;
        job.Cancel();

        if (wasPending)
        {
            job.CompletedAt = DateTimeOffset.UtcNow;
        }

        return true;
    }

    public async ValueTask<LinkCheckerJob> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }
}
