using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ApplicationDbContext _context;

        private readonly IPublicationService _publicationService;

        public ReleaseService(ApplicationDbContext context, IPublicationService publicationService)
        {
            _context = context;
            _publicationService = publicationService;
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
        public ReleaseViewModel CreateRelease(EditReleaseViewModel createRelease)
        {
            // Get the current release order
            var p = _publicationService.Get(createRelease.PublicationId);
            var nextReleaseOrder = p.LatestRelease() != null ? p.LatestRelease().Order + 1 : 0;
            
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
            
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Release, ReleaseViewModel>()
                    .ForMember(dest => dest.LatestRelease,
                        LatestReleaseMapperConfig);
            });
            
            var mapper = config.CreateMapper();
            return mapper.Map<ReleaseViewModel>(saved);
        }
        
        public static void LatestReleaseMapperConfig(IMemberConfigurationExpression<Release, ReleaseViewModel, bool> m)
        {
            m.MapFrom(r => r.Publication.LatestRelease().Id == r.Id);
        }
        
    }
}
