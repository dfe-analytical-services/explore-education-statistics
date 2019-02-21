using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class LaCharacteristicDataService : AbstractDataService<CharacteristicDataLa>
    {
        public LaCharacteristicDataService(ApplicationDbContext context) : base(context)
        {
        }
    }
}