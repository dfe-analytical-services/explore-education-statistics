#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

/// <summary>
/// TODO EES-3755 Remove after Permalink snapshot migration work is complete
/// </summary>
[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class PermalinkMigrationController : ControllerBase
{
    private readonly IPermalinkMigrationService _permalinkMigrationService;

    public PermalinkMigrationController(IPermalinkMigrationService permalinkMigrationService)
    {
        _permalinkMigrationService = permalinkMigrationService;
    }

    [HttpPatch("bau/migrate-permalinks")]
    public async Task<ActionResult<Unit>> MigratePermalinks()
    {
        return await _permalinkMigrationService
            .MigrateAll()
            .HandleFailuresOrOk();
    }
}
