#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class OrganisationService(ContentDbContext contentDbContext) : IOrganisationService
{
    private readonly List<string> _hiddenOrganisations = ["Ofqual", "Ofsted"]; // TODO: Temporarily hiding these orgs until EES-6991 is done

    public async Task<Organisation[]> GetAllOrganisations(CancellationToken cancellationToken = default) =>
        await contentDbContext
            .Organisations.Where(o => !_hiddenOrganisations.Contains(o.Title))
            .OrderBy(o => o.Title)
            .ToArrayAsync(cancellationToken);
}
