using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class GeographicService : BaseTidyDataService<TidyDataGeographic, GeographicQueryContext>
    {
        public GeographicService(IMDatabase<TidyData> database) : base(database)
        {
        }
    }
}