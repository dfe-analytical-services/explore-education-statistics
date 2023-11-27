#nullable enable
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

/// <summary>
/// TODO EES-4661 Remove after the EES-4660 data guidance migration is successful
/// </summary>
public interface IDataGuidanceMigrationService
{
    public Task<Either<ActionResult, DataGuidanceMigrationReport>> MigrateDataGuidance(
        bool dryRun = true,
        CancellationToken cancellationToken = default);
}
