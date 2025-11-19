#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IPublicationRepository : Content.Model.Repository.Interfaces.IPublicationRepository
{
    Task<List<Publication>> ListPublicationsForUser(
        Guid userId,
        Guid? themeId = null,
        CancellationToken cancellationToken = default
    );
}
