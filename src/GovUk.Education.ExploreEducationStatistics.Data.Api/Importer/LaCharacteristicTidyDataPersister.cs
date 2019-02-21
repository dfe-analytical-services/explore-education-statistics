using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class LaCharacteristicTidyDataPersister : ITidyDataPersister
    {
        private readonly ApplicationDbContext _context;

        public LaCharacteristicTidyDataPersister(ApplicationDbContext context)
        {
            _context = context;
        }

        public int Count(Guid publicationId)
        {
            return _context.CharacteristicDataLa.Count(data => data.PublicationId == publicationId);
        }
    }
}