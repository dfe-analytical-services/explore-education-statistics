using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IPermalinkMigrationService
    {
        Task<Either<ActionResult, bool>> MigrateAll<T>(Func<T, Task<Either<string, Permalink>>> transformFunc);
    }
}