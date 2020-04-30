using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IEES17PermalinkMigrationService
    {
        Task<Either<ActionResult, bool>> MigrateAll();
    }
}