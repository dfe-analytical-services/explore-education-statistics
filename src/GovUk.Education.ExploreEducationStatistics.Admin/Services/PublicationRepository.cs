#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.PublicationService;

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

        public async Task<List<MyPublicationViewModel>> GetAllPublicationsForTopic(Guid topicId)
        {
            var results = await HydratePublicationForPublicationViewModel(_context.Publications)
                .Where(publication => publication.TopicId == topicId)
                .ToListAsync();

            return results
                .Select(publication => _mapper.Map<MyPublicationViewModel>(publication))
                .ToList();
        }

        public async Task<List<MyPublicationViewModel>> GetPublicationsForTopicRelatedToUser(Guid topicId,
            Guid userId)
        {
            var publicationsGrantedByPublicationOwnerRole = await _context.UserPublicationRoles
                .AsQueryable()
                .Where(userPublicationRole => userPublicationRole.UserId == userId &&
                                              userPublicationRole.Publication.TopicId == topicId &&
                                              userPublicationRole.Role == PublicationRole.Owner)
                .Select(userPublicationRole => userPublicationRole.Publication)
                .ToListAsync();

            var publicationIdsGrantedByPublicationOwnerRole = publicationsGrantedByPublicationOwnerRole
                .Select(publication => publication.Id)
                .ToList();

            var releasesGrantedByReleaseRoles = await _context.UserReleaseRoles
                .Include(userReleaseRole => userReleaseRole.Release.Publication)
                .Where(userReleaseRole => userReleaseRole.UserId == userId &&
                                          userReleaseRole.Release.Publication.TopicId == topicId &&
                                          userReleaseRole.Role != ReleaseRole.PrereleaseViewer)
                .Select(userReleaseRole => userReleaseRole.Release)
                .ToListAsync();

            var publicationViewModels = new List<MyPublicationViewModel>();

            // Add publication view models for the Publications granted by the Publication Owner role
            publicationViewModels.AddRange(await publicationsGrantedByPublicationOwnerRole
                .SelectAsync(async publication =>
                    // Include all Releases of the Publication unconditionally
                    await GetPublicationWithAllReleases(publication.Id)));

            // Add publication view models for the Publications granted indirectly via Release roles
            publicationViewModels.AddRange(await releasesGrantedByReleaseRoles
                .GroupBy(release => release.Publication)
                .Where(publicationWithReleases =>
                {
                    // Don't include a publication that's already been included by Publication Owner role
                    var publication = publicationWithReleases.Key;
                    return !publicationIdsGrantedByPublicationOwnerRole.Contains(publication.Id);
                })
                .SelectAsync(async publicationWithReleases =>
                {
                    var publication = publicationWithReleases.Key;
                    // Only include Releases of the Publication that the user has access to
                    var releaseIds = publicationWithReleases.Select(r => r.Id);
                    return await GetPublicationWithFilteredReleases(publication.Id, releaseIds);
                }));

            return publicationViewModels
                .OrderBy(model => model.Title)
                .ToList();
        }

        public async Task<MyPublicationViewModel> GetPublicationForUser(Guid publicationId, Guid userId)
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

        public async Task<MyPublicationViewModel> GetPublicationWithAllReleases(Guid publicationId)
        {
            var hydratedPublication = await HydratePublicationForPublicationViewModel(_context.Publications)
                .FirstAsync(p => p.Id == publicationId);

            return _mapper.Map<MyPublicationViewModel>(hydratedPublication);
        }

        public async Task<List<Release>> GetLatestVersionsOfAllReleases(Guid publicationId)
        {
            var publication = await _context.Publications
                .Include(p => p.Releases)
                .AsAsyncEnumerable()
                .SingleAsync(p => p.Id == publicationId);

            return publication.Releases
                .Where(r => r.Publication.IsLatestVersionOfRelease(r.Id))
                .ToList();
        }

        public async Task<Release?> GetLatestReleaseForPublication(Guid publicationId)
        {
            var publication = await _context
                .Publications
                .Include(p => p.Releases)
                .SingleAsync(p => p.Id == publicationId);

            return publication.LatestRelease();
        }

        private async Task<MyPublicationViewModel> GetPublicationWithFilteredReleases(Guid publicationId,
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

            return _mapper.Map<MyPublicationViewModel>(hydratedPublication);
        }
    }
}
