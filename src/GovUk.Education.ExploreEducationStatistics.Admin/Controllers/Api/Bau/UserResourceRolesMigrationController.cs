#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration.Dtos;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.UserResourceRolesMigration;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

/// <summary>
/// TODO EES-6885 Remove after the Release Versions migration is complete.
/// </summary>
[Route("api/bau")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class UserResourceRolesMigrationController(IUserResourceRolesMigrationService userResourceRolesMigrationService)
    : ControllerBase
{
    [HttpPatch("migrate-user-resource-roles")]
    public Task<ActionResult<ReleaseVersionsMigrationReportDto>> MigrateUserResourceRoles(
        [FromQuery] bool dryRun = true,
        CancellationToken cancellationToken = default
    ) =>
        // TODO EES-6830 Dry run has been deliberately set to true to prevent accidental runs
        // while it is being tested.
        userResourceRolesMigrationService
            .MigrateUserResourceRoles(dryRun: true, cancellationToken)
            .HandleFailuresOrOk();
}
