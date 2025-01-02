using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
        public string Errors;
    }

    [HttpPut("bau/migrate-datasetfile-geographiclevels")]
    public async Task<MigrationResult> DataSetFileVersionGeographicLevelsMigration(
        [FromQuery] bool isDryRun = true,
        [FromQuery] int? num = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = contentDbContext.Files
            .Where(f => f.Type == FileType.Data
                        && f.DataSetFileVersionGeographicLevels.Count == 0);

        if (num != null)
        {
            queryable = queryable.Take(num.Value);
        }

        var files = queryable.ToList();

        var numProcessed = 0;
        List<string> errors = [];

        foreach (var file in files)
        {
            var meta = file.DataSetFileMeta;

            if (meta == null)
            {
                errors.Add($"No DataSetFileMeta found for File {file.Id}");
                continue;
            }

            var dataSetFileVersionGeographicLevels = meta!.GeographicLevels
                .Distinct()
                .Select(gl => new DataSetFileVersionGeographicLevel
                {
                    DataSetFileVersionId = file.Id,
                    GeographicLevel = gl,
                })
                .ToList();

            contentDbContext.DataSetFileVersionGeographicLevels.AddRange(
                dataSetFileVersionGeographicLevels);

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
            Errors = errors.IsNullOrEmpty() ? "No errors" : errors.JoinToString("\n"),
        };
    }

}
