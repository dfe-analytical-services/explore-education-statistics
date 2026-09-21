#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Services.Organisations;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Organisations.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Organisations;

[Route("api")]
[ApiController]
public class OrganisationsController(IOrganisationsService organisationsService) : ControllerBase
{
    [HttpGet("organisations")]
    public async Task<OrganisationDto[]> GetAllOrganisations(CancellationToken cancellationToken = default) =>
        await organisationsService.GetAllOrganisations(cancellationToken);
}
