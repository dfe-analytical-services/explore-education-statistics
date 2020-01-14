using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IPublicationRepository _publicationRepository;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;

        public PublicationService(ContentDbContext context, IMapper mapper, IUserService userService, 
            IPublicationRepository publicationRepository, IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _publicationRepository = publicationRepository;
            _persistenceHelper = persistenceHelper;
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

        public Task<List<MyPublicationViewModel>> GetMyPublicationsAndReleasesByTopicAsync(Guid topicId)
        {
            return _userService
                .CheckCanViewAllReleases()
                .OnSuccess(() => 
                    _publicationRepository.GetAllPublicationsForTopicAsync(topicId))
                .OrElse(() => _publicationRepository.
                    GetPublicationsForTopicRelatedToUserAsync(topicId, _userService.GetUserId()));
        }

        public async Task<Either<ActionResult, MyPublicationViewModel>> CreatePublicationAsync(CreatePublicationViewModel publication)
        {
            return await _persistenceHelper
                .CheckEntityExists<Topic>(publication.TopicId)
                .OnSuccess(_userService.CheckCanCreatePublicationForTopic)
                .OnSuccess(async _ =>
                {
                    if (_context.Publications.Any(p => p.Slug == publication.Slug))
                    {
                        return ValidationActionResult<MyPublicationViewModel>(SlugNotUnique);
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
                });
        }

        public async Task<MyPublicationViewModel> GetViewModelAsync(Guid publicationId)
        {
            var publication = await _context.Publications
                .Where(p => p.Id == publicationId)
                .HydratePublicationForPublicationViewModel()
                .FirstOrDefaultAsync();
            return _mapper.Map<MyPublicationViewModel>(publication);
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