#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IOrganisationsService
{
    Task<Organisation[]> GetAllOrganisations(CancellationToken cancellationToken = default);
}
