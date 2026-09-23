#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IOrganisationsService
{
    Task<OrganisationViewModel[]> GetAllOrganisations(CancellationToken cancellationToken = default);
}
