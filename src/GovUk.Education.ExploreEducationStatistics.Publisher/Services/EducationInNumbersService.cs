using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

/// <summary>
/// Publish events specific to the Publisher
/// </summary>
public class EducationInNumbersService(ContentDbContext contentDbContext, PublicDataDbContext publicDataDbContext)
    : IEducationInNumbersService
{
    public async Task UpdateEinTiles(Guid[] releaseVersionIdsToUpdate)
    {
        var updatedPublicationIds = contentDbContext
            .ReleaseVersions.Include(rv => rv.Release.Publication)
            .Select(rv => rv.Release.PublicationId)
            .ToList();

        var dataSets = publicDataDbContext
            .DataSets.Include(ds => ds.LatestLiveVersion)
            .Where(ds => updatedPublicationIds.Contains(ds.PublicationId) && ds.LatestLiveVersion != null)
            .ToList();

        var dataSetIds = dataSets.Select(ds => ds.Id).ToList();

        var apiQueryTiles = contentDbContext
            .EinTiles.OfType<EinApiQueryStatTile>()
            .Where(t => t.DataSetId != null && dataSetIds.Contains(t.DataSetId.Value))
            .ToList();

        foreach (var tile in apiQueryTiles)
        {
            var dataSet = dataSets.Single(ds => ds.Id == tile.DataSetId);
            var dataSetLatestVersion =
                $"{dataSet.LatestLiveVersion!.VersionMajor}.{dataSet.LatestLiveVersion.VersionMinor}.{dataSet.LatestLiveVersion.VersionPatch}";

            if (tile.LatestPublishedVersion != dataSetLatestVersion)
            {
                tile.LatestPublishedVersion = dataSetLatestVersion;
                // TODO Send email to inform BAU that a tile needs updating
                await contentDbContext.SaveChangesAsync();
            }
        }
    }
}
