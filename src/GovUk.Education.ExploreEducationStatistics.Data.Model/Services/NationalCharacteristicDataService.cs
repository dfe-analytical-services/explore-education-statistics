using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class NationalCharacteristicDataService : AbstractDataService<CharacteristicDataNational>,
        INationalCharacteristicDataService
    {
        public NationalCharacteristicDataService(ApplicationDbContext context,
            ILogger<NationalCharacteristicDataService> logger) : base(context, logger)
        {
        }
    }
}