using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class NationalCharacteristicTidyDataPersister : ITidyDataPersister
    {
        private readonly ApplicationDbContext _context;

        public NationalCharacteristicTidyDataPersister(ApplicationDbContext context)
        {
            _context = context;
        }

        public int Count(Guid publicationId)
        {
            return _context.CharacteristicDataNational.Count(data => data.PublicationId == publicationId);
        }
    }
}