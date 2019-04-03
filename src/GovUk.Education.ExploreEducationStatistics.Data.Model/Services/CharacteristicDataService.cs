using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class CharacteristicDataService : AbstractDataService<CharacteristicData, long>, ICharacteristicDataService
    {
        public CharacteristicDataService(ApplicationDbContext context, ILogger<CharacteristicDataService> logger) :
            base(context, logger)
        {
        }
    }
}