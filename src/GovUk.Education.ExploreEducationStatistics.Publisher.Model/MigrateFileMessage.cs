#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

/// <summary>
/// Temporary message class used to migrate a file in EES-3547
/// TODO Remove in EES-3552
/// </summary>
public record MigrateFileMessage(Guid Id)
{
    public override string ToString()
    {
        return $"{nameof(Id)}: {Id}";
    }
}
