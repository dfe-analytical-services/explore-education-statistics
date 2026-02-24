using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using LinqToDB.Internal.Common;
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

        List<EinApiQueryStatTile> updatedTiles = [];
        foreach (var tile in apiQueryTiles)
        {
            var dataSet = dataSets.Single(ds => ds.Id == tile.DataSetId);
            var dataSetLatestVersionId = dataSet.LatestLiveVersionId;

            if (tile.DataSetVersionId != dataSetLatestVersionId)
            {
                // We update all tiles, even if they're for previous versions, so that if there is a
                // draft amendment, the published version still gets updated, so we can indicate it is out-of-date
                // on the public EIN page.
                tile.LatestDataSetVersionId = dataSetLatestVersionId;
                updatedTiles.Add(tile);
            }
        }
        await contentDbContext.SaveChangesAsync(cancellationToken);

        await SendBauEmail(updatedTiles, dataSets, cancellationToken);
    }

    private async Task SendBauEmail(
        List<EinApiQueryStatTile> updatedTiles,
        List<DataSet> dataSets,
        CancellationToken cancellationToken
    )
    {
        // For the email, we only want to inform BAU user about tiles that need updating on latest page versions -
        // they cannot update tiles on older page versions even if they wanted to!
        var uniqueEinPageSlugs = updatedTiles
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

        var pagesToBeInEmail = updatedTiles
            .Select(t => t.EinParentBlock.EinContentSection.EducationInNumbersPage)
            .Where(page => latestEinPageIds.Contains(page.Id))
            .Distinct()
            .ToList();

        if (pagesToBeInEmail.IsNullOrEmpty())
            return;

        var pagesMessage = pagesToBeInEmail
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
        await notifierClient.NotifyEinTilesRequireUpdate(
            new List<EinTilesRequireUpdateMessage> { message },
            cancellationToken
        );
    }
}
