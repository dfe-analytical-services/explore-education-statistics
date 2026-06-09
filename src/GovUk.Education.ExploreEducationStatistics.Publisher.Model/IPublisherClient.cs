#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public interface IPublisherClient
{
    Task PublishMethodologyFiles(Guid methodologyId, CancellationToken cancellationToken = default);

    Task PublishReleaseFiles(PublishReleaseFilesMessage message, CancellationToken cancellationToken = default);

    Task PublishTaxonomy(CancellationToken cancellationToken = default);

    Task HandleReleaseChanged(
        ReleasePublishingKey releasePublishingKey,
        bool immediate,
        CancellationToken cancellationToken = default
    );
}
