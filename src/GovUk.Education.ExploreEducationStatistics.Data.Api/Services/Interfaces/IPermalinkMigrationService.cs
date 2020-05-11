using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IPermalinkMigrationService
    {
        Task<bool> MigrateAll<T>(string migrationId, Func<T, Task<Either<string, Permalink>>> transformFunc);
    }
}