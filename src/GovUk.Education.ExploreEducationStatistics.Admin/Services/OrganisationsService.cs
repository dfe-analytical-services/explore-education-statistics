#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class OrganisationsService(ContentDbContext contentDbContext) : IOrganisationsService
{
    public async Task<OrganisationViewModel[]> GetAllOrganisations(CancellationToken cancellationToken = default) =>
        await contentDbContext
            .Organisations.AsNoTracking()
            .OrderBy(o => o.Title)
            .Select(o => OrganisationViewModel.FromOrganisation(o))
            .ToArrayAsync(cancellationToken);
}
