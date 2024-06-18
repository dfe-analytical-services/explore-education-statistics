#nullable enable
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api/bau")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class BauMigrationController(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext)
    : ControllerBase
{
    [HttpPost("migrate-ees-4993")]
    public async Task<ActionResult> MigrateEes4993(CancellationToken cancellationToken)
    {
        var unpublishedDataSetVersions = await publicDataDbContext
            .DataSetVersions
            .Where(dsv => dsv.Status == DataSetVersionStatus.Draft
                          || dsv.Status == DataSetVersionStatus.Failed
                          || dsv.Status == DataSetVersionStatus.Processing
                          || dsv.Status == DataSetVersionStatus.Cancelled)
            .ToListAsync(cancellationToken);

        foreach (var draftVersion in unpublishedDataSetVersions)
        {
            // Update any published release files to no longer incorrectly reference draft data set versions.
            //
            // This should only apply to draft data set versions for amendments that were created prior
            // to the `EES4993_AddPublicApiDataSetIdVersionToReleaseFile` migration.

            // The DB migration moves the `PublicApiDataSetId` and `PublicApiDataSetVersion` columns
            // from `Files` to `ReleaseFiles`, but can set these incorrectly for published release files.
            // We need this additional endpoint migration to fix any incorrect column values.
            await contentDbContext.ReleaseFiles
                .Where(rf => rf.ReleaseVersion.Published != null)
                .Where(rf => rf.PublicApiDataSetId == draftVersion.DataSetId)
                .Where(rf => rf.PublicApiDataSetVersion == draftVersion.Version)
                .ExecuteUpdateAsync(
                    s => s
                        .SetProperty(rf => rf.PublicApiDataSetId, _ => null)
                        .SetProperty(rf => rf.PublicApiDataSetVersion, _ => null),
                    cancellationToken
                );
        }

        return Ok();
    }
}
