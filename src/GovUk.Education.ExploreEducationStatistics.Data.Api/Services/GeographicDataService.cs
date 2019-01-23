using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class GeographicDataService : BaseDataService<GeographicData, GeographicQueryContext>
    {
        public GeographicDataService(MDatabase database) : base(database)
        {
        }
    }
}