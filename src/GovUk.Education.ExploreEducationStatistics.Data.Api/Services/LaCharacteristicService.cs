using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class LaCharacteristicService : BaseTidyDataService<TidyDataLaCharacteristic, LaQueryContext>
    {
        public LaCharacteristicService(IMDatabase<TidyData> database) : base(database)
        {
        }
    }
}