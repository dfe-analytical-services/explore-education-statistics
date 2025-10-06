using System.Text.RegularExpressions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public static class SqlTestUtils
{
    public static string NormaliseSqlFormatting(string sql)
    {
        var removeNewLines = new Regex("\n", RegexOptions.Compiled);
        var removeWhitespaceAfterOpeningBracket = new Regex("\\( *", RegexOptions.Compiled);
        var removeWhitespaceBeforeClosingBracket = new Regex(" *\\)", RegexOptions.Compiled);
        var removeExtraWhitespace = new Regex("[ ]{2,}", RegexOptions.Compiled);

        var newLinesStripped = removeNewLines.Replace(sql, "");
        var openingBracketWhitespaceStripped = removeWhitespaceAfterOpeningBracket
            .Replace(newLinesStripped, "(");
        var closingBracketWhitespaceStripped = removeWhitespaceBeforeClosingBracket
            .Replace(openingBracketWhitespaceStripped, ")");
        return removeExtraWhitespace
            .Replace(closingBracketWhitespaceStripped, " ")
            .Trim();
    }
}
