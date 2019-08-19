using System;
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
        
        public async Task<Release> GetAsync(ReleaseId id)
        {
            return await _context.Releases.FirstOrDefaultAsync(x => x.Id == id);
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
        public async Task<Either<ValidationResult, ReleaseViewModel>> CreateReleaseAsync(
            CreateReleaseViewModel createRelease)
        {
            return await ValidateReleaseSlugUniqueToPublication(createRelease.Slug, createRelease.PublicationId)
                .OnSuccess(async () =>
                {
                    var order = OrderForNextReleaseOnPublication(createRelease.PublicationId);
                    var content = await TemplateFromRelease(createRelease.TemplateReleaseId);
                    var releaseSummary = _mapper.Map<ReleaseSummaryVersion>(createRelease);
                    releaseSummary.Created = DateTime.Now;
                    var saved = _context.Releases.Add(new Release
                    {
                        PublicationId = createRelease.PublicationId,
                        ReleaseSummary = new ReleaseSummary
                        {
                            Versions = new List<ReleaseSummaryVersion>()
                            {
                                releaseSummary
                            }
                        },
                        Content = content,
                        Order = order,
                    });
                    
                    await _context.SaveChangesAsync();
                    return await GetReleaseForIdAsync(saved.Entity.Id);
                });
        }

        // TODO Authorisation will be required when users are introduced
        public async Task<EditReleaseSummaryViewModel> GetReleaseSummaryAsync(Guid releaseId)
        {
            var release = await _context.Releases
                .Where(r => r.Id == releaseId)
                .HydrateReleaseForReleaseViewModel()
                .FirstOrDefaultAsync();
            return _mapper.Map<EditReleaseSummaryViewModel>(release.ReleaseSummary);
        }

        // TODO Authorisation will be required when users are introduced
        public async Task<Either<ValidationResult, ReleaseViewModel>> EditReleaseSummaryAsync(
            EditReleaseSummaryViewModel model)
        {
            var publication = await GetAsync(model.Id);
            return await ValidateReleaseSlugUniqueToPublication(model.Slug, publication.Id, model.Id)
                .OnSuccess(async () =>
                {
                    var release = await _context.Releases
                        .Where(r => r.Id == model.Id)
                        .HydrateReleaseForReleaseViewModel()
                        .FirstOrDefaultAsync();
                    var currentSummary = release.ReleaseSummary.Current;
                    var newSummary = _mapper.Map<ReleaseSummaryVersion>(model);
                    newSummary.Created = DateTime.Now;
                    newSummary.Summary = currentSummary.Summary;
                    release.ReleaseSummary.Versions.Add(newSummary);
                    _context.Update(release);
                    return await GetReleaseForIdAsync(model.Id);
                });
        }

        // TODO Authorisation will be required when users are introduced
        public async Task<List<ReleaseViewModel>> GetReleasesForPublicationAsync(Guid publicationId)
        {
            var release = await _context.Releases
                .Where(r => r.Publication.Id == publicationId)
                .HydrateReleaseForReleaseViewModel()
                .ToListAsync();

            return _mapper.Map<List<ReleaseViewModel>>(release);
        }

        private async Task<Either<ValidationResult, bool>> ValidateReleaseSlugUniqueToPublication(string slug,
            PublicationId publicationId, ReleaseId? releaseId = null)
        {
            var slugMatches = _context.Releases
                .Where(r => r.PublicationId == publicationId && r.ReleaseSummary.ReleaseId != releaseId)
                .Select(r => r.ReleaseSummary)
                .Include(rs => rs.Versions)
                .ToList() // Required as the following operation are not expressible in sql as they query computed fields
                .Any(rs => rs.Current.Slug == slug);
            if (slugMatches)
            {
                return ValidationResult(SlugNotUnique);
            }

            return true;
        }

        private int OrderForNextReleaseOnPublication(PublicationId publicationId)
        {
            var publication = _context.Publications.Include(p => p.Releases)
                .Single(p => p.Id == publicationId);
            return publication.Releases.Select(r => r.Order).DefaultIfEmpty().Max() + 1;
        }

        private async Task<List<ContentSection>> TemplateFromRelease(ReleaseId? releaseId)
        {
            if (releaseId.HasValue)
            {
                var templateContent = (await GetAsync(releaseId.Value)).Content;
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
        
        public Release Get(PublicationId id)
        {
            return _context.Releases.FirstOrDefault(x => x.Id == id);
        }

        public Release Get(string slug)
        {
            return _context.Releases.FirstOrDefault(x => x.Slug == slug);
        }
        
        public List<Release> List()
        {
            return _context.Releases.ToList();
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
                .Include(r => r.Type)
                .Include(r => r.ReleaseSummary.Versions)
                .Include(r => r.ReleaseSummary.Versions)
                .ThenInclude(v => v.Type);
        }
    }
}