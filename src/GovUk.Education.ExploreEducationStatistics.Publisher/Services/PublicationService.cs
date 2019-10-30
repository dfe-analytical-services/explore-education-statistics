using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ContentDbContext _context;

        public PublicationService(ContentDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Publication> ListPublicationsWithPublishedReleases()
        {
            // TODO: sort this to only be live releases
            return _context.Publications.Where(x => x.Releases.Any(r => r.Live)).Include(p => p.Releases).ToList();
//            return _context.Publications.Where(x => x.Releases.Any(r => r.Live)).Include(p => p.Releases.Where(r => r.Live)).ToList();
        }
    }
}