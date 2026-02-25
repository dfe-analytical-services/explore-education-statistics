#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.UserResourceRolesMigration.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.UserResourceRolesMigration;

/// <summary>
/// TODO EES-6957 Remove after the User Resource Roles migration is complete.
/// </summary>
public interface IUserResourceRolesMigrationService
{
    Task<Either<ActionResult, UserResourceRolesMigrationReportDto>> MigrateUserResourceRoles(
        bool dryRun = true,
        CancellationToken cancellationToken = default
    );
}
