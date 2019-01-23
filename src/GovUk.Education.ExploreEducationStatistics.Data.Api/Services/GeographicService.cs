using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class GeographicService : BaseTidyDataService<TidyDataGeographic, GeographicQueryContext>
    {
        public GeographicService(MDatabase database) : base(database)
        {
        }
    }
}