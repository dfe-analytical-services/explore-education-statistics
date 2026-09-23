using GovUk.Education.ExploreEducationStatistics.Content.Services.Organisations.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Organisations;

public interface IOrganisationsService
{
    Task<OrganisationDto[]> GetAllOrganisations(CancellationToken cancellationToken = default);
}
