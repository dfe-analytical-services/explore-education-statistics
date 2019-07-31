using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.ModelMappers;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using UserId = System.Guid;
using TopicId = System.Guid;
using PublicationId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ApplicationDbContext _context;

        public PublicationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Publication Get(UserId id)
        {
            return _context.Publications.FirstOrDefault(x => x.Id == id);
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
        public List<PublicationViewModel> GetByTopicAndUser(TopicId topicId, UserId userId)
        {
            var publications = _context.Publications.Where(p => p.TopicId == topicId)
                .Include(p => p.Contact)
                .Include(p => p.Releases)
                .Include(p => p.Methodology)
                .ToList();

            return PublicationViewModelMapper.Map<List<PublicationViewModel>>(publications);
        }

        public async Task<Either<ValidationResult, PublicationViewModel>> CreatePublication(CreatePublicationViewModel publication)
        {
            if (_context.Publications.Any(p => p.Slug == publication.Slug))
            {
                return ValidationResult("Slug", "Slug is not unique");
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
            var publication = await _context.Publications.Include(p => p.Methodology)
                .Include(p => p.Contact)
                .Include(p => p.Releases)
                .Where(p => p.Id == publicationId)
                .FirstOrDefaultAsync();
            return PublicationViewModelMapper.Map<PublicationViewModel>(publication);
        }
    }
}