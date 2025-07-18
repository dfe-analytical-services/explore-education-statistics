#nullable enable
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IOrganisationService
{
    Task<Organisation[]> GetAllOrganisations(CancellationToken cancellationToken = default);
}
