using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

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
            var results = await _context
                .Publications
                .HydratePublicationForPublicationViewModel()
                .Where(publication => publication.TopicId == topicId)
                .ToListAsync();
                
            return results
                .Select(publication => _mapper.Map<MyPublicationViewModel>(publication))
                .ToList();
        }

        public async Task<List<MyPublicationViewModel>> GetPublicationsForTopicRelatedToUserAsync(Guid topicId, Guid userId)
        {
            var userReleasesForTopic = await _context
                .UserReleaseRoles
                .Include(r => r.Release)
                .ThenInclude(release => release.Publication)
                .ThenInclude(publication => publication.Topic)
                .Where(r => r.UserId == userId && r.Release.Publication.TopicId == topicId)
                .Select(r => r.Release)
                .Distinct()
                .Where(release => release.Publication.TopicId == topicId)
                .ToListAsync();

            var userReleasesByPublication = new Dictionary<Publication, List<Release>>();

            foreach (var publication in userReleasesForTopic
                .Select(release => release.Publication)
                .Distinct())
            {
                var releasesForPublication = userReleasesForTopic.FindAll(release => release.Publication == publication);
                userReleasesByPublication.Add(publication, releasesForPublication);
            }

            return userReleasesByPublication
                .Select(publicationWithReleases =>
                {
                    var (publication, releases) = publicationWithReleases;
                    publication.Releases = releases;
                    return _mapper.Map<MyPublicationViewModel>(publication);
                })
                .ToList();
        }
    }
}