using GovUk.Education.ExploreEducationStatistics.Admin.Services.LinkChecker;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.LinkChecker;

public interface ILinkCheckerQueue
{
    Guid EnqueueJob();
    bool TryGetJob(Guid jobId, out LinkCheckerJob? job);
    bool TryCancelJob(Guid jobId);
    ValueTask<LinkCheckerJob> DequeueAsync(CancellationToken cancellationToken);
}
