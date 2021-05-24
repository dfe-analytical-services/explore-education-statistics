using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.PublicationService;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

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

        public async Task<List<MyPublicationViewModel>> GetAllPublicationsForTopicAsync(Guid topicId)
        {
            var results = await HydratePublicationForPublicationViewModel(_context.Publications)
                .Where(publication => publication.TopicId == topicId)
                .ToListAsync();
                
            return results
                .Select(publication => _mapper.Map<MyPublicationViewModel>(publication))
                .ToList();
        }

        public async Task<List<MyPublicationViewModel>> GetPublicationsForTopicRelatedToUserAsync(Guid topicId, Guid userId)
        {
            var userReleasesForTopicFromReleases = await _context.UserReleaseRoles
                .Include(userReleaseRole => userReleaseRole.Release.Publication)
                .Where(userReleaseRole => userReleaseRole.UserId == userId &&
                                          userReleaseRole.Release.Publication.TopicId == topicId &&
                                          userReleaseRole.Role != ReleaseRole.PrereleaseViewer)
                .Select(userReleaseRole => userReleaseRole.Release)
                .ToListAsync();

            var userReleasesForTopicFromPublications = await _context.UserPublicationRoles
                .Where(userPublicationRole => userPublicationRole.UserId == userId &&
                                              userPublicationRole.Publication.TopicId == topicId &&
                                              userPublicationRole.Role == Owner)
                .SelectMany(userPublicationRole => userPublicationRole.Publication.Releases)
                .Include(release => release.Publication)
                .ToListAsync();

            var userReleasesForTopic = userReleasesForTopicFromReleases
                .Concat(userReleasesForTopicFromPublications)
                .Distinct()
                .ToList();

            var userReleasesByPublication = new Dictionary<Publication, List<Release>>();

            foreach (var publication in userReleasesForTopic
                .Select(release => release.Publication)
                .Distinct())
            {
                var releasesForPublication = userReleasesForTopic
                    .FindAll(release => release.PublicationId == publication.Id);
                userReleasesByPublication.Add(publication, releasesForPublication);
            }

            return userReleasesByPublication
                .Select(publicationWithReleases =>
                {
                    var (publication, releases) = publicationWithReleases;
                    var releaseIds = releases.Select(r => r.Id);
                    
                    var hydratedPublication = 
                        HydratePublicationForPublicationViewModel(_context.Publications)
                        .First(p => p.Id == publication.Id);
                    
                    hydratedPublication.Releases = hydratedPublication
                        .Releases
                        .FindAll(r => releaseIds.Contains(r.Id));
                    
                    return _mapper.Map<MyPublicationViewModel>(hydratedPublication);
                })
                .ToList();
        }

        public async Task<MyPublicationViewModel> GetPublicationForUser(Guid publicationId, Guid userId)
        {
            var userReleaseIdsForPublication = await _context
                .UserReleaseRoles
                .Include(r => r.Release)
                .Where(r => r.UserId == userId && r.Release.PublicationId == publicationId && r.Role != ReleaseRole.PrereleaseViewer)
                .Select(r => r.ReleaseId)
                .Distinct()
                .ToListAsync();

            var hydratedPublication =
                HydratePublicationForPublicationViewModel(_context.Publications)
                    .First(p => p.Id == publicationId);
            
            hydratedPublication.Releases = hydratedPublication
                .Releases
                .FindAll(r => userReleaseIdsForPublication.Contains(r.Id));
            
            return _mapper.Map<MyPublicationViewModel>(hydratedPublication);
        }

        public async Task<MyPublicationViewModel> GetPublicationWithAllReleases(Guid publicationId)
        {
            var hydratedPublication = await HydratePublicationForPublicationViewModel(_context.Publications)
                .FirstAsync(p => p.Id == publicationId);

            return _mapper.Map<MyPublicationViewModel>(hydratedPublication);
        }
    }
}
