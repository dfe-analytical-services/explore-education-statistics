#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

/// <summary>
/// TODO EES-4372 Remove after the EES-4364 filter migration is complete
/// </summary>
[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class FilterMigrationController : ControllerBase
{
    private readonly IFilterMigrationService _filterMigrationService;

    public FilterMigrationController(IFilterMigrationService filterMigrationService)
    {
        _filterMigrationService = filterMigrationService;
    }

    [HttpPatch("bau/migrate-filter-group-csv-columns")]
    public async Task<ActionResult<FilterMigrationReport>> MigrateGroupCsvColumns(
        [FromQuery] bool dryRun = true)
    {
        return await _filterMigrationService
            .MigrateGroupCsvColumns(dryRun)
            .HandleFailuresOrOk();
    }
}
