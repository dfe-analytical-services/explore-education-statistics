#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IOrganisationService
{
    IAsyncEnumerable<Organisation> GetAllOrganisations();
}
