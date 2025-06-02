#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class DataSetFileMetaMigrationController(ContentDbContext contentDbContext) : ControllerBase
{
    public class DataSetFileMetaMigrationResult
    {
        public bool dryRun;
        public int Processed;
        public List<string> Errors = new();
    }

    [HttpPatch("bau/migrate-datasetfilemeta-totalrows")]
    public async Task<DataSetFileMetaMigrationResult> MigrateDataSetFileMeta(
        [FromQuery] bool dryRun = true,
        [FromQuery] int? num = null,
        CancellationToken cancellationToken = default)
    {
        var files = (await contentDbContext.Files
                .Where(f =>
                    f.DataSetFileMeta != null
                    && f.Type == FileType.Data)
                .ToListAsync(cancellationToken: cancellationToken))
            .Where(f => f.DataSetFileMeta!.NumDataFileRows == null);

        if (num != null)
        {
            files = files.Take(num.Value);
        }

        files = files.ToList();

        var errors = new List<string>();
        var numProcessed = 0;

        foreach (var file in files)
        {
            var totalRows = contentDbContext.DataImports
                .Where(di => di.SubjectId == file.SubjectId)
                .Select(di => di.TotalRows)
                .SingleOrDefault();

            if (totalRows == null)
            {
                errors.Add($"No DataImport.TotalRows found for Subject {file.SubjectId}");
                continue;
            }

            file.DataSetFileMeta!.NumDataFileRows = totalRows;
            contentDbContext.Update(file);
            numProcessed++;
        }

        if (!dryRun)
        {
            await contentDbContext.SaveChangesAsync(cancellationToken);
        }

        return new DataSetFileMetaMigrationResult
        {
            dryRun = dryRun,
            Processed = numProcessed,
            Errors = errors,
        };
    }
}
