using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration;

/// <summary>
/// TODO EES-6885 Remove after the Release Versions migration is complete.
/// </summary>
public class ReleaseVersionsMigrationService(ContentDbContext contentDbContext, IUserService userService)
    : IReleaseVersionsMigrationService
{
    public async Task<Either<ActionResult, ReleaseVersionsMigrationReportDto>> MigrateReleaseVersionsPublishedDate(
        bool dryRun = true,
        CancellationToken cancellationToken = default
    ) =>
        await userService
            .CheckIsBauUser()
            .OnSuccess(async _ =>
            {
                if (!dryRun)
                {
                    await contentDbContext.SaveChangesAsync(cancellationToken);
                }

                return new ReleaseVersionsMigrationReportDto { DryRun = dryRun };
            });
}
