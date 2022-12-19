#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.PublicationService;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PublicationRepository : Content.Model.Repository.PublicationRepository, IPublicationRepository
    {
        private readonly ContentDbContext _context;

        public PublicationRepository(ContentDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Publication> QueryPublicationsForTopic(Guid? topicId = null)
        {
            return _context.Publications
                .Where(publication => topicId == null || publication.TopicId == topicId);
        }

        public async Task<List<Publication>> ListPublicationsForUser(
            Guid userId,
            Guid? topicId = null)
        {
            var publicationsGrantedByPublicationRoleQueryable = _context
                .UserPublicationRoles
                .AsQueryable()
                .Where(userPublicationRole => userPublicationRole.UserId == userId &&
                                              ListOf(PublicationRole.Owner, PublicationRole.Approver)
                                                  .Contains(userPublicationRole.Role));

            if (topicId.HasValue)
            {
                publicationsGrantedByPublicationRoleQueryable =
                    publicationsGrantedByPublicationRoleQueryable.Where(userPublicationRole =>
                        userPublicationRole.Publication.TopicId == topicId.Value);
            }

            var publicationsGrantedByPublicationRole = await publicationsGrantedByPublicationRoleQueryable
                .Select(userPublicationRole => userPublicationRole.Publication)
                .ToListAsync();

            var publicationIdsGrantedByPublicationRole = publicationsGrantedByPublicationRole
                .Select(publication => publication.Id)
                .ToList();

            var releasesGrantedByReleaseRolesQueryable = _context.UserReleaseRoles
                .Include(userReleaseRole => userReleaseRole.Release.Publication)
                .Where(userReleaseRole => userReleaseRole.UserId == userId &&
                                          userReleaseRole.Role != ReleaseRole.PrereleaseViewer);

            if (topicId.HasValue)
            {
                releasesGrantedByReleaseRolesQueryable =
                    releasesGrantedByReleaseRolesQueryable.Where(userReleaseRole =>
                        userReleaseRole.Release.Publication.TopicId == topicId.Value);
            }

            var releasesGrantedByReleaseRoles = await releasesGrantedByReleaseRolesQueryable
                .Select(userReleaseRole => userReleaseRole.Release)
                .ToListAsync();

            var publications = new List<Publication>();

            // Add publication view models for the Publications granted directly via Publication roles
            publications.AddRange(await publicationsGrantedByPublicationRole
                .SelectAsync(async publication =>
                    await HydratePublication(_context.Publications)
                        .FirstAsync(p => p.Id == publication.Id)));

            // Add publication view models for the Publications granted indirectly via Release roles
            publications.AddRange(await releasesGrantedByReleaseRoles
                .GroupBy(release => release.Publication)
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

        public async Task<List<Release>> ListActiveReleases(Guid publicationId)
        {
            var publication = await _context.Publications
                .Include(p => p.Releases)
                .AsAsyncEnumerable()
                .SingleAsync(p => p.Id == publicationId);

            return publication.ListActiveReleases();
        }

        public async Task<Release?> GetLatestReleaseForPublication(Guid publicationId)
        {
            var publication = await _context
                .Publications
                .Include(p => p.Releases)
                .SingleAsync(p => p.Id == publicationId);

            return publication.LatestRelease();
        }
    }
}
