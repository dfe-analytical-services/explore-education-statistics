using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class NationalCharacteristicDataService : AbstractDataService<CharacteristicDataNational>
    {
        public NationalCharacteristicDataService(ApplicationDbContext context) : base(context)
        {
        }
    }
}