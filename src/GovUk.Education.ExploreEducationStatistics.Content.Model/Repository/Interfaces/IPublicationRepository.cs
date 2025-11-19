#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;

public interface IPublicationRepository
{
    Task<bool> IsPublished(Guid publicationId, CancellationToken cancellationToken = default);

    Task<bool> IsSuperseded(Guid publicationId, CancellationToken cancellationToken = default);
}
