#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public abstract class Versioned<T> where T : Versioned<T>
    {
        public DateTime Created { get; set; }

        public User CreatedBy { get; set; } = null!;

        public Guid CreatedById { get; set; }

        public T? PreviousVersion { get; set; }

        public Guid? PreviousVersionId { get; set; }

        public int Version { get; set; }
    }
}
