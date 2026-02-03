using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class EducationInNumbersService(ContentDbContext contentDbContext, PublicDataDbContext publicDataDbContext)
    : IEducationInNumbersService
{
    public async Task UpdateEinTiles(Guid[] releaseVersionIdsToUpdate, CancellationToken cancellationToken = default)
    {
        var updatedPublicationIds = await contentDbContext
            .ReleaseVersions.AsNoTracking()
            .Select(rv => rv.Release.PublicationId)
            .ToListAsync(cancellationToken);

        var dataSets = await publicDataDbContext
            .DataSets.AsNoTracking()
            .Include(ds => ds.LatestLiveVersion)
            .Where(ds => updatedPublicationIds.Contains(ds.PublicationId) && ds.LatestLiveVersion != null)
            .ToListAsync(cancellationToken);

        var dataSetIds = dataSets.Select(ds => ds.Id).ToList();

        var apiQueryTiles = await contentDbContext
            .EinTiles.OfType<EinApiQueryStatTile>()
            .Where(t => t.DataSetId != null && dataSetIds.Contains(t.DataSetId.Value))
            .ToListAsync(cancellationToken);

        foreach (var tile in apiQueryTiles)
        {
            var dataSet = dataSets.Single(ds => ds.Id == tile.DataSetId);
            var dataSetLatestVersion =
                $"{dataSet.LatestLiveVersion!.VersionMajor}.{dataSet.LatestLiveVersion.VersionMinor}.{dataSet.LatestLiveVersion.VersionPatch}";

            if (tile.LatestPublishedVersion != dataSetLatestVersion)
            {
                tile.LatestPublishedVersion = dataSetLatestVersion;
                // TODO EES-6868 Send email to inform BAU that a tile's papi query needs updating
            }
        }
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }
}
