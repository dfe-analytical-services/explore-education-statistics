using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

/**
 * Interface used to migrate files in EES-3547
 * TODO Remove in EES-3552
 */
public interface IFileMigrationService
{
    Task<Either<ActionResult, Unit>> MigrateAll();
}
