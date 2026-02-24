using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class EducationInNumbersService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    INotifierClient notifierClient
) : IEducationInNumbersService
{
    public async Task UpdateEinTiles(Guid[] releaseVersionIdsToUpdate, CancellationToken cancellationToken = default)
    {
        if (releaseVersionIdsToUpdate.Length == 0)
            return;

        var updatedPublicationIds = await contentDbContext
            .ReleaseVersions.AsNoTracking()
            .Select(rv => rv.Release.PublicationId)
            .ToListAsync(cancellationToken);

        var dataSets = await publicDataDbContext
            .DataSets.Include(ds => ds.LatestLiveVersion)
            .Where(ds => updatedPublicationIds.Contains(ds.PublicationId) && ds.LatestLiveVersion != null)
            .ToListAsync(cancellationToken);

        var dataSetIds = dataSets.Select(ds => ds.Id).ToList();

        var apiQueryTiles = await contentDbContext
            .EinTiles.Include(t => t.EinParentBlock.EinContentSection.EducationInNumbersPage)
            .OfType<EinApiQueryStatTile>()
            .Where(t => t.DataSetId != null && dataSetIds.Contains(t.DataSetId.Value))
            .ToListAsync(cancellationToken);

        // BAU users don't need to update EIN pages that have newer versions, so we filter them out
        var uniqueEinPageSlugs = apiQueryTiles
            .Select(tile => tile.EinParentBlock.EinContentSection.EducationInNumbersPage.Slug)
            .Distinct()
            .ToList();

        var latestEinPageIds = await uniqueEinPageSlugs
            .ToAsyncEnumerable()
            .Select(
                async (slug, ct) =>
                    await contentDbContext
                        // We don't care if the latest page is published or not - we're collecting these pages for BAU
                        // users to update. If the latest is published, they'll have to create an amendment - if it
                        // is a draft or draft amendment, then they'll want to edit that.
                        .EducationInNumbersPages.Where(page => page.Slug == slug)
                        .OrderByDescending(page => page.Version)
                        .FirstAsync(cancellationToken: ct)
            )
            .Select(page => page.Id)
            .ToListAsync(cancellationToken);

        var latestEinPageApiQueryTiles = apiQueryTiles.Where(tile =>
            latestEinPageIds.Contains(tile.EinParentBlock.EinContentSection.EducationInNumbersPageId)
        );

        List<EinApiQueryStatTile> updatedTiles = [];
        foreach (var tile in latestEinPageApiQueryTiles)
        {
            var dataSet = dataSets.Single(ds => ds.Id == tile.DataSetId);
            var dataSetLatestVersionId = dataSet.LatestLiveVersionId;

            if (tile.DataSetVersionId != dataSetLatestVersionId)
            {
                tile.LatestDataSetVersionId = dataSetLatestVersionId;
                updatedTiles.Add(tile);
            }
        }
        await contentDbContext.SaveChangesAsync(cancellationToken);

        // construct message to send bau email to inform about tiles that require updating
        var pagesRequiringUpdate = updatedTiles
            .Select(t => t.EinParentBlock.EinContentSection.EducationInNumbersPage)
            .Distinct();

        var pagesMessage = pagesRequiringUpdate
            .Select(updatedPage =>
            {
                var tilesMessage = updatedTiles
                    .Where(t => t.EinParentBlock.EinContentSection.EducationInNumbersPageId == updatedPage.Id)
                    .Select(updatedTile => new EinTileRequiresUpdate
                    {
                        Title = updatedTile.Title,
                        ContentSectionTitle = updatedTile.EinParentBlock.EinContentSection.Heading,
                        DataSetFileId = dataSets
                            .Single(dataSet => dataSet.Id == updatedTile.DataSetId)
                            .LatestLiveVersion!.Release.DataSetFileId,
                    })
                    .ToList();

                return new EinPageRequiresUpdate
                {
                    Id = updatedPage.Id,
                    Title = updatedPage.Title,
                    Tiles = tilesMessage,
                };
            })
            .ToList();

        var message = new EinTilesRequireUpdateMessage { Pages = pagesMessage };
        await notifierClient.NotifyEinTilesRequireUpdate([message], cancellationToken);
    }
}
