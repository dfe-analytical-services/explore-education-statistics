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
            .EinTiles.Include(t => t.EinParentBlock.EinContentSection.EinPageVersion.EinPage)
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

        SendBauEmail(updatedTiles, [.. dataSets.Values]);
    }

    private void SendBauEmail(List<EinApiQueryStatTile> updatedTiles, List<DataSet> dataSets)
    {
        // For the email, we only want to inform BAU user about tiles that need updating on latest page versions -
        // they cannot update tiles on older/published page versions even if they wanted to!
        var latestEinPageVersionIds = updatedTiles
            .Where(tile =>
                tile.EinParentBlock.EinContentSection.EinPageVersionId
                == tile.EinParentBlock.EinContentSection.EinPageVersion.EinPage.LatestVersionId
            )
            .Select(tile => tile.EinParentBlock.EinContentSection.EinPageVersionId)
            .Distinct()
            .ToList();

        var pageVersionsToBeInEmail = updatedTiles
            .Select(t => t.EinParentBlock.EinContentSection.EinPageVersion)
            .Where(pageVersion => latestEinPageVersionIds.Contains(pageVersion.Id))
            .Distinct()
            .ToList();

        if (pageVersionsToBeInEmail.Count == 0)
        {
            return;
        }

        var bulletsStr = new StringBuilder();
        foreach (var pageVersion in pageVersionsToBeInEmail)
        {
            bulletsStr.Append(
                $"* [{pageVersion.EinPage.Title}]({_appOptions.AdminAppUrl}/education-in-numbers/{pageVersion.Id}/content)\n"
            );

            foreach (var tile in updatedTiles)
            {
                // sub-bullets for each tile associated with a particular page
                if (pageVersion.Id != tile.EinParentBlock.EinContentSection.EinPageVersionId)
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
