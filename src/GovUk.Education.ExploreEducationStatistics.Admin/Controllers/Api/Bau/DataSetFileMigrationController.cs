#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class DataSetFileMigrationController(ContentDbContext context) : ControllerBase
{
    public class DataSetFileMigrationResult
    {
        public bool IsDryRun;
        public int NumDataSetFileVersionsCreated;
    }

    // @MarkFix Notes to self: Num DataSetFileVersions per ReleaseVersion == Num DataReleaseFiles
    // @MarkFix Notes to self: We duplicate DataSetFileVersion on amendment, but don't increament DataSetFileVersion.Version number
    // @MarkFix Notes to self: We remove DataSetFileVersion on amendment cancel
    // @MarkFix Notes to self: On replacement, we don't duplicate, set Version to previous Version + 1, need ReplacingDataFileId, ReplacingMetaFileId columns

    [HttpPatch("bau/migrate-data-set-files")]
    public async Task<DataSetFileMigrationResult> MigrateDataSetFiles(
        [FromQuery] bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        var allDataReleaseFiles = context.ReleaseFiles
            .Include(rf => rf.File)
            .Include(rf => rf.ReleaseVersion)
            .Where(rf => rf.File.Type == FileType.Data)
            .ToList();

        var numDataSetFileVersionsCreated = 0;

        foreach (var releaseFile in allDataReleaseFiles)
        {
            var metaFile = await context.Files
                .Where(f => f.SubjectId == releaseFile.File.SubjectId)
                .Where(f => f.Type == FileType.Metadata)
                .SingleOrDefaultAsync(cancellationToken: cancellationToken);

            if (metaFile == null)
            {
                throw new Exception($"Couldn't find meta file for data file {releaseFile.FileId}");
            }

            var dataSetFileVersion = new DataSetFileVersion
            {
                Id = Guid.NewGuid(),
                ReleaseVersionId = releaseFile.ReleaseVersionId,
                DataFileId = releaseFile.File.Id,
                MetaFileId = metaFile.Id,
                Version = 0, // @MarkFix if Files.DataSetFileVersion exists, releaseFile.File.DataSetFileVersion
                CreatedById = releaseFile.File.CreatedById,
                Created = releaseFile.File.Created,
                Updated = null,
                DataSetFile = new DataSetFile
                {
                    Id = Guid.NewGuid(), // @MarkFix if Files.DataSetFileId exists, and DataSetFile doesn't already exist, releaseFile.File.DataSetFileId
                    ReleaseId = releaseFile.ReleaseVersion.ReleaseId,
                },
            };

            context.DataSetFileVersions.Add(dataSetFileVersion);
            numDataSetFileVersionsCreated++;
        }

        if (!dryRun)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return new DataSetFileMigrationResult
        {
            IsDryRun = dryRun,
            NumDataSetFileVersionsCreated = numDataSetFileVersionsCreated,
        };
    }
}
