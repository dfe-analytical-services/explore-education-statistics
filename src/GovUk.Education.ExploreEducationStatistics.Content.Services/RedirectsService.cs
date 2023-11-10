#nullable enable
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class RedirectsService : IRedirectsService
{
    private readonly ContentDbContext _contentDbContext;

    public RedirectsService(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task<Either<ActionResult, RedirectsViewModel>> List()
    {
        // A redirect for a MethodologyVersion that isn't published shouldn't appear in the list. A redirect becomes
        // active once the associated MethodologyVersion is published. It remains active if subsequent methodology
        // amendments are published. We establish this by checking against each MethodologyVersion's Version number.
        var methodologyRedirects = await _contentDbContext.MethodologyRedirects
            .Where(mr => mr.MethodologyVersion.Methodology.LatestPublishedVersion != null
                         && mr.MethodologyVersion.Methodology.LatestPublishedVersion.Version >=
                         mr.MethodologyVersion.Version
            )
            .Select(mr => new
            {
                RedirectSlug = mr.Slug,
                // MethodologyVersion.Slug cannot be translated into SQL, so we do this...
                LatestPublishedSlug = mr.MethodologyVersion.Methodology.LatestPublishedVersion!.AlternativeSlug
                                      ?? mr.MethodologyVersion.Methodology.OwningPublicationSlug,
            })
            .ToListAsync();

        var methodologyRedirectViewModels = methodologyRedirects
            .Select(mr =>
            {
                if (mr.RedirectSlug == mr.LatestPublishedSlug)
                {
                    // It is possible for a redirect to point to the currently active slug. This can happen as a
                    // methodology can change to a slug even if there is a redirect for that slug,
                    // *if* the redirect is for the same methodology - i.e. the methodology's slug is changing
                    // back to a previous slug it used. But the slug may not change back immediately, if the change
                    // is on an unpublished amendment, so we cannot remove the redirect immediately when changing the
                    // slug back; redirect needs to be active until the amendment is published.
                    return null;
                }

                return new MethodologyRedirectViewModel(
                    FromSlug: mr.RedirectSlug, ToSlug: mr.LatestPublishedSlug);
            })
            .WhereNotNull()
            .Distinct()
            .ToList();

        return new RedirectsViewModel(methodologyRedirectViewModels);
    }
}
