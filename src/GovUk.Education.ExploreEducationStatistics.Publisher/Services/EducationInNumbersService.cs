using System.Text;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class EducationInNumbersService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    IEmailService emailService,
    IOptions<AppOptions> appOptions
) : IEducationInNumbersService
{
    private readonly AppOptions _appOptions = appOptions.Value;

    public async Task UpdateEinTiles(Guid[] releaseVersionIdsToUpdate, CancellationToken cancellationToken = default)
    {
        if (releaseVersionIdsToUpdate.Length == 0)
        {
            return;
        }

        var updatedPublicationIds = await contentDbContext
            .ReleaseVersions.AsNoTracking()
            .Where(rv => releaseVersionIdsToUpdate.Contains(rv.Id))
            .Select(rv => rv.Release.PublicationId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var dataSets = await publicDataDbContext
            .DataSets.Include(ds => ds.LatestLiveVersion)
            .Where(ds => updatedPublicationIds.Contains(ds.PublicationId) && ds.LatestLiveVersion != null)
            .ToDictionaryAsync(ds => ds.Id, cancellationToken);

        var apiQueryTiles = await contentDbContext
            .EinTiles.Include(t => t.EinParentBlock.EinContentSection.EducationInNumbersPage)
            .OfType<EinApiQueryStatTile>()
            .Where(t => t.DataSetId != null && dataSets.Keys.Contains(t.DataSetId.Value))
            .ToListAsync(cancellationToken);

        List<EinApiQueryStatTile> updatedTiles = [];
        foreach (var tile in apiQueryTiles)
        {
            var dataSet = dataSets[tile.DataSetId!.Value];
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

        await SendBauEmail(updatedTiles, [.. dataSets.Values], cancellationToken);
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

        if (pagesToBeInEmail.Count == 0)
        {
            return;
        }

        var bulletsStr = new StringBuilder();
        foreach (var page in pagesToBeInEmail)
        {
            bulletsStr.Append($"* [{page.Title}]({_appOptions.AdminAppUrl}/education-in-numbers/{page.Id}/content)\n");

            foreach (var tile in updatedTiles)
            {
                // sub-bullets for each tile associated with a particular page
                if (page.Id != tile.EinParentBlock.EinContentSection.EducationInNumbersPageId)
                {
                    continue;
                }

                var contentSectionTitle = tile.EinParentBlock.EinContentSection.Heading;
                var dataSetFileId = dataSets
                    .Single(dataSet => dataSet.Id == tile.DataSetId)
                    .LatestLiveVersion!.Release.DataSetFileId;

                bulletsStr.Append(
                    $"  * Tile titled '{tile.Title}' in section '{contentSectionTitle}', which uses [this data set]({_appOptions.PublicAppUrl}/data-catalogue/data-set/{dataSetFileId})\n"
                );
            }
        }

        emailService.NotifyEinTilesRequireUpdate(_appOptions.BauEmail, bulletsStr.ToString());
    }
}
