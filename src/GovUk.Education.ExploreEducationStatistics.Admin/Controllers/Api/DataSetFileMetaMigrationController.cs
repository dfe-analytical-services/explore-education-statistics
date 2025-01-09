using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class DataSetFileMetaMigrationController(ContentDbContext contentDbContext) : ControllerBase
{
    public class MigrationResult
    {
        public bool IsDryRun;
        public int Processed;
        public int NumLeftToProcess;
    }

    [HttpPut("bau/meta-geographic-levels-remove")]
    public async Task<MigrationResult> DataSetFileVersionGeographicLevelsMigration(
        [FromQuery] bool isDryRun = true,
        [FromQuery] int? num = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = contentDbContext.Files
            .Where(f => f.Type == FileType.Data
                        && f.DataSetFileMetaGeogLvlMigrated == false
                        && f.DataSetFileVersionGeographicLevels.Count > 0); // Paranoidly protecting against data set where geog lvls aren't migrated

        if (num != null)
        {
            queryable = queryable.Take(num.Value);
        }

        var files = queryable.ToList();

        var numProcessed = 0;

        foreach (var file in files)
        {
            if (file.DataSetFileMeta != null)
            {
                file.DataSetFileMeta = new DataSetFileMeta
                {
                    GeographicLevels = null,
                    TimePeriodRange = file.DataSetFileMeta.TimePeriodRange,
                    Filters = file.DataSetFileMeta.Filters,
                    Indicators = file.DataSetFileMeta.Indicators,
                };
            }

            file.DataSetFileMetaGeogLvlMigrated = true;
            numProcessed++;
        }

        if (!isDryRun)
        {
            await contentDbContext.SaveChangesAsync(cancellationToken);
        }

        return new MigrationResult
        {
            IsDryRun = isDryRun,
            Processed = numProcessed,
            NumLeftToProcess = contentDbContext.Files.Count(f => f.DataSetFileMetaGeogLvlMigrated == false),
        };
    }

}
