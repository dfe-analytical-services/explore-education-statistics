using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using ContentSectionId = System.Guid;
using PublicationId = System.Guid;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;

        public ReleaseService(ContentDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public Release Get(ReleaseId id)
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
        
        public async Task<List<ContentSection>> GetContentAsync(ReleaseId id)
        {
            return await _context
                .Releases
                .Where(release => release.Id == id)
                .Select(release => release.Content)
                .SelectMany(join => join.Select(j => j.ContentSection))
                .ToListAsync();
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
        public async Task<Either<ValidationResult, ReleaseViewModel>> CreateReleaseAsync(
            CreateReleaseViewModel createRelease)
        {
            return await ValidateReleaseSlugUniqueToPublication(createRelease.Slug, createRelease.PublicationId)
                .OnSuccess(async () =>
                {
                    var releaseSummary = _mapper.Map<ReleaseSummaryVersion>(createRelease);
                    releaseSummary.Created = DateTime.Now;
                    
                    var release = _mapper.Map<Release>(createRelease);
                    release.GenericContent = await TemplateFromRelease(createRelease.TemplateReleaseId);
                    release.SummarySection = new ContentSection
                    {
                        Type = ContentSectionType.ReleaseSummary
                    };
                    release.KeyStatisticsSection = new ContentSection{
                        Type = ContentSectionType.KeyStatistics
                    };
                    release.KeyStatisticsSecondarySection = new ContentSection{
                        Type = ContentSectionType.KeyStatisticsSecondary
                    };
                    release.HeadlinesSection = new ContentSection{
                        Type = ContentSectionType.Headlines
                    };
                    release.Order = OrderForNextReleaseOnPublication(createRelease.PublicationId);
                    release.ReleaseSummary = new ReleaseSummary
                    {
                        Versions = new List<ReleaseSummaryVersion>()
                        {
                            releaseSummary
                        }
                    };
                    var saved =_context.Releases.Add(release);
                    await _context.SaveChangesAsync();
                    return await GetReleaseForIdAsync(saved.Entity.Id);
                });
        }

        // TODO Authorisation will be required when users are introduced
        public async Task<ReleaseSummaryViewModel> GetReleaseSummaryAsync(ReleaseId releaseId)
        {
            var release = await _context.Releases
                .Where(r => r.Id == releaseId)
                .Include(r => r.ReleaseSummary)
                .ThenInclude(summary => summary.Versions)
                .Include(summary => summary.Type)
                .FirstOrDefaultAsync();
            return _mapper.Map<ReleaseSummaryViewModel>(release.ReleaseSummary);
        }

        // TODO Authorisation will be required when users are introduced
        public async Task<Either<ValidationResult, ReleaseViewModel>> EditReleaseSummaryAsync(
            Guid releaseId, UpdateReleaseSummaryRequest request)
        {
            var publication = await GetAsync(releaseId);
            return await ValidateReleaseSlugUniqueToPublication(request.Slug, publication.Id, releaseId)
                .OnSuccess(async () =>
                {
                    var release = await _context.Releases
                        .Where(r => r.Id == releaseId)
                        .Include(r => r.ReleaseSummary)
                        .ThenInclude(summary => summary.Versions)
                        .FirstOrDefaultAsync();

                    release.Slug = request.Slug;
                    release.TypeId = request.TypeId;
                    release.PublishScheduled = request.PublishScheduled;
                    release.ReleaseName = request.ReleaseName;
                    release.NextReleaseDate = request.NextReleaseDate;
                    release.TimePeriodCoverage = request.TimePeriodCoverage;
                    
                    var newSummaryVersion = new ReleaseSummaryVersion
                    {
                        Slug = request.Slug,
                        TypeId = request.TypeId,
                        PublishScheduled = request.PublishScheduled,
                        ReleaseName = request.ReleaseName,
                        NextReleaseDate = request.NextReleaseDate,
                        TimePeriodCoverage = request.TimePeriodCoverage,
                        Created = DateTime.Now
                    };
                    
                    release.ReleaseSummary.Versions.Add(newSummaryVersion);
                    _context.Update(release);
                    await _context.SaveChangesAsync();
                    return await GetReleaseForIdAsync(releaseId);
                });
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
        
        // TODO Authorisation will be required when users are introduced
        public async Task<List<ReleaseViewModel>> GetReleasesForReleaseStatusesAsync(params ReleaseStatus[] releaseStatuses)
        {
            var release = await _context.Releases
                .Where(r => releaseStatuses.Contains(r.Status))
                .HydrateReleaseForReleaseViewModel()
                .ToListAsync();
            return _mapper.Map<List<ReleaseViewModel>>(release);
        }

        private async Task<Either<ValidationResult, bool>> ValidateReleaseSlugUniqueToPublication(string slug,
            PublicationId publicationId, ReleaseId? releaseId = null)
        {
            if (await _context.Releases.AnyAsync(r => r.Slug == slug && r.PublicationId == publicationId && r.Id != releaseId))
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
                var templateContent = await GetContentAsync(releaseId.Value);
                if (templateContent != null)
                {
                    return templateContent.Select(c => new ContentSection
                    {
                        Id = new ContentSectionId(),
                        Caption = c.Caption,
                        Heading = c.Heading,
                        Order = c.Order,
                        // TODO in future do we want to copy across more? Is it possible to do so?
                    }).ToList();
                }
            }

            return new List<ContentSection>();
        }

        // TODO Authorisation will be required when users are introduced
        public async Task<Either<ValidationResult, ReleaseSummaryViewModel>> UpdateReleaseStatusAsync(
            ReleaseId releaseId, ReleaseStatus status, string internalReleaseNote)
        {
            var release = await GetAsync(releaseId);
            release.Status = status;
            release.InternalReleaseNote = internalReleaseNote;
            _context.Releases.Update(release);
            await _context.SaveChangesAsync();
            return await GetReleaseSummaryAsync(releaseId);
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
                .ThenInclude(publication => publication.Releases) // Back refs required to work out latest
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Contact)
                .Include(r => r.Type);
        }
    }
}