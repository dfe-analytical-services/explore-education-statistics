using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GovUk.Education.ExploreEducationStatistics.Common.Database;

/// <summary>
/// Generates UUID v7s that are compatible with non-MSSQL databases (e.g. Postgres).
/// </summary>
public class UuidV7ValueGenerator : ValueGenerator<Guid>
{
    public override Guid Next(EntityEntry entry) => UuidUtils.UuidV7();

    public override bool GeneratesTemporaryValues => false;
}
