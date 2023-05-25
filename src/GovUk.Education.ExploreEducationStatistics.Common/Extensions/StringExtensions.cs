#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Text;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
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

        public static bool IsNullOrEmpty(this string? value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhitespace(this string? value)
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
    }
}
