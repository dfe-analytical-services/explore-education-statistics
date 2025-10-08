using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
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
        var publicationRedirects = await _contentDbContext
            .PublicationRedirects.Where(pr => pr.Slug != pr.Publication.Slug) // don't use redirects to the current live slug
            .Distinct()
            .Select(pr => new RedirectViewModel(pr.Slug, pr.Publication.Slug))
            .ToListAsync();

        // A redirect for a MethodologyVersion that isn't published shouldn't appear in the list. A redirect becomes
        // active once the associated MethodologyVersion is published. It remains active if subsequent methodology
        // amendments are published. We establish this by checking against each MethodologyVersion's Version number.
        var methodologyRedirects = (
            await _contentDbContext
                .MethodologyRedirects.Where(mr =>
                    mr.MethodologyVersion.Methodology.LatestPublishedVersion != null
                    && mr.MethodologyVersion.Methodology.LatestPublishedVersion.Version >= mr.MethodologyVersion.Version
                )
                .Select(mr => new
                {
                    RedirectSlug = mr.Slug,
                    // MethodologyVersion.Slug cannot be translated into SQL, so we do this...
                    LatestPublishedSlug = mr.MethodologyVersion.Methodology.LatestPublishedVersion!.AlternativeSlug
                        ?? mr.MethodologyVersion.Methodology.OwningPublicationSlug,
                })
                .ToListAsync()
        )
            .Select(mr =>
            {
                if (mr.RedirectSlug == mr.LatestPublishedSlug)
                {
                    // guard against redirect pointing to current slug
                    return null;
                }

                return new RedirectViewModel(FromSlug: mr.RedirectSlug, ToSlug: mr.LatestPublishedSlug);
            })
            .WhereNotNull()
            .Distinct()
            .ToList();

        var releaseRedirectsByPublicationSlug = (
            await _contentDbContext
                .ReleaseRedirects.Where(rr => rr.Slug != rr.Release.Slug) // don't use redirects to the current live slug
                .Distinct()
                .Select(rr => new
                {
                    PublicationSlug = rr.Release.Publication.Slug,
                    FromSlug = rr.Slug,
                    ToSlug = rr.Release.Slug,
                })
                .ToListAsync()
        ).GroupBy(rr => rr.PublicationSlug).ToDictionary(g => g.Key, g => g.Select(a => new RedirectViewModel(FromSlug: a.FromSlug, ToSlug: a.ToSlug)).ToList());

        return new RedirectsViewModel(
            PublicationRedirects: publicationRedirects,
            MethodologyRedirects: methodologyRedirects,
            ReleaseRedirectsByPublicationSlug: releaseRedirectsByPublicationSlug
        );
    }
}
