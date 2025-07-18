#nullable enable
using System.Collections.Generic;
using System.Linq;
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
    public IAsyncEnumerable<OrganisationViewModel> GetAllOrganisations() =>
        organisationService.GetAllOrganisations()
            .Select(OrganisationViewModel.FromOrganisation);
}
