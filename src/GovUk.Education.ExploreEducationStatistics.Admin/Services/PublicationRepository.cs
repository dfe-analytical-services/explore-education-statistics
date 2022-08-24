#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.PublicationService;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PublicationRepository : IPublicationRepository
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;

        public PublicationRepository(ContentDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<Publication>> GetAllPublicationsForTopic(Guid topicId)
        {
            return await HydratePublicationForPublicationViewModel(_context.Publications)
                .Where(publication => publication.TopicId == topicId)
                .ToListAsync();
        }

        public async Task<List<Publication>> GetPublicationsForTopicRelatedToUser(
            Guid topicId,
            Guid userId)
        {
            var publicationsGrantedByPublicationRole = await _context
                .UserPublicationRoles
                .AsQueryable()
                .Where(userPublicationRole => userPublicationRole.UserId == userId &&
                                              userPublicationRole.Publication.TopicId == topicId &&
                                              ListOf(PublicationRole.Owner, PublicationRole.ReleaseApprover)
                                                  .Contains(userPublicationRole.Role))
                .Select(userPublicationRole => userPublicationRole.Publication)
                .ToListAsync();

            var publicationIdsGrantedByPublicationRole = publicationsGrantedByPublicationRole
                .Select(publication => publication.Id)
                .ToList();

            var releasesGrantedByReleaseRoles = await _context.UserReleaseRoles
                .Include(userReleaseRole => userReleaseRole.Release.Publication)
                .Where(userReleaseRole => userReleaseRole.UserId == userId &&
                                          userReleaseRole.Release.Publication.TopicId == topicId &&
                                          userReleaseRole.Role != ReleaseRole.PrereleaseViewer)
                .Select(userReleaseRole => userReleaseRole.Release)
                .ToListAsync();

            var publications = new List<Publication>();

            // Add publication view models for the Publications granted directly via Publication roles
            publications.AddRange(await publicationsGrantedByPublicationRole
                .SelectAsync(async publication =>
                    // Include all Releases of the Publication unconditionally
                    await HydratePublicationForPublicationViewModel(_context.Publications)
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
                    // Only include Releases of the Publication that the user has access to
                    var releaseIds = publicationWithReleases.Select(r => r.Id);
                    return await GetPublicationWithFilteredReleases(publication.Id, releaseIds);
                }));

            return publications;
        }

        public async Task<Publication> GetPublicationForUser(Guid publicationId, Guid userId)
        {
            var userReleaseIdsForPublication = await _context
                .UserReleaseRoles
                .Include(r => r.Release)
                .Where(r => r.UserId == userId
                            && r.Release.PublicationId == publicationId
                            && r.Role != ReleaseRole.PrereleaseViewer)
                .Select(r => r.ReleaseId)
                .Distinct()
                .ToListAsync();

            return await GetPublicationWithFilteredReleases(publicationId, userReleaseIdsForPublication);
        }

        public async Task<Publication> GetPublicationWithAllReleases(Guid publicationId)
        {
            return await HydratePublicationForPublicationViewModel(_context.Publications)
                .FirstAsync(p => p.Id == publicationId);
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

        private async Task<Publication> GetPublicationWithFilteredReleases(Guid publicationId,
            IEnumerable<Guid> releaseIds)
        {
            // Use AsNoTracking:
            // - There should be no need to track changes as this method is only used to create a view model.
            // - We also mutate Publication to filter only Releases visible to the user and this mutation shouldn't
            //   be tracked otherwise it will affect any other code retrieving Publication from the context that's
            //   expecting an unfiltered list of Releases. Entities tracked by the context can be returned immediately
            //   without making a request to the database, e.g. when using DbContext.Find/FindAsync.
            var hydratedPublication = await HydratePublicationForPublicationViewModel(_context.Publications)
                .AsNoTracking()
                .FirstAsync(p => p.Id == publicationId);

            // TODO EES-2624 Don't mutate Publication
            hydratedPublication.Releases = hydratedPublication.Releases
                .FindAll(r => releaseIds.Contains(r.Id));

            return hydratedPublication;
        }

        public bool IsSuperseded(Publication publication)
        {
            return publication.SupersededById != null
                   // To be superseded, superseding publication must have Live release
                   && _context.Releases
                       .Any(r => r.PublicationId == publication.SupersededById
                                 && r.Published.HasValue && DateTime.UtcNow >= r.Published.Value);
        }
    }
}
