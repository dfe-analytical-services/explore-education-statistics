#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.PublicationService;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PublicationRepository(
    ContentDbContext context,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IUserReleaseRoleRepository userReleaseRoleRepository
) : Content.Model.Repository.PublicationRepository(context), IPublicationRepository
{
    public IQueryable<Publication> QueryPublicationsForTheme(Guid? themeId = null)
    {
        return context.Publications.Where(publication => themeId == null || publication.ThemeId == themeId);
    }

    public async Task<List<Publication>> ListPublicationsForUser(Guid userId, Guid? themeId = null)
    {
        var publicationsGrantedByPublicationRoleQueryable = userPublicationRoleRepository
            .Query()
            .WhereForUser(userId)
            .WhereRolesIn([PublicationRole.Owner, PublicationRole.Allower]);

        if (themeId.HasValue)
        {
            publicationsGrantedByPublicationRoleQueryable = publicationsGrantedByPublicationRoleQueryable.Where(
                userPublicationRole => userPublicationRole.Publication.ThemeId == themeId.Value
            );
        }

        var publicationsGrantedByPublicationRole = await publicationsGrantedByPublicationRoleQueryable
            .Select(userPublicationRole => userPublicationRole.Publication)
            .ToListAsync();

        var publicationIdsGrantedByPublicationRole = publicationsGrantedByPublicationRole
            .Select(publication => publication.Id)
            .ToList();

        var releasesGrantedByReleaseRolesQueryable = userReleaseRoleRepository
            .Query()
            .Include(urr => urr.ReleaseVersion)
            .ThenInclude(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .WhereForUser(userId)
            .WhereRolesNotIn(ReleaseRole.PrereleaseViewer);

        if (themeId.HasValue)
        {
            releasesGrantedByReleaseRolesQueryable = releasesGrantedByReleaseRolesQueryable.Where(userReleaseRole =>
                userReleaseRole.ReleaseVersion.Release.Publication.ThemeId == themeId.Value
            );
        }

        var releasesGrantedByReleaseRoles = await releasesGrantedByReleaseRolesQueryable
            .Select(userReleaseRole => userReleaseRole.ReleaseVersion)
            .ToListAsync();

        var publications = new List<Publication>();

        // Add publication view models for the Publications granted directly via Publication roles
        publications.AddRange(
            await publicationsGrantedByPublicationRole.SelectAsync(async publication =>
                await HydratePublication(context.Publications).FirstAsync(p => p.Id == publication.Id)
            )
        );

        // Add publication view models for the Publications granted indirectly via Release roles
        publications.AddRange(
            await releasesGrantedByReleaseRoles
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
                    return await HydratePublication(context.Publications)
                        .AsNoTracking()
                        .FirstAsync(p => p.Id == publication.Id);
                })
        );

        return publications;
    }
}
