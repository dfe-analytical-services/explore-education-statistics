using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseService
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
    }
}
