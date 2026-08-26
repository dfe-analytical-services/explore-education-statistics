using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class ReleaseService(ContentDbContext contentDbContext) : IReleaseService
{
    public async Task<ReleaseVersion> Get(Guid releaseVersionId)
    {
        return await contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
            .SingleAsync(releaseVersion => releaseVersion.Id == releaseVersionId);
    }

    public async Task<IEnumerable<ReleaseVersion>> GetAmendedReleases(IEnumerable<Guid> releaseVersionIds)
    {
        return await contentDbContext
            .ReleaseVersions.Include(rv => rv.PreviousVersion)
            .Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
            .Where(rv => releaseVersionIds.Contains(rv.Id) && rv.PreviousVersionId != null)
            .ToListAsync();
    }

    public async Task<ReleaseVersion> GetLatestPublishedReleaseVersion(Guid publicationId)
    {
        var publication = await contentDbContext.Publications.SingleAsync(p => p.Id == publicationId);

        // Get the publications release id's by the order they appear in the release series
        var releaseSeriesReleaseIds = publication.ReleaseSeries.ReleaseIds();

        // The latest published release version for a publication is determined by finding the latest published version
        // of the first release in the series that has a published version.
        ReleaseVersion? latestPublishedReleaseVersion = null;
        foreach (var releaseId in releaseSeriesReleaseIds)
        {
            latestPublishedReleaseVersion = await contentDbContext
                .ReleaseVersions.LatestReleaseVersion(releaseId: releaseId, publishedOnly: true)
                .SingleOrDefaultAsync();

            if (latestPublishedReleaseVersion != null)
            {
                break;
            }
        }

        return latestPublishedReleaseVersion
            ?? throw new InvalidOperationException(
                $"No latest published release version found for publication {publicationId}"
            );
    }

    public async Task<List<File>> GetFiles(Guid releaseVersionId, params FileType[] types)
    {
        return await contentDbContext
            .ReleaseFiles.Include(rf => rf.File)
            .Where(rf => rf.ReleaseVersionId == releaseVersionId)
            .Select(rf => rf.File)
            .Where(file => types.Contains(file.Type))
            .ToListAsync();
    }

    public async Task CompletePublishing(Guid releaseVersionId, DateTimeOffset actualPublishedDate)
    {
        var releaseVersion = await contentDbContext
            .ReleaseVersions.Include(rv => rv.DataBlockVersions)
                .ThenInclude(dataBlockVersion => dataBlockVersion.DataBlock)
            .SingleAsync(rv => rv.Id == releaseVersionId);

        // Set Published to the actual published date, maintaining a record of when the release version was actually published.
        releaseVersion.Published = actualPublishedDate;

        // Calculate and set the PublishedDisplayDate, which can differ from the actual published date.
        var publishedDisplayDate = await CalculatePublishedDisplayDate(releaseVersion, actualPublishedDate);
        releaseVersion.PublishedDisplayDate = publishedDisplayDate;

        await UpdateReleaseFilesPublishedDate(releaseVersion, actualPublishedDate);

        await UpdatePublishedDataBlockVersions(releaseVersion);

        await contentDbContext.SaveChangesAsync();
    }

    private async Task<DateTimeOffset> CalculatePublishedDisplayDate(
        ReleaseVersion releaseVersion,
        DateTimeOffset actualPublishedDate
    )
    {
        // For the first version of a release or if an update to the published display date has been requested
        // return the actual published date
        if (releaseVersion.Version == 0 || releaseVersion.UpdatePublishedDisplayDate)
        {
            return actualPublishedDate;
        }

        // Otherwise, return the published display date from the previous version
        await contentDbContext.Entry(releaseVersion).Reference(rv => rv.PreviousVersion).LoadAsync();
        var previousVersion = releaseVersion.PreviousVersion!;

        if (!previousVersion.PublishedDisplayDate.HasValue)
        {
            throw new ArgumentException(
                $"Expected previous release version '{releaseVersion.PreviousVersionId}' to be published."
            );
        }

        return previousVersion.PublishedDisplayDate.Value;
    }

    private async Task UpdateReleaseFilesPublishedDate(ReleaseVersion releaseVersion, DateTimeOffset publishedDate)
    {
        // Only update the Published date for new or replacement files, i.e. where the Published date is null.
        // This is to preserve the original published date for unchanged files on an amendment version.
        var newOrReplacementReleaseFiles = await contentDbContext
            .ReleaseFiles.Where(rf => rf.ReleaseVersionId == releaseVersion.Id)
            .Where(rf => rf.Published == null)
            .ToListAsync();

        newOrReplacementReleaseFiles.ForEach(rf => rf.Published = publishedDate.UtcDateTime);
    }

    private async Task UpdatePublishedDataBlockVersions(ReleaseVersion releaseVersion)
    {
        // Update all of the DataBlocks to point their "LatestPublishedVersions" to the "latest" versions
        // on the Release. Mark the "latest" version as null until a Release Amendment is created.
        var latestDataBlocks = releaseVersion
            .DataBlockVersions.Select(dataBlockVersion => dataBlockVersion.DataBlock)
            .ToList();

        latestDataBlocks.ForEach(latestDataBlock =>
        {
            // Promote the LatestDraftVersion as the newly published version.
            latestDataBlock.LatestPublishedVersion = latestDataBlock.LatestDraftVersion;
            latestDataBlock.LatestPublishedVersionId = latestDataBlock.LatestDraftVersionId;
            latestDataBlock.LatestPublishedVersion!.Published = DateTime.UtcNow;

            // Remove the LatestDraftVersion completely until such a point in the future as a Release Amendment is
            // created that will contain this Data Block as a Draft.
            latestDataBlock.LatestDraftVersionId = null;
        });

        // Find all DataBlockVersions that were part of the previously published Release, if any, and update them
        // to no longer have a Published version.
        if (releaseVersion.PreviousVersionId != null)
        {
            var latestDataBlockIds = latestDataBlocks.Select(dataBlock => dataBlock.Id);

            var removedDataBlockVersions = await contentDbContext
                .DataBlockVersions.Where(dataBlockVersion =>
                    dataBlockVersion.ReleaseVersionId == releaseVersion.PreviousVersionId
                    && !latestDataBlockIds.Contains(dataBlockVersion.DataBlockId)
                )
                .Include(dataBlockVersion => dataBlockVersion.DataBlock)
                .ToListAsync();

            // Update all of the removed DataBlockVersions' DataBlocks to point their
            // "LatestPublishedVersions" to null, as they should no longer be publicly accessible.
            removedDataBlockVersions.ForEach(latestDataBlockVersion =>
            {
                var latestDataBlock = latestDataBlockVersion.DataBlock;
                latestDataBlock.LatestPublishedVersionId = null;
            });
        }
    }
}
