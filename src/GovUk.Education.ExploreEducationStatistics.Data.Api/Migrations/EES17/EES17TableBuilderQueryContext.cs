using GovUk.Education.ExploreEducationStatistics.Common.Migrations.EES17;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations.EES17
{
    public class EES17TableBuilderQueryContext : EES17ObservationQueryContext
    {
        public EES17TableBuilderConfiguration Configuration { get; set; }
    }
}