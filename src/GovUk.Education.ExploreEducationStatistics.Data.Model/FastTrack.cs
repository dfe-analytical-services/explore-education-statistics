using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class FastTrack
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public TableBuilderConfiguration Configuration { get; set; }

        public ObservationQueryContext Query { get; set; }

        public Guid ReleaseId { get; set; }

        public FastTrack(Guid id,
            TableBuilderConfiguration configuration,
            ObservationQueryContext query,
            Guid releaseId)
        {
            Id = id;
            Created = DateTime.UtcNow;
            Configuration = configuration;
            Query = query;
            ReleaseId = releaseId;
        }
    }
}