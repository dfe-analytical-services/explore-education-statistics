using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class GeographicTidyDataPersister : ITidyDataPersister
    {
        private readonly ApplicationDbContext _context;

        public GeographicTidyDataPersister(ApplicationDbContext context)
        {
            _context = context;
        }

        public int Count(Guid publicationId)
        {
            return _context.GeographicData.Count(data => data.PublicationId == publicationId);
        }
    }
}