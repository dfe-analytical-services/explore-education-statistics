#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Text;
using NaturalSort.Extension;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Convert a string into an enumerable of its constituent lines.
    /// </summary>
    public static IEnumerable<string> ToLines(this string value)
    {
        var reader = new StringReader(value);

        var currentLine = reader.ReadLine();

        while (currentLine != null)
        {
            yield return currentLine;
            currentLine = reader.ReadLine();
        }

        reader.Close();
    }

    /// <summary>
    /// Convert a string into a list of its constituent lines.
    /// </summary>
    public static IList<string> ToLinesList(this string value)
    {
        return value.ToLines().ToList();
    }

    /// <summary>
    /// Order some strings in natural order for humans to read.
    /// </summary>
    public static IOrderedEnumerable<string> NaturalOrder(
        this IEnumerable<string> source,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        return source.Order(comparison.WithNaturalSort());
    }

    public static Stream ToStream(this string value)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);

        writer.Write(value);
        writer.Flush();

        stream.Position = 0;
        return stream;
    }

    public static string StripLines(this string value)
    {
        return value.StripLineBreaks();
    }

    public static int IndentWidth(this string value)
    {
        var firstChar = value.IndexOfFirst(c => !c.IsWhiteSpaceCharacter());
        return firstChar > -1 ? firstChar : value.Length;
    }

    public static string TrimToLower(this string value)
    {
        return value.Trim().ToLower();
    }

    public static string TrimIndent(this string value)
    {
        var lines = value.ToLines()
            .ToList();

        var minIndent = lines.Where(line => !string.IsNullOrEmpty(line))
            .Select(line => line.IndentWidth())
            .Min();

        return lines
            .Select(line => line.Skip(minIndent).JoinToString())
            .JoinToString("\n");
    }

    public static string AppendTrailingSlash(this string input)
    {
        return input.EndsWith("/") ? input : input + "/";
    }

    public static string CamelCase(this string input)
    {
        if (input.IsNullOrEmpty())
        {
            return input;
        }

        var s = PascalCase(input);
        return char.ToLowerInvariant(s[0]) + s.Substring(1);
    }

    public static string ToLowerFirst(this string input)
    {
        return input.IsNullOrEmpty() ?
            input :
            char.ToLowerInvariant(input[0]) + input[1..];
    }

    public static string ToUpperFirst(this string input)
    {
        return input.IsNullOrEmpty()
            ? input
            : char.ToUpperInvariant(input[0]) + input[1..];
    }

    public static string ToTitleCase(this string input)
    {
        var titleCasedInputBuilder = new StringBuilder();

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        words.ForEach(word =>
        {
            titleCasedInputBuilder.Append(word
                .ToLowerInvariant()
                .ToUpperFirst())
                .Append(' ');
        });

        return input.IsNullOrEmpty()
            ? input
            : titleCasedInputBuilder
                .ToString()
                .TrimEnd();
    }

    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool IsNullOrWhitespace([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static string? NullIfWhiteSpace(this string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static string PascalCase(this string input)
    {
        if (input.IsNullOrEmpty())
        {
            return input;
        }

        var cultInfo = CultureInfo.CurrentCulture.TextInfo;
        input = Regex.Replace(input, "[^A-Za-z0-9]", " ");
        input = Regex.Replace(input, "([A-Z]+)", " $1");
        input = cultInfo.ToTitleCase(input);
        input = input.Replace(" ", "");
        return input;
    }

    public static string SnakeCase(this string input)
    {
        if (input.IsNullOrEmpty())
        {
            return input;
        }

        input = Regex.Replace(input, "[^A-Za-z0-9]", " ");
        input = Regex.Replace(input, "([A-Z]+)", " $1").Trim();
        input = Regex.Replace(input, "(\\s+)", "_");

        return input.ToLower();
    }

    public static string ToMd5Hash(this string input, Encoding? encoding = null)
    {
        using var md5 = MD5.Create();

        var inputBytes = (encoding ?? Encoding.UTF8).GetBytes(input);

        return BitConverter.ToString(md5.ComputeHash(inputBytes))
            .Replace("-", string.Empty)
            .ToLowerInvariant();
    }

    /// <summary>
    /// Decodes a Base64 encoded string back to its original representation.
    /// </summary>
    /// <param name="input">The Base64 encoded string to be decoded. If null, the method returns null.</param>
    /// <returns>
    /// The original string decoded from the Base64 input, or null if the input is null.
    /// </returns>
    [return: NotNullIfNotNull(nameof(input))]
    public static string? FromBase64String(this string? input)
    {
        if (input == null)
        {
            return null;
        }

        var bytes = Convert.FromBase64String(input);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Converts a string to its Base64 encoded representation.
    /// </summary>
    /// <param name="input">The input string to be encoded. If null, the method returns null.</param>
    /// <returns>
    /// A Base64 encoded string representation of the input, or null if the input is null.
    /// </returns>
    [return: NotNullIfNotNull(nameof(input))]
    public static string? ToBase64String(this string? input)
    {
        if (input == null)
        {
            return null;
        }

        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }
}
