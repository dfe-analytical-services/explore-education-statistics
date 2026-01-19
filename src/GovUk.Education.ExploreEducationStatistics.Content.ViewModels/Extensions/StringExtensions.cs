using System.Text.RegularExpressions;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels.Extensions;

public static partial class StringExtensions
{
    [GeneratedRegex(@"(\r\n|\r|\n)")]
    private static partial Regex MatchNewLineRepresentations();

    /// <summary>
    /// Normalise all occurrences of new line to the Unix standard representation
    /// </summary>
    public static string UseUnixNewLine(this string value) => value.UseSpecificNewLine("\n");

    /// <summary>
    /// Normalise all occurrences of new line to the Windows standard representation
    /// </summary>
    public static string UseWindowsNewLine(this string value) => value.UseSpecificNewLine("\r\n");

    /// <summary>
    /// Normalise all occurrences of new line to the current environment standard representation
    /// </summary>
    public static string UseEnvironmentNewLine(this string value) => value.UseSpecificNewLine(Environment.NewLine);

    /// <summary>
    /// Normalise all occurrences of new line to the specified format
    /// </summary>
    private static string UseSpecificNewLine(this string value, string specificNewline) =>
        MatchNewLineRepresentations().Replace(value, specificNewline);
}
