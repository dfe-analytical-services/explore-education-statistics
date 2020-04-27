using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class FastTrack
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        // TODO EES-229 Added from TableBuilderQueryContext
        public TableBuilderConfiguration Configuration { get; set; }

        // TODO EES-229 Replaced with ObservationQueryContext from TableBuilderQueryContext
        public ObservationQueryContext Query { get; set; }

        public FastTrack()
        {
            Id = Guid.NewGuid();
            Created = DateTime.UtcNow;
        }
    }
}