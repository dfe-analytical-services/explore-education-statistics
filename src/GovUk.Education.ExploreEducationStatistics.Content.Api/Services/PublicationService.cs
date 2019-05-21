using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ApplicationDbContext _context;

        public PublicationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Publication GetPublication(string id)
        {
            return Guid.TryParse(id, out var newGuid)
                ? _context.Publications.Include(x => x.Releases).Include(x => x.LegacyReleases)
                    .FirstOrDefault(t => t.Id == newGuid)
                : _context.Publications.Include(x => x.Releases).Include(x => x.LegacyReleases)
                    .FirstOrDefault(t => t.Slug == id);
        }
    }
}