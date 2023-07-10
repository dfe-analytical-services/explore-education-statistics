#nullable enable
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

/// <summary>
/// TODO EES-4372 Remove after the EES-4364 filter migration is complete
/// </summary>
public interface IFilterMigrationService
{
    public Task<Either<ActionResult, FilterMigrationReport>> MigrateGroupCsvColumns(
        bool dryRun = true,
        CancellationToken cancellationToken = default);
}
