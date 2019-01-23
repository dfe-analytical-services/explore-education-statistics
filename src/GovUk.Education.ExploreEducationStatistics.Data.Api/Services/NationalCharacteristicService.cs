using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class NationalCharacteristicService : BaseDataService<CharacteristicDataNational, NationalQueryContext>
    {
        public NationalCharacteristicService(MDatabase database) : base(database)
        {
        }
    }
}