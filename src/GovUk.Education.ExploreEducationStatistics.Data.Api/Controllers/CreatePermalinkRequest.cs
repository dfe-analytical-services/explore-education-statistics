using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    public class CreatePermalinkRequest
    {
        public TableBuilderConfiguration Configuration { get; set; }
        public ObservationQueryContext Query { get; set; }
    }
}