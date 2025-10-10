#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api/bau")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class BauBlobManagementController(IPrivateBlobStorageService privateBlobStorageService) : ControllerBase
{
    [HttpPut("rename-release-directories")]
    public async Task<ActionResult> RenameReleaseDirectories(
        bool dryRun = true,
        CancellationToken cancellationToken = default
    )
    {
        var directoryPrefixFilters = new List<string> { "data-zip", "bulk-data-zip" };
        var renamedDirectoryPrefix = "z";
        var blobKeys = await GetBlobKeysForFilters(directoryPrefixFilters, cancellationToken);

        if (dryRun)
        {
            return Ok(new { BlobsToBeMovedCount = blobKeys.Count, BlobsToBeMoved = blobKeys });
        }

        var renamedDirectories = new Dictionary<string, string>();

        foreach (var blobKey in blobKeys)
        {
            var releaseId = blobKey.Split('/')[0];
            var subDirectoryKey = blobKey.Split('/', 3)[1];
            var blobSourcePath = $"{releaseId}/{subDirectoryKey}";
            var blobDestinationPath = $"{releaseId}/{renamedDirectoryPrefix}{subDirectoryKey}";

            if (renamedDirectories.ContainsKey(blobSourcePath))
            {
                // A release may contain numerous blobs with the same directory prefix, MoveDirectory only needs to be called per directory, not per blob.
                continue;
            }

            await privateBlobStorageService.MoveDirectory(
                PrivateReleaseFiles,
                blobSourcePath,
                PrivateReleaseFiles,
                blobDestinationPath
            );

            renamedDirectories.Add(blobSourcePath, blobDestinationPath);
        }

        return Ok(
            new
            {
                MovedBlobCount = blobKeys.Count,
                RenamedDirectoryCount = renamedDirectories.Count,
                RenamedDirectories = renamedDirectories,
            }
        );
    }

    [HttpDelete("delete-release-directories")]
    public async Task<ActionResult> DeleteReleaseDirectories(
        bool dryRun = true,
        CancellationToken cancellationToken = default
    )
    {
        var directoryPrefixFilters = new List<string> { "zdata-zip", "zbulk-data-zip" };
        var blobKeys = await GetBlobKeysForFilters(directoryPrefixFilters, cancellationToken);

        if (dryRun)
        {
            return Ok(new { BlobsToBeDeletedCount = blobKeys.Count, BlobsToBeDeleted = blobKeys });
        }

        var deletedBlobs = new List<string>();
        var deletedDirectories = new List<string>();

        foreach (var blobKey in blobKeys)
        {
            var releaseId = blobKey.Split('/')[0];
            var subDirectoryKey = blobKey.Split('/', 3)[1];
            var blobSourcePath = $"{releaseId}/{subDirectoryKey}";

            if (deletedDirectories.Contains(blobSourcePath))
            {
                // A release may contain numerous blobs with the same directory prefix, DeleteBlobs only needs to be called per directory, not per blob.
                deletedBlobs.Add(blobKey);
                continue;
            }

            await privateBlobStorageService.DeleteBlobs(PrivateReleaseFiles, blobSourcePath);

            deletedDirectories.Add(blobSourcePath);
            deletedBlobs.Add(blobKey);
        }

        return Ok(
            new
            {
                DeletedBlobCount = deletedBlobs.Count,
                DeletedDirectoryCount = deletedDirectories.Count,
                DeletedDirectories = deletedDirectories,
                DeletedBlobs = deletedBlobs,
            }
        );
    }

    private async Task<List<string>> GetBlobKeysForFilters(
        List<string> directoryPrefixFilters,
        CancellationToken cancellationToken
    )
    {
        var blobKeys = new List<string>();
        foreach (var filter in directoryPrefixFilters)
        {
            blobKeys.AddRange(await privateBlobStorageService.GetBlobs(PrivateReleaseFiles, filter, cancellationToken));
        }

        return blobKeys;
    }
}
