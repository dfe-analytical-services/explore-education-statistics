using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
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

        public Task<List<MyPublicationViewModel>> GetAllPublicationsForTopicAsync(Guid topicId)
        {
            return _context
                .Publications
                .HydratePublicationForPublicationViewModel()
                .Where(publication => publication.TopicId == topicId)
                .Select(publication => _mapper.Map<MyPublicationViewModel>(publication))
                .ToListAsync();
        }

        public async Task<List<MyPublicationViewModel>> GetPublicationsForTopicRelatedToUserAsync(Guid topicId, Guid userId)
        {
            var userReleasesForTopic = await _context
                .UserReleaseRoles
                .Include(r => r.Release)
                .ThenInclude(release => release.Publication)
                .ThenInclude(publication => publication.Topic)
                .Where(r => r.UserId == userId)
                .Select(r => r.Release)
                .Where(release => release.Publication.TopicId == topicId)
                .ToListAsync();

            var userReleasesByPublication = new Dictionary<Publication, List<Release>>();

            foreach (var publication in userReleasesForTopic.Select(release => release.Publication).Distinct())
            {
                var releasesForPublication = userReleasesForTopic.FindAll(release => release.Publication == publication);
                userReleasesByPublication.Add(publication, releasesForPublication);
            }

            return userReleasesByPublication
                .Select(publicationWithReleases =>
                {
                    var (publication, releases) = publicationWithReleases;

                    return new MyPublicationViewModel
                    {
                        Id = publication.Id,
                        Contact = publication.Contact,
                        Methodology = _mapper.Map<MethodologyViewModel>(publication.Methodology),
                        Releases = releases.Select(release => _mapper.Map<ReleaseViewModel>(release)).ToList(),
                        Title = publication.Title,
                        NextUpdate = publication.NextUpdate,
                        ThemeId = publication.Topic.ThemeId
                    };
                })
                .ToList();
        }
    }
}