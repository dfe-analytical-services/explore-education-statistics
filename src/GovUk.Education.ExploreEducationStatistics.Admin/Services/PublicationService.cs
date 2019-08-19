using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using UserId = System.Guid;
using TopicId = System.Guid;
using PublicationId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PublicationService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Publication> GetAsync(PublicationId id)
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

        // TODO it maybe necessary to add authorisation to this method
        public async Task<List<PublicationViewModel>> GetByTopicAndUserAsync(TopicId topicId, UserId userId)
        {
            var publications = await _context.Publications
                .Where(p => p.TopicId == topicId)
                .HydratePublicationForPublicationViewModel()
                .ToListAsync();

            return _mapper.Map<List<PublicationViewModel>>(publications);
        }

        public async Task<Either<ValidationResult, PublicationViewModel>> CreatePublicationAsync(CreatePublicationViewModel publication)
        {
            if (_context.Publications.Any(p => p.Slug == publication.Slug))
            {
                return ValidationResult(SlugNotUnique);
            }
            
            var saved = _context.Publications.Add(new Publication
            {
                Id = UserId.NewGuid(),
                ContactId = publication.ContactId,
                Title = publication.Title,
                TopicId = publication.TopicId,
                MethodologyId = publication.MethodologyId,
                Slug = publication.Slug
            });
            _context.SaveChanges();
            return await GetViewModelAsync(saved.Entity.Id);
            
        }

        public async Task<PublicationViewModel> GetViewModelAsync(PublicationId publicationId)
        {
            var publication = await _context.Publications
                .Where(p => p.Id == publicationId)
                .HydratePublicationForPublicationViewModel()
                .FirstOrDefaultAsync();
            return _mapper.Map<PublicationViewModel>(publication);
        }
    }
    
    
    public static class PublicationLinqExtensions
    {
        public static IQueryable<Publication> HydratePublicationForPublicationViewModel(this IQueryable<Publication> values)
        {
            return values.Include(p => p.Contact)
                .Include(p => p.Releases).ThenInclude(r => r.Type)
                .Include(r => r.Releases).ThenInclude(r => r.ReleaseSummary).ThenInclude(rs => rs.Versions)
                .Include(p => p.Methodology);
        }
    }
}