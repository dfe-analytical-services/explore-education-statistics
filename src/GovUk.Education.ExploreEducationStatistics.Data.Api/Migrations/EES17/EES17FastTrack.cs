using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations.EES17
{
    public class EES17FastTrack
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public EES17TableBuilderQueryContext Query { get; set; }

        public EES17FastTrack()
        {
            Id = Guid.NewGuid();
            Created = DateTime.UtcNow;
        }
    }
}