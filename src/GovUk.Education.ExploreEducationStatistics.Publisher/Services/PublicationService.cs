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
            return _context.Publications
                // TODO: Only include releases that are live rather than all releases
                .Include(p => p.Releases)
                .ToList()
                .Where(publication => publication.Releases.Any(release => release.Live));
        }
    }
}