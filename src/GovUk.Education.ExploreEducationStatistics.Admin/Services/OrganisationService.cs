#nullable enable
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class OrganisationService(ContentDbContext contentDbContext) : IOrganisationService
{
    public async Task<Organisation[]> GetAllOrganisations(CancellationToken cancellationToken = default) =>
        await contentDbContext.Organisations
            .OrderBy(o => o.Title)
            .ToArrayAsync(cancellationToken);
}
