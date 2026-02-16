#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.UserResourceRolesMigration.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.UserResourceRolesMigration;

/// <summary>
/// TODO EES-XXXX Remove after the User Resource Roles migration is complete.
/// </summary>
public interface IUserResourceRolesMigrationService
{
    Task<Either<ActionResult, ThingDto>> MigrateUserResourceRoles(
        bool dryRun = true,
        CancellationToken cancellationToken = default
    );
}
