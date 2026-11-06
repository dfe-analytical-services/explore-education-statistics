using System.Security.Cryptography;
using System.Text;
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
    public static Guid UuidV7MsSql() => Uuid7.NewMsSqlUniqueIdentifier();

    /// <summary>
    /// Generate a name-based UUID v5, which is always the same for a given namespace and name.
    /// </summary>
    public static Guid UuidV5(Guid namespaceId, string name)
    {
        var hash = SHA1.HashData([.. namespaceId.ToByteArray(bigEndian: true), .. Encoding.UTF8.GetBytes(name)]);

        hash[6] = (byte)((hash[6] & 0x0F) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        return new Guid(hash.AsSpan(0, 16), bigEndian: true);
    }
}
