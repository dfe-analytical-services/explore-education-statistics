#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public static class ContentEncodings
{
    /// <summary>
    /// A stream of bytes compressed using the Gzip file format
    /// </summary>
    public const string Gzip = "gzip";

    /// <summary>
    /// A stream of bytes compressed using the Zstandard protocol
    /// </summary>
    public const string Zstd = "zstd";
}
