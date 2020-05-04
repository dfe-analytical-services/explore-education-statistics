using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    /**
     * Temporary service used to migrate datablocks for EES-17.
     */
    public interface IDataBlockMigrationService
    {
        Task<Either<ActionResult, bool>> MigrateAll();
    }
}