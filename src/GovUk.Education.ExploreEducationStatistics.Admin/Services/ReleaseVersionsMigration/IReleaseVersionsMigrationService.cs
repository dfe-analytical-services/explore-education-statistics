#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration;

/// <summary>
/// TODO EES-6885 Remove after the Release Versions migration is complete.
/// </summary>
public interface IReleaseVersionsMigrationService
{
    Task<Either<ActionResult, ReleaseVersionsMigrationReportDto>> MigrateReleaseVersionsPublishedDate(
        bool dryRun = true,
        CancellationToken cancellationToken = default
    );
}
