using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

/**
 * Controller used to migrate files in EES-3547
 * TODO Remove in EES-3552
 */
[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class FileMigrationController : ControllerBase
{
    private readonly IFileMigrationService _fileMigrationService;

    public FileMigrationController(IFileMigrationService fileMigrationService)
    {
        _fileMigrationService = fileMigrationService;
    }

    [HttpPatch("bau/migrate-files")]
    public async Task<ActionResult<Unit>> MigrateFiles()
    {
        return await _fileMigrationService
            .MigrateAll()
            .HandleFailuresOrOk();
    }
}
