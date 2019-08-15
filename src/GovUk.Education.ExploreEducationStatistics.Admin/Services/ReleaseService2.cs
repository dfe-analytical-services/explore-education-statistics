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

// DFE-1163 release summary information into version tables.
namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseService2 : IReleaseService2
    {
        private readonly ApplicationDbContext _context;

        // TODO this can bee added to MappingProfiles when the fields are moved from the release object
        private static readonly IMapper _mapper = new MapperConfiguration(cfg =>
        {
            // TODO this can be rationalised if we add delegate methods to Release.
            cfg.CreateMap<Release, ReleaseViewModel>()
                .ForMember(dest => dest.Contact,
                    m => m.MapFrom(r => r.Publication.Contact))
                .ForMember(dest => dest.Published,
                    m => m.MapFrom(r => r.ReleaseSummary.Published))
                .ForMember(dest => dest.Title,
                    m => m.MapFrom(r => r.ReleaseSummary.Title))
                .ForMember(dest => dest.Type,
                    m => m.MapFrom(r => r.ReleaseSummary.Type))
                .ForMember(dest => dest.CoverageTitle,
                    m => m.MapFrom(r => r.ReleaseSummary.CoverageTitle))
                .ForMember(dest => dest.LatestRelease,
                    m => m.MapFrom(r => r.Publication.LatestRelease().Id == r.Id))
                .ForMember(dest => dest.PublicationTitle,
                    m => m.MapFrom(r => r.Publication.Title))
                .ForMember(dest => dest.PublishScheduled,
                    m => m.MapFrom(r => r.ReleaseSummary.PublishScheduled))
                .ForMember(dest => dest.ReleaseName,
                    m => m.MapFrom(r => r.ReleaseSummary.ReleaseName))
                .ForMember(dest => dest.TypeId,
                    m => m.MapFrom(r => r.ReleaseSummary.TypeId))
                .ForMember(dest => dest.YearTitle,
                    m => m.MapFrom(r => r.ReleaseSummary.YearTitle))
                .ForMember(dest => dest.NextReleaseDate,
                    m => m.MapFrom(r => r.ReleaseSummary.NextReleaseDate))
                .ForMember(dest => dest.TimePeriodCoverage,
                    m => m.MapFrom(r => r.ReleaseSummary.TimePeriodCoverage));

            cfg.CreateMap<ReleaseSummaryVersion, CreateReleaseViewModel>();
            cfg.CreateMap<ReleaseSummaryVersion, EditReleaseSummaryViewModel>();
            cfg.CreateMap<EditReleaseSummaryViewModel, Release>();
        }).CreateMapper();


        public ReleaseService2(ApplicationDbContext context)
        {
            _context = context;
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
                    // DFE-1163 release summary information into version tables.
                    var saved = _context.Releases.Add(new Release
                    {
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
            return _mapper.Map<EditReleaseSummaryViewModel>(release);
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
                    newSummary.Published = currentSummary.Published;
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
                .Select(r => new ReleaseViewModel
                {
                })
                .ToListAsync();

            return _mapper.Map<List<ReleaseViewModel>>(release);
        }

        private async Task<Either<ValidationResult, bool>> ValidateReleaseSlugUniqueToPublication(string slug,
            PublicationId publicationId, ReleaseId? releaseId = null)
        {
            var slugMatches = _context.Releases
                .Where(r => r.PublicationId == publicationId)
                .Include(r => r.ReleaseSummary)
                .ThenInclude(rs => rs.Versions)
                .Select(r => r.ReleaseSummary)
                .Any(rs => rs.ReleaseId != releaseId && rs.Current.Slug == slug);
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
    }
}