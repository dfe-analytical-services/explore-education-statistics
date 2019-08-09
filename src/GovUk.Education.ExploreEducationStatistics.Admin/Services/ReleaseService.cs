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
using PublicationId = System.Guid;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ReleaseService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public Release Get(PublicationId id)
        {
            return _context.Releases.FirstOrDefault(x => x.Id == id);
        }
        
        public Release Get(string slug)
        {
            return _context.Releases.FirstOrDefault(x => x.Slug == slug);
        }
        
        public async Task<Release> GetAsync(ReleaseId id)
        {
            return await _context.Releases.FirstOrDefaultAsync(x => x.Id == id);
        }

        public List<Release> List()
        {
            return _context.Releases.ToList();
        }

        // TODO Authorisation will be required when users are introduced
        public async Task<ReleaseViewModel> GetReleaseForIdAsync(ReleaseId id)
        {
            var release = await _context.Releases
                .Where(x => x.Id == id)
                .HydrateReleaseForReleaseViewModel()
                .FirstOrDefaultAsync(); 
            return _mapper.Map<ReleaseViewModel>(release);
        }

        // TODO Authorisation will be required when users are introduced
        public async Task<Either<ValidationResult, ReleaseViewModel>> CreateReleaseAsync(CreateReleaseViewModel createRelease)
        {
            if (_context.Releases.Any(r => r.Slug == createRelease.Slug))
            {
                return ValidationResult(SlugNotUnique);
            }
            var order = OrderForNextReleaseOnPublication(createRelease.PublicationId);
            var content = TemplateFromRelease(createRelease.TemplateReleaseId);
            var release = _mapper.Map<Release>(createRelease);
            release.Content = content;
            release.Order = order;
            var saved = _context.Releases.Add(release);
            await _context.SaveChangesAsync();
            return await GetReleaseForIdAsync(saved.Entity.Id);
        }
        
        // TODO Authorisation will be required when users are introduced
        public async Task<EditReleaseSummaryViewModel> GetReleaseSummaryAsync(ReleaseId releaseId)
        {
            var release = await _context.Releases.FirstOrDefaultAsync(r => r.Id == releaseId);
            return _mapper.Map<EditReleaseSummaryViewModel>(release);
        }
        
        // TODO Authorisation will be required when users are introduced
        public async Task<Either<ValidationResult, ReleaseViewModel>> EditReleaseSummaryAsync(EditReleaseSummaryViewModel model)
        {
            var withSameSlug = _context.Releases.Where(r => r.Slug == model.Slug);
            if (withSameSlug.Any() && (withSameSlug.Count() > 1 || withSameSlug.First().Id != model.Id))
            {
                return ValidationResult(SlugNotUnique);
            }
            var release = await _context.Releases.FirstOrDefaultAsync(r => r.Id == model.Id);
            _context.Releases.Update(release);
            _mapper.Map(model, release);
            await _context.SaveChangesAsync();
            return await GetReleaseForIdAsync(model.Id);
        }
        
        // TODO Authorisation will be required when users are introduced
        public async Task<List<ReleaseViewModel>> GetReleasesForPublicationAsync(PublicationId publicationId)
        {
            var release = await _context.Releases
                .Where(r => r.Publication.Id == publicationId)
                .HydrateReleaseForReleaseViewModel()
                .ToListAsync();
            return _mapper.Map<List<ReleaseViewModel>>(release);
        }
        
        

        private int OrderForNextReleaseOnPublication(PublicationId publicationId)
        {
            var publication = _context.Publications.Include(p => p.Releases)
                .Single(p => p.Id == publicationId);
            return publication.Releases.Select(r => r.Order).DefaultIfEmpty().Max() + 1;
        }

        private List<ContentSection> TemplateFromRelease(ReleaseId? releaseId)
        {
            if (releaseId.HasValue)
            {
                var templateContent = Get(releaseId.Value).Content;
                if (templateContent != null)
                {
                    return templateContent.Select(c => new ContentSection
                    {
                        Caption = c.Caption,
                        Heading = c.Heading,
                        Order = c.Order,
                        // TODO in future do we want to copy across more? Is it possible to do so?
                    }).ToList();
                }
            }
            return new List<ContentSection>();
        }
    }
    public static class ReleaseLinqExtensions
    {
        public static IQueryable<Release> HydrateReleaseForReleaseViewModel(this IQueryable<Release> values)
        {
            // Require publication / release / contact / type graph to be able to work out:
            // If the release is the latest
            // The contact
            // The type
            return values.Include(r => r.Publication)
                .Include(r => r.Publication.Releases) // Back refs required to work out latest
                .Include(r => r.Publication.Contact) 
                .Include(r => r.Type);
        }
    }
}