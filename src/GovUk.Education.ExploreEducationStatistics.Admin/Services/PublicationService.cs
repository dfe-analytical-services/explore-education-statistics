using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public PublicationService(ContentDbContext context, IMapper mapper, IUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<Publication> GetAsync(Guid id)
        {
            return await _context.Publications.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Publication Get(string slug)
        {
            return _context.Publications.FirstOrDefault(x => x.Slug == slug);
        }

        public List<Publication> List()
        {
            return _context.Publications.ToList();
        }

        public Task<List<PublicationViewModel>> GetMyPublicationsAndReleasesByTopicAsync(Guid topicId)
        {
            return _userService
                .CheckCanViewAllReleases()
                .OnSuccess(() => GetAllPublicationsForTopicAsync(topicId))
                .OrElse(() => GetPublicationsForTopicRelatedToMeAsync(topicId));
        }

        public async Task<Either<ValidationResult, PublicationViewModel>> CreatePublicationAsync(CreatePublicationViewModel publication)
        {
            if (_context.Publications.Any(p => p.Slug == publication.Slug))
            {
                return ValidationResult(SlugNotUnique);
            }
            
            var saved = _context.Publications.Add(new Publication
            {
                Id = Guid.NewGuid(),
                ContactId = publication.ContactId,
                Title = publication.Title,
                TopicId = publication.TopicId,
                MethodologyId = publication.MethodologyId,
                Slug = publication.Slug
            });
            _context.SaveChanges();
            return await GetViewModelAsync(saved.Entity.Id);
            
        }

        public async Task<PublicationViewModel> GetViewModelAsync(Guid publicationId)
        {
            var publication = await _context.Publications
                .Where(p => p.Id == publicationId)
                .HydratePublicationForPublicationViewModel()
                .FirstOrDefaultAsync();
            return _mapper.Map<PublicationViewModel>(publication);
        }

        private Task<List<PublicationViewModel>> GetAllPublicationsForTopicAsync(Guid topicId)
        {
            return _context
                .Publications
                .HydratePublicationForPublicationViewModel()
                .Where(publication => publication.TopicId == topicId)
                .Select(publication => _mapper.Map<PublicationViewModel>(publication))
                .ToListAsync();
        }

        private async Task<List<PublicationViewModel>> GetPublicationsForTopicRelatedToMeAsync(Guid topicId)
        {
            var userId = _userService.GetUserId();
            
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

                    return new PublicationViewModel
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
    
    
    public static class PublicationLinqExtensions
    {
        public static IQueryable<Publication> HydratePublicationForPublicationViewModel(this IQueryable<Publication> values)
        {
            return values.Include(p => p.Contact)
                .Include(p => p.Releases)
                .ThenInclude(r => r.Type)
                .Include(p => p.Methodology)
                .Include(p => p.Topic);
        }
    }
}