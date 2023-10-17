#nullable enable
using System;
using System.Text;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class StringBuilderExtensions
{
    private const string CrlfLineEnding = "\r\n";

    public static StringBuilder AppendCrlfLine(this StringBuilder builder)
    {
        return builder.Append(CrlfLineEnding);
    }

    public static StringBuilder AppendCrlfLine(this StringBuilder builder, string str)
    {
        builder.Append(str);

        return builder.Append(CrlfLineEnding);
    }

    /// <summary>
    /// Get a substring from the string builder using a range.
    /// </summary>
    /// <remarks>
    /// If the range starts or ends outside of the string builder's length,
    /// no <see cref="ArgumentOutOfRangeException"/> will be thrown.
    /// Instead, we will clamp the range to the length of the string builder.
    /// </remarks>
    /// <param name="builder">The current string builder</param>
    /// <param name="range">The substring range to get from the string builder</param>
    /// <returns>The substring</returns>
    public static string Substring(this StringBuilder builder, Range range)
    {
        if (range.Start.IsFromEnd && range.Start.Value > builder.Length)
        {
            range = ..range.End;
        }
        
        if (!range.End.IsFromEnd && range.End.Value > builder.Length)
        {
            range = range.Start..builder.Length;
        }

        var (offset, length) = range.GetOffsetAndLength(builder.Length);

        return builder.ToString(offset, length);
    }
}
