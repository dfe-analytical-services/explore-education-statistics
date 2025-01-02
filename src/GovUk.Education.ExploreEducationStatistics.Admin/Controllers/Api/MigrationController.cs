using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using LinqToDB.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class DataSetFileMigrationController(ContentDbContext contentDbContext) : ControllerBase
{
    public class MigrationResult
    {
        public bool IsDryRun;
        public int Processed;
        public int NumLeftToProcess;
        public List<string> Errors;
    }

    [HttpPut("bau/migrate/data-set-files")]
    public async Task<MigrationResult> DataSetFileMigration(
        [FromQuery] bool isDryRun = true,
        CancellationToken cancellationToken = default)
    {
        var dataSetFileIds = contentDbContext.Files
            .Where(f => f.DataSetFileId != null)
            .Select(f => f.DataSetFileId.Value)
            .Distinct()
            .ToList();

        await contentDbContext.AddRangeAsync(dataSetFileIds
            .Select(id => new DataSetFile
            {
                Id = id,
            })
            .ToList(), cancellationToken);


        if (!isDryRun)
        {
            await contentDbContext.SaveChangesAsync(cancellationToken);
        }

        return new MigrationResult
        {
            IsDryRun = isDryRun,
        };
    }

    [HttpPut("bau/migrate/data-set-file-versions")]
    public async Task<MigrationResult> DataSetFileVersionsMigration(
        [FromQuery] bool isDryRun = true,
        [FromQuery] int? num = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = contentDbContext.Files
            .Where(f => f.Type == FileType.Data || f.Type == FileType.Metadata);

        if (num != null)
        {
            queryable = queryable.Take(num.Value);
        }

        var dataFiles = queryable.ToList();

        var numProcessed = 0;
        var errors = new List<string>();

        foreach (var dataFile in dataFiles)
        {
            if (dataFile.DataSetFileId == null || dataFile.DataSetFileVersion == null)
            {
                errors.Add($"File's DataSetFileId or DataSetFileVersion should not be null. FileId: {dataFile.Id}");
            }

            var metaFile = contentDbContext.Files
                .SingleOrDefault(f => f.Type == FileType.Metadata
                                      && f.SubjectId == dataFile.SubjectId);

            if (metaFile == null)
            {
                errors.Add($"Data file has no meta file?!? FileId: {dataFile.Id}");
            }

            var dataSetFileVersion = new DataSetFileVersion
            {
                Id = dataFile.Id,
                DataSetFileId = dataFile.DataSetFileId!.Value,
                Version = dataFile.DataSetFileVersion!.Value,
                SubjectId = dataFile.SubjectId,
                DataFileId = dataFile.Id,
                MetaFileId = metaFile!.Id,
                DataSetFileMeta = dataFile.DataSetFileMeta,
                FilterHierarchies = dataFile.FilterHierarchies,
                ReplacedById = dataFile.ReplacedById,
                ReplacingId = dataFile.ReplacingId,
                SourceId = dataFile.SourceId,
                Created = dataFile.Created,
                CreatedById = dataFile.CreatedById,
            };

            contentDbContext.DataSetFileVersions.Add(dataSetFileVersion);
        }

        if (!isDryRun)
        {
            await contentDbContext.SaveChangesAsync(cancellationToken);
        }

        return new MigrationResult
        {
            IsDryRun = isDryRun,
        };
    }

    [HttpPut("bau/migrate/release-data-set-files")]
    public async Task<MigrationResult> ReleaseVersionDataSetFileVersionsMigration(
        [FromQuery] bool isDryRun = true,
        [FromQuery] int? num = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = contentDbContext.ReleaseFiles
            .Where(rf => !contentDbContext.ReleaseDataSetFiles.Any(rdsf =>
                rdsf.ReleaseVersionId == rf.ReleaseVersionId
                && rdsf.DataSetFileVersionId == rf.FileId));

        if (num != null)
        {
            queryable = queryable.Take(num.Value);
        }

        var releaseFiles = queryable.ToList();

        var numProcessed = 0;
        var errors = new List<string>();

        foreach (var releaseFile in releaseFiles)
        {
            var releaseDataSetFile = new ReleaseDataSetFile
            {
                ReleaseVersionId = releaseFile.ReleaseVersionId,
                DataSetFileVersionId = releaseFile.FileId,
                Title = releaseFile.Name,
                Summary = releaseFile.Summary,
                Order = releaseFile.Order,
                PublicApiDataSetId = releaseFile.PublicApiDataSetId,
                PublicApiDataSetVersion = releaseFile.PublicApiDataSetVersion,
                FilterSequence = releaseFile.FilterSequence,
                IndicatorSequence = releaseFile.IndicatorSequence,
                Published = releaseFile.Published
            };

            contentDbContext.ReleaseDataSetFiles.Add(releaseDataSetFile);
        }

        if (!isDryRun)
        {
            await contentDbContext.SaveChangesAsync(cancellationToken);
        }

        return new MigrationResult
        {
            IsDryRun = isDryRun,
        };
    }
}
