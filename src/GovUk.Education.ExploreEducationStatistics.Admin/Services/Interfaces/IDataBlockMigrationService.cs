#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    /**
     * Interface used to migrate data blocks. Intended to be adapted as needed when migrations are required.  
     */
    public interface IDataBlockMigrationService
    {
        Task<Either<ActionResult, Unit>> Migrate();
    }
}
