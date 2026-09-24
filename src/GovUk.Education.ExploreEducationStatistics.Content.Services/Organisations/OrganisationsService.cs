using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Organisations.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Organisations;

public class OrganisationsService(ContentDbContext contentDbContext) : IOrganisationsService
{
    public async Task<OrganisationDto[]> GetAllOrganisations(CancellationToken cancellationToken = default) =>
        await contentDbContext
            .Organisations.AsNoTracking()
            .OrderByTitleWithDepartmentForEducationFirst()
            .Select(o => OrganisationDto.FromOrganisation(o))
            .ToArrayAsync(cancellationToken);
}
