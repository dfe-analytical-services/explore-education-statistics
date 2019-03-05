using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class LaCharacteristicDataService : AbstractDataService<CharacteristicDataLa>, ILaCharacteristicDataService
    {
        public LaCharacteristicDataService(ApplicationDbContext context) : base(context)
        {
        }
    }
}