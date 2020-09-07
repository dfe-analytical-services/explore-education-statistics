#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public abstract class Versioned
    {
        public DateTime Created { get; set; }

        public User CreatedBy { get; set; } = null!;

        public Guid CreatedById { get; set; }

        public Versioned? PreviousVersion { get; set; }

        public Guid? PreviousVersionId { get; set; }

        public int Version { get; set; }
    }
}