#nullable enable
using System;
using System.IO.Compression;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class ZipArchiveExtensions
    {
        /// <summary>
        /// Adds Unix file permissions to the zip entry.
        /// </summary>
        /// <remarks>
        /// We need this due to .NET not adding these permissions for us.
        /// See: https://github.com/dotnet/runtime/issues/1548
        /// This should be fixed in .NET 6 as part of the following PR:
        /// https://github.com/dotnet/runtime/pull/55531
        /// </remarks>
        public static ZipArchiveEntry SetUnixPermissions(this ZipArchiveEntry entry, string permissions)
        {
            entry.ExternalAttributes |= Convert.ToInt32(permissions, 8) << 16;
            return entry;
        }
    }
}