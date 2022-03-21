using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Permalink
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public TableBuilderConfiguration Configuration { get; set; }

        public PermalinkTableBuilderResult FullTable { get; set; }

        public ObservationQueryContext Query { get; set; }

        public Permalink()
        {
        }

        public Permalink(TableBuilderConfiguration configuration,
            PermalinkTableBuilderResult result,
            ObservationQueryContext query)
        {
            Id = Guid.NewGuid();
            Created = DateTime.UtcNow;
            Configuration = configuration;
            FullTable = result;
            Query = query;
        }
    }
}
