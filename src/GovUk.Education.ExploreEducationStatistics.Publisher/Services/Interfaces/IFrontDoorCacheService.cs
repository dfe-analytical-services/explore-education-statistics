namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IFrontDoorCacheService
{
    Task PurgeAllFilesZipCache(IReadOnlySet<Guid> releaseVersionIds, CancellationToken cancellationToken = default);
}
