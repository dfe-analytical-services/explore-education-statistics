#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.PublicationService;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PublicationRepository : Content.Model.Repository.PublicationRepository, IPublicationRepository
{
    private readonly ContentDbContext _context;

    public PublicationRepository(ContentDbContext context) : base(context)
    {
        _context = context;
    }

    public IQueryable<Publication> QueryPublicationsForTheme(Guid? themeId = null)
    {
        return _context.Publications
            .Where(publication => themeId == null || publication.ThemeId == themeId);
    }

    public async Task<List<Publication>> ListPublicationsForUser(
        Guid userId,
        Guid? themeId = null)
    {
        var publicationsGrantedByPublicationRoleQueryable = _context
            .UserPublicationRoles
            .Where(userPublicationRole => userPublicationRole.UserId == userId &&
                                          ListOf(PublicationRole.Owner, PublicationRole.Allower)
                                              .Contains(userPublicationRole.Role));

        if (themeId.HasValue)
        {
            publicationsGrantedByPublicationRoleQueryable =
                publicationsGrantedByPublicationRoleQueryable.Where(userPublicationRole =>
                    userPublicationRole.Publication.ThemeId == themeId.Value);
        }

        var publicationsGrantedByPublicationRole = await publicationsGrantedByPublicationRoleQueryable
            .Select(userPublicationRole => userPublicationRole.Publication)
            .ToListAsync();

        var publicationIdsGrantedByPublicationRole = publicationsGrantedByPublicationRole
            .Select(publication => publication.Id)
            .ToList();

        var releasesGrantedByReleaseRolesQueryable = _context.UserReleaseRoles
            .Include(userReleaseRole => userReleaseRole.ReleaseVersion)
            .ThenInclude(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .Where(userReleaseRole => userReleaseRole.UserId == userId &&
                                      userReleaseRole.Role != ReleaseRole.PrereleaseViewer);

        if (themeId.HasValue)
        {
            releasesGrantedByReleaseRolesQueryable =
                releasesGrantedByReleaseRolesQueryable.Where(userReleaseRole =>
                    userReleaseRole.ReleaseVersion.Release.Publication.ThemeId == themeId.Value);
        }

        var releasesGrantedByReleaseRoles = await releasesGrantedByReleaseRolesQueryable
            .Select(userReleaseRole => userReleaseRole.ReleaseVersion)
            .ToListAsync();

        var publications = new List<Publication>();

        // Add publication view models for the Publications granted directly via Publication roles
        publications.AddRange(await publicationsGrantedByPublicationRole
            .SelectAsync(async publication =>
                await HydratePublication(_context.Publications)
                    .FirstAsync(p => p.Id == publication.Id)));

        // Add publication view models for the Publications granted indirectly via Release roles
        publications.AddRange(await releasesGrantedByReleaseRoles
            .GroupBy(releaseVersion => releaseVersion.Release.Publication)
            .Where(publicationWithReleases =>
            {
                // Don't include a publication that's already been included by Publication roles
                var publication = publicationWithReleases.Key;
                return !publicationIdsGrantedByPublicationRole.Contains(publication.Id);
            })
            .SelectAsync(async publicationWithReleases =>
            {
                var publication = publicationWithReleases.Key;
                return await HydratePublication(_context.Publications)
                    .AsNoTracking()
                    .FirstAsync(p => p.Id == publication.Id);
            }));

        return publications;
    }
}
