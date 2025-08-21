#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class EducationInNumbersContentController(
    IEducationInNumbersContentService einContentService) : ControllerBase
{
    [HttpGet("education-in-numbers/{id:guid}/content")]
    public async Task<ActionResult<EducationInNumbersContentViewModel>> GetPageContent(
        [FromRoute] Guid id)
    {
        return await einContentService.GetPageContent(id)
            .HandleFailuresOrOk();
    }
}
