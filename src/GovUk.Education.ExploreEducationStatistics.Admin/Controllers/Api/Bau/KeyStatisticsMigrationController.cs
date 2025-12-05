#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.KeyStatisticsMigration;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.KeyStatisticsMigration.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

/// <summary>
/// TODO EES-6747 Remove after the Key Statistics migration is complete.
/// </summary>
[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class KeyStatisticsMigrationController(IKeyStatisticsMigrationService keyStatisticsMigrationService)
    : ControllerBase
{
    [HttpPatch("bau/migrate-key-statistics-guidance-text")]
    public Task<ActionResult<KeyStatisticsMigrationReportDto>> MigrateKeyStatisticsGuidanceText(
        [FromQuery] bool dryRun = true,
        CancellationToken cancellationToken = default
    ) => keyStatisticsMigrationService.MigrateKeyStatisticsGuidanceText(dryRun, cancellationToken).HandleFailuresOrOk();
}
