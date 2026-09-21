#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[ApiController]
[Authorize]
[Route("api")]
public class OrganisationsController(IOrganisationsService organisationsService) : ControllerBase
{
    [HttpGet("organisations")]
    public async Task<OrganisationViewModel[]> GetAllOrganisations(CancellationToken cancellationToken = default) =>
        (await organisationsService.GetAllOrganisations(cancellationToken))
            .Select(OrganisationViewModel.FromOrganisation)
            .ToArray();
}
