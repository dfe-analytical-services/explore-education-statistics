using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            // Get the current release order
            var publication = _context.Publications.Include(p => p.Releases)
                .Single(p => p.Id == createRelease.PublicationId);
            var nextReleaseOrder = publication?.LatestRelease()?.Order + 1 ?? 0;


            var saved = _context.Releases.Add(new Release
            {
                Id = Guid.NewGuid(),
                Order = nextReleaseOrder,
                PublicationId = createRelease.PublicationId,
                Published = null,
                TypeId = createRelease.ReleaseTypeId,
                TimePeriodCoverage = createRelease.TimeIdentifier,
                PublishScheduled = createRelease.PublishScheduled,
                ReleaseName = createRelease.ReleaseName,
                NextReleaseDate = createRelease.NextReleaseExpected
            });
            _context.SaveChanges();
            return GetViewModel(saved.Entity.Id);
        }

        private static readonly IMapper ReleaseToReleaseViewMapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Release, ReleaseViewModel>()
                .ForMember(dest => dest.LatestRelease,
                    m => m.MapFrom(r => r.Publication.LatestRelease().Id == r.Id));
        }).CreateMapper();
    }
}