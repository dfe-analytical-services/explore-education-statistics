#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class OrganisationService(ContentDbContext contentDbContext) : IOrganisationService
{
    public IAsyncEnumerable<Organisation> GetAllOrganisations() =>
        contentDbContext.Organisations
            .OrderBy(o => o.Title)
            .AsAsyncEnumerable();
}
