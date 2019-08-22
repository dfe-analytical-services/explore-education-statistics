using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PublicationService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public PublicationViewModel GetPublication(string slug)
        {
            return _mapper.Map<PublicationViewModel>(_context.Publications.FirstOrDefault(t => t.Slug == slug));
        }

        public IEnumerable<Publication> ListPublicationsWithPublishedReleases()
        {
            return _context.Publications.Where(x => x.Releases.Any(r => r.Live)).Include(p => p.Releases.Where(r => r.Live));
        }
    }
}