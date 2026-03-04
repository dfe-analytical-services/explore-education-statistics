namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IPublishingService
{
    Task PublishStagedReleaseContent(CancellationToken cancellationToken = default);

    Task PublishMethodologyFiles(Guid methodologyId, CancellationToken cancellationToken = default);

    Task PublishMethodologyFilesIfApplicableForRelease(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    );

    Task PublishReleaseFiles(Guid releaseVersionId, CancellationToken cancellationToken = default);
}
