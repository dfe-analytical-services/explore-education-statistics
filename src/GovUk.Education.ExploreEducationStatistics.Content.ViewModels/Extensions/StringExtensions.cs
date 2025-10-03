using System.Text.RegularExpressions;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels.Extensions;

public static partial class StringExtensions
{
    [GeneratedRegex("<.*?>")]
    private static partial Regex MatchHtmlTags();

    [GeneratedRegex(@"(\r\n|\r|\n)")]
    private static partial Regex MatchNewLineRepresentations();

    /// <summary>
    /// A very unsophisticated method that removes anything that looks like an html tag from the string
    /// </summary>
    /// <param name="text">html string</param>
    /// <returns>plain text string</returns>
    public static string StripHtml(this string text) => MatchHtmlTags().Replace(text, string.Empty);

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
