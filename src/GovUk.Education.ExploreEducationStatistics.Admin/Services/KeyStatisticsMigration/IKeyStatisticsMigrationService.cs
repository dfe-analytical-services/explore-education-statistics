#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.KeyStatisticsMigration.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.KeyStatisticsMigration;

/// <summary>
/// TODO EES-6747 Remove after the Key Statistics migration is complete.
/// </summary>
public interface IKeyStatisticsMigrationService
{
    Task<Either<ActionResult, KeyStatisticsMigrationReportDto>> MigrateKeyStatisticsGuidanceText(
        bool dryRun = true,
        CancellationToken cancellationToken = default
    );
}
