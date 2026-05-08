using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class PublicationService(ContentDbContext contentDbContext) : IPublicationService
{
    public async Task<IList<PublicationInfoViewModel>> ListPublicationInfos(
        Guid? themeId = null,
        CancellationToken cancellationToken = default
    ) =>
        await contentDbContext
            .Publications.Include(p => p.LatestPublishedReleaseVersion)
                .ThenInclude(rv => rv!.Release)
            .Where(p =>
                // Is published
                p.LatestPublishedReleaseVersionId.HasValue
                // Is not superseded/archived
                && (p.SupersededById == null || !p.SupersededBy!.LatestPublishedReleaseVersionId.HasValue)
            )
            .If(!themeId.IsBlank())
            .ThenWhere(p => p.ThemeId == themeId!.Value)
            .Select(publication => PublicationInfoViewModel.FromEntity(publication))
            .ToListAsync(cancellationToken);
}
