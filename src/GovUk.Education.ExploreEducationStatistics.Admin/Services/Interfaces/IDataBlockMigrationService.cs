#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    /**
     * Temporary interface used to migrate data blocks for EES-3167.
     */
    public interface IDataBlockMigrationService
    {
        Task<Either<ActionResult, Unit>> MigrateAll();
    }
}
