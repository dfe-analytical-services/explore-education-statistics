using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class FastTrack
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public TableBuilderQueryContext Query { get; set; }

        public FastTrack()
        {
            Id = Guid.NewGuid();
            Created = DateTime.UtcNow;
        }
    }
}