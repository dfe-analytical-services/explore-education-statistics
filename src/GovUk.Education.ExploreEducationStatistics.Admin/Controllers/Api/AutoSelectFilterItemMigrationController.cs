using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.StringComparison;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class AutoSelectFilterItemMigrationController(StatisticsDbContext statisticsDbContext) : ControllerBase
{
    public class MigrationResult
    {
        public bool IsDryRun;
        public int Processed;
    }

    [HttpPut("bau/auto-select-filter-item")] // @MarkFix test this
    public async Task<MigrationResult> AutoSelectFilterItemMigration(
        [FromQuery] bool isDryRun = true,
        [FromQuery] int? num = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = statisticsDbContext.Filter
            .Where(f =>
                f.AutoSelectFilterItemId == null
                && f.FilterGroups.Count(
                    group => group.FilterItems.Count(
                        item => item.Label.Equals("Total", CurrentCultureIgnoreCase)
                    ) == 1
                ) == 1) // only one Total filter item across all filter items
            .Select(f => f.Id);

        if (num != null)
        {
            queryable = queryable.Take(num.Value);
        }

        var filterIds = queryable.ToList();

        var numProcessed = 0;

        foreach (var filterId in filterIds)
        {
            var defaultFilterItemId = statisticsDbContext.FilterItem
                .Where(fi => fi.FilterGroup.FilterId == filterId
                             && fi.Label.Equals("Total", CurrentCultureIgnoreCase))
                .Select(fi => fi.Id)
                .SingleOrDefault();

            if (defaultFilterItemId == default)
                continue;

            await statisticsDbContext.Filter
                .Where(f => f.Id == filterId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(f => f.AutoSelectFilterItemId, defaultFilterItemId)
                    .SetProperty(f => f.AutoSelectFilterItemLabel, "Total"), cancellationToken);

            numProcessed++;
        }

        if (!isDryRun)
        {
            await statisticsDbContext.SaveChangesAsync(cancellationToken);
        }

        return new MigrationResult
        {
            IsDryRun = isDryRun,
            Processed = numProcessed,
        };
    }
}
