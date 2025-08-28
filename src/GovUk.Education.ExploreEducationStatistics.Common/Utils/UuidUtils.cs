#nullable enable
using Medo;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class UuidUtils
{
    /// <summary>
    /// Generate a UUID v7 compatible with non-MSSQL databases e.g. Postgres.
    /// </summary>
    /// <remarks>
    /// Use <see cref="UuidV7MsSql"/> if you are using an MSSQL database.
    /// </remarks>
    public static Guid UuidV7() => Uuid7.NewGuid();

    /// <summary>
    /// Generate a UUID v7 compatible with MSSQL databases.
    /// </summary>
    /// <remarks>
    /// Use <see cref="UuidV7"/> if you are using non-MSSQL databases.
    /// </remarks>
    public static Guid UuidV7MsSql() => Uuid7.NewGuidMsSql();
}
