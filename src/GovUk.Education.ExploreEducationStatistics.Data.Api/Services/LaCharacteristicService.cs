using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class LaCharacteristicService : BaseDataService<CharacteristicDataLa, LaQueryContext>
    {
        public LaCharacteristicService(MDatabase database) : base(database)
        {
        }
    }
}