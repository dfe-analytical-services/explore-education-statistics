using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly PersistenceHelper<Release, Guid> _releaseHelper;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReleaseService(ContentDbContext context, IMapper mapper, 
            IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _releaseHelper = new PersistenceHelper<Release, Guid>(_context, context.Releases, ReleaseNotFound);
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<Either<ValidationResult, Release>> GetAsync(Guid id)
        {
            return _releaseHelper.CheckEntityExists(id, async release =>
            {
                var canView = await _authorizationService.MatchesPolicy(GetUser(), release, CanViewSpecificRelease);

                if (!canView)
                {
                    return ValidationResult<Release>(ForbiddenToAccessRelease);
                }

                return release;
            });
        }
        
        private async Task<List<ContentSection>> GetContentAsync(Guid id)
        {
            return await _context
                .ReleaseContentSections
                .Include(join => join.ContentSection)
                .ThenInclude(section => section.Content)
                .Where(join => join.ReleaseId == id)
                .Select(join => join.ContentSection)
                .ToListAsync();
        }
        
        // TODO Authorisation will be required when users are introduced
        public async Task<ReleaseViewModel> GetReleaseForIdAsync(Guid id)
        {
            var release = await _context.Releases
                .HydrateReleaseForReleaseViewModel()
                .Where(x => x.Id == id)
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
        public Task<Either<ValidationResult, ReleaseSummaryViewModel>> GetReleaseSummaryAsync(Guid releaseId)
        {
            return _releaseHelper.CheckEntityExists(releaseId, async release =>
            {
                var canView = await _authorizationService.MatchesPolicy(GetUser(), release, CanViewSpecificRelease);

                if (!canView)
                {
                    return ValidationResult<ReleaseSummaryViewModel>(ForbiddenToAccessRelease);
                }
                
                var releaseForSummary = await _context.Releases
                    .Where(r => r.Id == releaseId)
                    .Include(r => r.ReleaseSummary)
                    .ThenInclude(summary => summary.Versions)
                    .Include(summary => summary.Type)
                    .FirstOrDefaultAsync();
                
                return _mapper.Map<ReleaseSummaryViewModel>(releaseForSummary.ReleaseSummary);
            });
        }

        // TODO Authorisation will be required when users are introduced
        public async Task<Either<ValidationResult, ReleaseViewModel>> EditReleaseSummaryAsync(
            Guid releaseId, UpdateReleaseSummaryRequest request)
        {
            return await ValidateReleaseSlugUniqueToPublication(request.Slug, releaseId, releaseId)
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
        public async Task<List<ReleaseViewModel>> GetReleasesForPublicationAsync(Guid publicationId)
        {
            var release = await _context.Releases
                .HydrateReleaseForReleaseViewModel()
                .Where(r => r.Publication.Id == publicationId)
                .ToListAsync();
            return _mapper.Map<List<ReleaseViewModel>>(release);
        }
        
        public async Task<List<ReleaseViewModel>> GetMyReleasesForReleaseStatusesAsync(
            params ReleaseStatus[] releaseStatuses)
        {
            var canViewAllReleases = 
                await _authorizationService.MatchesPolicy(GetUser(), CanViewAllReleases);
            
            if (canViewAllReleases)
            {
                return await GetAllReleasesForReleaseStatusesAsync(releaseStatuses);
            }

            return await GetReleasesForReleaseStatusRelatedToMeAsync(releaseStatuses);
        }

        private async Task<List<ReleaseViewModel>> GetAllReleasesForReleaseStatusesAsync(
            ReleaseStatus[] releaseStatuses)
        {
            var releases = await _context
                .Releases
                .HydrateReleaseForReleaseViewModel()
                .Where(r => releaseStatuses.Contains(r.Status))
                .ToListAsync();
            
            return _mapper.Map<List<ReleaseViewModel>>(releases);
        }

        private async Task<List<ReleaseViewModel>> GetReleasesForReleaseStatusRelatedToMeAsync(
            ReleaseStatus[] releaseStatuses)
        {
            var userId = GetUserId(GetUser());
            
            var userReleaseIds = await _context
                .UserReleaseRoles
                .Where(r => r.UserId == userId)
                .Select(r => r.ReleaseId)
                .ToListAsync();
            
            var releases = await _context
                .Releases
                .HydrateReleaseForReleaseViewModel()
                .Where(r => userReleaseIds.Contains(r.Id) && releaseStatuses.Contains(r.Status))
                .ToListAsync();
            
            return _mapper.Map<List<ReleaseViewModel>>(releases);
        }

        private async Task<Either<ValidationResult, bool>> ValidateReleaseSlugUniqueToPublication(string slug,
            Guid publicationId, Guid? releaseId = null)
        {
            if (await _context.Releases.AnyAsync(r => r.Slug == slug && r.PublicationId == publicationId && r.Id != releaseId))
            {
                return ValidationResult(SlugNotUnique);
            }

            return true;
        }

        private int OrderForNextReleaseOnPublication(Guid publicationId)
        {
            var publication = _context.Publications.Include(p => p.Releases)
                .Single(p => p.Id == publicationId);
            return publication.Releases.Select(r => r.Order).DefaultIfEmpty().Max() + 1;
        }

        private async Task<List<ContentSection>> TemplateFromRelease(Guid? releaseId)
        {
            if (releaseId.HasValue)
            {
                var templateContent = await GetContentAsync(releaseId.Value);
                if (templateContent != null)
                {
                    return templateContent.Select(c => new ContentSection
                    {
                        Id = new Guid(),
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
            Guid releaseId, ReleaseStatus status, string internalReleaseNote)
        {
            var release = _context.Releases.First(r => r.Id == releaseId);
            release.Status = status;
            release.InternalReleaseNote = internalReleaseNote;
            _context.Releases.Update(release);
            await _context.SaveChangesAsync();
            return await GetReleaseSummaryAsync(releaseId);
        }
        
        private ClaimsPrincipal GetUser()
        {
            return _httpContextAccessor.HttpContext.User;
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