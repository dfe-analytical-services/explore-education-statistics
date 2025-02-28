using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class AutoSelectFilterItemMigrationController(StatisticsDbContext statisticsDbContext) : ControllerBase
{
    public class MigrationResult
    {
        public int Processed;
    }

    [HttpPut("bau/auto-select-filter-item")]
    public async Task<MigrationResult> AutoSelectFilterItemMigration(
        [FromQuery] int? num = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = statisticsDbContext.Filter
            .Where(f =>
                f.AutoSelectFilterItemId == null
                // one group with label "Total" that contains one item with label "Total"
                && f.FilterGroups.Count(group => group.Label == "Total"
                                               && group.FilterItems.Count(item => item.Label == "Total")
                                               == 1)
                == 1)
            .Select(f => f.Id);

        if (num != null)
        {
            queryable = queryable.Take(num.Value);
        }

        var filterIds = queryable.ToList();

        var numProcessed = 0;

        foreach (var filterId in filterIds)
        {
            var autoSelectFilterItemId = statisticsDbContext.FilterItem
                .Where(fi => fi.FilterGroup.FilterId == filterId
                             && fi.Label == "Total"
                             && fi.FilterGroup.Label == "Total")
                .Select(fi => fi.Id)
                .SingleOrDefault();

            if (autoSelectFilterItemId == default)
                continue;

            await statisticsDbContext.Filter
                .Where(f => f.Id == filterId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(f => f.AutoSelectFilterItemId, autoSelectFilterItemId)
                    .SetProperty(f => f.AutoSelectFilterItemLabel, "Total"), cancellationToken);

            numProcessed++;
        }

        return new MigrationResult
        {
            Processed = numProcessed,
        };
    }
}
