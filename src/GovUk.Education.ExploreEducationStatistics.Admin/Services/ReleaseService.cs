using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using PublicationId = System.Guid;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ApplicationDbContext _context;

        public ReleaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Release Get(Guid id)
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

        // TODO Authorisation will be required when users are introduced
        public ReleaseViewModel GetViewModel(Guid id)
        {
            return ReleaseToReleaseViewMapper.Map<ReleaseViewModel>(Get(id));
        }

        // TODO Authorisation will be required when users are introduced
        public ReleaseViewModel CreateRelease(CreateReleaseViewModel createRelease)
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
                TimePeriodCoverage = createRelease.TimeIdentifier,
                PublishScheduled = createRelease.PublishScheduled,
                ReleaseName = createRelease.ReleaseName,
                NextReleaseDate = createRelease.NextReleaseExpected,
                Content = content
            });
            _context.SaveChanges();
            return GetViewModel(saved.Entity.Id);
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
                        // TODO do we want to copy any more that this?
                    }).ToList();
                }
            }
            return new List<ContentSection>();
        }
        
        private static readonly IMapper ReleaseToReleaseViewMapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Release, ReleaseViewModel>()
                .ForMember(dest => dest.LatestRelease,
                    m => m.MapFrom(r => r.Publication.LatestRelease().Id == r.Id));
        }).CreateMapper();
    }
}