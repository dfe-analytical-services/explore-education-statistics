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
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;

        public PublicationService(ContentDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public PublicationViewModel GetPublication(string slug)
        {
            // TODO: Publisher function missing mapping config
            var publication = _context.Publications.FirstOrDefault(t => t.Slug == slug);

            if (publication != null)
            {
                return new PublicationViewModel() {Id = publication.Id, Title = publication.Title};
            }

            return null;
        }

        public IEnumerable<Publication> ListPublicationsWithPublishedReleases()
        {
            // TODO: sort this to only be live releases
            return _context.Publications.Where(x => x.Releases.Any(r => r.Live)).Include(p => p.Releases).ToList();
//            return _context.Publications.Where(x => x.Releases.Any(r => r.Live)).Include(p => p.Releases.Where(r => r.Live)).ToList();
        }
    }
}