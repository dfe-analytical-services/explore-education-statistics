#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class ThemeService : IThemeService
{
    private readonly ContentDbContext _contentDbContext;

    public ThemeService(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task<IList<ThemeViewModel>> ListThemes()
    {
        return await _contentDbContext.Themes
            .Where(theme =>
                theme.Publications.Any(publication =>
                    publication.LatestPublishedReleaseVersionId.HasValue &&
                    (publication.SupercededById == null ||
                    !publication.SupersededBy!.LatestPublishedReleaseVersionId.HasValue)))
            .OrderBy(theme => theme.Title)
            .Select(theme => new ThemeViewModel(
                theme.Id,
                theme.Slug,
                theme.Title,
                theme.Summary
            )).ToListAsync();
    }
}
