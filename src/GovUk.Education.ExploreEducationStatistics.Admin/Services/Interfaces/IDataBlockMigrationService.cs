#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    /**
     * Temporary interface used to migrate data blocks for EES-3167 and EES-3142.
     */
    public interface IDataBlockMigrationService
    {
        Task<Either<ActionResult, List<DataBlockMigrationService.MapMigrationResult>>> MigrateMaps(bool dryRun = true);
    }
}
