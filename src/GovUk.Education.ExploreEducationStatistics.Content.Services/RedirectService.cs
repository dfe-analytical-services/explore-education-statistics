#nullable enable
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class RedirectService : IRedirectService
{
    private readonly ContentDbContext _contentDbContext;

    public RedirectService(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    // @MarkFix Keep Methodology.Slug column in case the whole thing goes wrong

    public async Task<Either<ActionResult, RedirectsViewModel>> List()
    {
        var methodologyRedirects = await _contentDbContext.MethodologyRedirects
            .Where(mr => mr.MethodologyVersion.Methodology.LatestPublishedVersionId != null // @MarkFix if it's not published, then no redirects should exist?
                                            && mr.MethodologyVersion.Version <= mr.MethodologyVersion.Methodology.LatestPublishedVersion.Version)
            // @MarkFix <= because we need to include redirects created when an inherited publication slug is changed
            // or the migrated redirects - and these redirects use LatestPublishedVersionId
            .Select(mr => new MethodologyRedirectViewModel(
                mr.Slug, mr.MethodologyVersion.Methodology.LatestPublishedVersion.Slug)) // @MarkFix
            .Distinct() // @MarkFix yes?
            .ToListAsync();

        return new RedirectsViewModel(methodologyRedirects);
    }
}

