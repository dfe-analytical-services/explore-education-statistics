using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public Release Get(Guid id)
        {
            return _context.Releases.FirstOrDefault(x => x.Id == id);
        }
        
        public async Task<Release> GetAsync(Guid id)
        {
            return await _context.Releases.Include(r => r.Publication).ThenInclude(p => p.Releases).FirstOrDefaultAsync(x => x.Id == id);
        }

        public Release Get(string slug)
        {
            return _context.Releases.FirstOrDefault(x => x.Slug == slug);
        }

        public List<Release> List()
        {
            return _context.Releases.ToList();
        }

        // TODO Authorisation will be required when users are introduced
        public async Task<ReleaseViewModel> GetViewModel(ReleaseId id)
        {
            // Require publication / release graph to be able to work out if the release is the latest. 
            var release = await _context.Releases
                .Include(r => r.Publication)
                .ThenInclude(p => p.Releases)
                .FirstOrDefaultAsync(x => x.Id == id); 
            return _mapper.Map<ReleaseViewModel>(release);
        }

        // TODO Authorisation will be required when users are introduced
        public async Task<ReleaseViewModel> CreateReleaseAsync(CreateReleaseViewModel createRelease)
        {
            var order = OrderForNextReleaseOnPublication(createRelease.PublicationId);
            var content = TemplateFromRelease(createRelease.TemplateReleaseId);                  
            var saved = _context.Releases.Add(new Release
            {
                Id = Guid.NewGuid(),
                Order = order,
                PublicationId = createRelease.PublicationId,
                Published = null,
                TypeId = createRelease.ReleaseTypeId,
                TimePeriodCoverage = createRelease.TimePeriodCoverage,
                PublishScheduled = createRelease.PublishScheduled,
                ReleaseName = createRelease.ReleaseName,
                NextReleaseDate = createRelease.NextReleaseExpected,
                Content = content
            });
            await _context.SaveChangesAsync();
            return await GetViewModel(saved.Entity.Id);
        }
        
        // TODO Authorisation will be required when users are introduced
        public async Task<ActionResult<EditReleaseViewModel>> GetReleaseSummaryAsync(ReleaseId releaseId)
        {
            var release = await _context.Releases.FirstOrDefaultAsync(r => r.Id == releaseId);
            return _mapper.Map<EditReleaseViewModel>(release);
        }
        
        // TODO Authorisation will be required when users are introduced
        public async Task<ActionResult<ReleaseViewModel>> EditReleaseSummaryAsync(EditReleaseViewModel model)
        {
            var release = await _context.Releases.FirstOrDefaultAsync(r => r.Id == model.Id);
            _context.Releases.Update(release);
            _mapper.Map(release, model);
            await _context.SaveChangesAsync();
            return await GetViewModel(model.Id);
        }

        private int OrderForNextReleaseOnPublication(Guid publicationId)
        {
            var publication = _context.Publications.Include(p => p.Releases)
                .Single(p => p.Id == publicationId);
            return publication?.LatestRelease()?.Order + 1 ?? 0;
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
}