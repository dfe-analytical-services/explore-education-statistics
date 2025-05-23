#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class ThemeService(ContentDbContext contentDbContext) : IThemeService
{
    public async Task<IList<ThemeViewModel>> ListThemes() =>
        await contentDbContext.Themes
            .Where(theme =>
                theme.Publications.Any(publication =>
                    publication.LatestPublishedReleaseVersionId.HasValue &&
                    (publication.SupersededById == null ||
                     !publication.SupersededBy!.LatestPublishedReleaseVersionId.HasValue)))
            .OrderBy(theme => theme.Title)
            .Select(theme => new ThemeViewModel
            {
                Id = theme.Id,
                Slug = theme.Slug,
                Title = theme.Title,
                Summary = theme.Summary
            }).ToListAsync();
}
