using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
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