using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class LaCharacteristicDataService : AbstractDataService<CharacteristicDataLa>, ILaCharacteristicDataService
    {
        public LaCharacteristicDataService(ApplicationDbContext context, ILogger<LaCharacteristicDataService> logger) :
            base(context, logger)
        {
        }
    }
}