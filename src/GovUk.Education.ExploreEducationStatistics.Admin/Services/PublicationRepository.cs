#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.EntityFrameworkCore;

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
        var authorizedPublicationIds = await GetAuthorizedPublicationIds(userId, themeId);

        if (authorizedPublicationIds.Count == 0)
        {
            return [];
        }

        return await context
            .Publications.AsNoTracking()
            .Where(p => authorizedPublicationIds.Contains(p.Id))
            .Include(p => p.Theme)
            .ToListAsync();
    }

    private async Task<HashSet<Guid>> GetAuthorizedPublicationIds(Guid userId, Guid? themeId = null)
    {
        var publicationIdsFromPublicationRolesQueryable = userPublicationRoleRepository
            .Query()
            .WhereForUser(userId)
            .WhereRolesIn([PublicationRole.Owner, PublicationRole.Allower]);

        var publicationIdsFromReleaseRolesQueryable = userReleaseRoleRepository
            .Query()
            .WhereForUser(userId)
            .WhereRolesNotIn(ReleaseRole.PrereleaseViewer);

        if (themeId.HasValue)
        {
            publicationIdsFromPublicationRolesQueryable = publicationIdsFromPublicationRolesQueryable.Where(upr =>
                upr.Publication.ThemeId == themeId.Value
            );

            publicationIdsFromReleaseRolesQueryable = publicationIdsFromReleaseRolesQueryable.Where(urr =>
                urr.ReleaseVersion.Release.Publication.ThemeId == themeId.Value
            );
        }

        var publicationIdsFromPublicationRoles = publicationIdsFromPublicationRolesQueryable.Select(upr =>
            upr.PublicationId
        );

        var publicationIdsFromReleaseRoles = publicationIdsFromReleaseRolesQueryable.Select(urr =>
            urr.ReleaseVersion.Release.PublicationId
        );

        var allDistinctPublicationIds = await publicationIdsFromPublicationRoles
            .Union(publicationIdsFromReleaseRoles)
            .ToListAsync();

        return [.. allDistinctPublicationIds];
    }
}
