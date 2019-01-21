using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class NationalCharacteristicService : BaseTidyDataService<TidyDataNationalCharacteristic, NationalQueryContext>
    {
        public NationalCharacteristicService(IMDatabase<TidyDataNationalCharacteristic> database) : base(database)
        {
        }
    }
}