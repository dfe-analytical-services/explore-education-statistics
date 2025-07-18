#nullable enable
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[ApiController]
[Authorize]
[Route("api")]
public class OrganisationsController(IOrganisationService organisationService) : ControllerBase
{
    [HttpGet("organisations")]
    public async Task<OrganisationViewModel[]> GetAllOrganisations(CancellationToken cancellationToken = default) =>
        (await organisationService.GetAllOrganisations(cancellationToken))
        .Select(OrganisationViewModel.FromOrganisation)
        .ToArray();
}
