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

    public async Task<Either<ActionResult, RedirectsViewModel>> List()
    {
        var methodologyRedirects = await _contentDbContext.MethodologyRedirects
            .Where(mr => mr.MethodologyVersion.Methodology.OwningPublication().Publication.LatestPublishedRelease !=
                         null)
            .Select(mr => new MethodologyRedirectViewModel(
                mr.Slug, mr.MethodologyVersion.Slug))
            .ToListAsync();

        return new RedirectsViewModel(methodologyRedirects);
    }
}

