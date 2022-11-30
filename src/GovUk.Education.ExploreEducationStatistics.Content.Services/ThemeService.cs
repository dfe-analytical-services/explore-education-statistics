#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.EntityFrameworkCore;

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
                theme.Topics.Any(topic =>
                    topic.Publications.Any(publication =>
                        publication.LatestPublishedReleaseId.HasValue &&
                        (publication.SupersededById == null ||
                         !publication.SupersededBy!.LatestPublishedReleaseId.HasValue))))
            .OrderBy(theme => theme.Title)
            .Select(theme => new ThemeViewModel(
                theme.Id,
                theme.Slug,
                theme.Title,
                theme.Summary
            )).ToListAsync();
    }
}
