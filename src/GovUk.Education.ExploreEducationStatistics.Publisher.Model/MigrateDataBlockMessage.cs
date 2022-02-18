#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    /// <summary>
    /// Temporary message class used to migrate a data block for EES-3167.
    /// </summary>
    public record MigrateDataBlockMessage
    {
        public Guid Id { get; init; }

        public MigrateDataBlockMessage(Guid id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}";
        }
    }
}
