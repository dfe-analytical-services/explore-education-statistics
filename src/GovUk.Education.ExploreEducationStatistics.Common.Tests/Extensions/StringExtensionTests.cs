#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class StringExtensionTests
    {
        public class ToLinesListTests
        {
            [Fact]
            public void ReturnsSingleLine()
            {
                var lines = "Test line 1"
                    .ToLinesList();

                Assert.Single(lines);
                Assert.Equal("Test line 1", lines[0]);
            }

            [Fact]
            public void ReturnsMultipleLines()
            {
                var lines = "Test line 1\nTest line 2\nTest line 3"
                    .ToLinesList();

                Assert.Equal(3, lines.Count);
                Assert.Equal("Test line 1", lines[0]);
                Assert.Equal("Test line 2", lines[1]);
                Assert.Equal("Test line 3", lines[2]);
            }

            [Fact]
            public void EmptyStringReturnsNoLines()
            {
                var lines = ""
                    .ToLinesList();

                Assert.Empty(lines);
            }
        }

        public class StripLinesTests
        {
            [Fact]
            public void RemovesUnixLines()
            {
                var stripped = "test line 1 \ntest line 2".StripLines();
                Assert.Equal("test line 1 test line 2", stripped);
            }

            [Fact]
            public void RemovesUnixLines_ToEmptyString()
            {
                var stripped = "\n".StripLines();
                Assert.Equal("", stripped);
            }

            [Fact]
            public void RemovesWindowsLine()
            {
                var stripped = "test line 1 \r\ntest line 2".StripLines();
                Assert.Equal("test line 1 test line 2", stripped);
            }

            [Fact]
            public void RemovesWindowsLines_ToEmptyString()
            {
                var stripped = "\r\n".StripLines();
                Assert.Equal("", stripped);
            }
        }

        public class CamelCaseTests
        {
            [Fact]
            public void NullStringCanBeCamelCased()
            {
                string? input = null;
                Assert.Null(input!.CamelCase());
            }

            [Fact]
            public void EmptyStringCanBeCamelCased()
            {
                Assert.Equal(string.Empty, string.Empty.CamelCase());
            }

            [Fact]
            public void StringsWithNonAlphaNumericCharactersAreCamelCased()
            {
                Assert.Equal("fooBarBaz", "foo bar  baz".CamelCase());
                Assert.Equal("fooBarBaz", "foo-bar--baz".CamelCase());
                Assert.Equal("fooBarBaz", "foo.bar..baz".CamelCase());
                Assert.Equal("fooBarBaz", "foo_bar__baz".CamelCase());
                Assert.Equal("fooBarBaz", "foo,bar,,baz".CamelCase());
            }

            [Fact]
            public void AcronymsAreCamelCased()
            {
                Assert.Equal("aBC", "ABC".CamelCase());
            }

            [Fact]
            public void CamelCasedStringRemainsCamelCased()
            {
                Assert.Equal("foo", "foo".CamelCase());
                Assert.Equal("camelCase", "camelCase".CamelCase());
            }

            [Fact]
            public void PascalCaseStringIsCamelCased()
            {
                Assert.Equal("fooBar", "FooBar".CamelCase());
            }

            [Fact]
            public void UnderscoreSeparatedStringsAreCamelCased()
            {
                Assert.Equal("fooBar", "foo_bar".CamelCase());
                Assert.Equal("fooBar", "foo_Bar".CamelCase());
                Assert.Equal("fooBar", "Foo_Bar".CamelCase());
                Assert.Equal("fOOBar", "FOO_Bar".CamelCase());
            }
        }

        public class AppendTrailingSlashTests
        {
            [Fact]
            public void AppendTrailingSlash()
            {
                Assert.Equal("foo/", "foo".AppendTrailingSlash());
            }

            [Fact]
            public void EmptyStringCanAppendTrailingSlash()
            {
                Assert.Equal("/", string.Empty.AppendTrailingSlash());
            }
        }

        public class NullIfWhitespaceTests
        {
            [Fact]
            public void NullIfWhitespace_IsNullForNullString()
            {
                string? input = null;
                Assert.Null(input!.NullIfWhiteSpace());
            }

            [Fact]
            public void NullIfWhitespace_RemainsUntouchedForNonWhiteSpaceString()
            {
                const string input = "foo";
                Assert.Equal(input, input.NullIfWhiteSpace());
            }

            [Fact]
            public void NullIfWhitespace_IsNullForWhiteSpaceString()
            {
                Assert.Null("".NullIfWhiteSpace());
                Assert.Null(" ".NullIfWhiteSpace());
            }

            [Fact]
            public void NullStringCanAppendTrailingSlash()
            {
                string? input = null;
                Assert.Null(input.AppendTrailingSlash());
            }
        }

        public class IsNullOrEmptyTests
        {
            [Fact]
            public void TrueForNullString()
            {
                string? input = null;
                Assert.True(input.IsNullOrEmpty());
            }


            [Fact]
            public void TrueForEmptyString()
            {
                Assert.True("".IsNullOrEmpty());
            }

            [Fact]
            public void FalseForWhitespaceString()
            {
                Assert.False("   ".IsNullOrEmpty());
                Assert.False(" \n  ".IsNullOrEmpty());
                Assert.False(" \r\n  ".IsNullOrEmpty());
            }

            [Fact]
            public void FalseForNonEmptyString()
            {
                Assert.False("foo".IsNullOrEmpty());
            }
        }

        public class IsNullOrWhitespaceTests
        {
            [Fact]
            public void TrueForNullString()
            {
                string? input = null;
                Assert.True(input.IsNullOrWhitespace());
            }


            [Fact]
            public void TrueForEmptyString()
            {
                Assert.True("".IsNullOrWhitespace());
            }

            [Fact]
            public void TrueForWhitespaceString()
            {
                Assert.True("   ".IsNullOrWhitespace());
                Assert.True(" \n  ".IsNullOrWhitespace());
                Assert.True(" \r\n  ".IsNullOrWhitespace());
            }

            [Fact]
            public void FalseForNonEmptyString()
            {
                Assert.False("foo".IsNullOrWhitespace());
            }
        }

        public class PascalCaseTests
        {
            [Fact]
            public void NullStringCanBePascalCased()
            {
                string? input = null;
                Assert.Null(input!.PascalCase());
            }

            [Fact]
            public void StringsWithNonAlphaNumericCharactersArePascalCased()
            {
                Assert.Equal("FooBarBaz", "foo bar  baz".PascalCase());
                Assert.Equal("FooBarBaz", "foo-bar--baz".PascalCase());
                Assert.Equal("FooBarBaz", "foo.bar..baz".PascalCase());
                Assert.Equal("FooBarBaz", "foo_bar__baz".PascalCase());
                Assert.Equal("FooBarBaz", "foo,bar,,baz".PascalCase());
            }

            [Fact]
            public void EmptyStringCanBePascalCased()
            {
                Assert.Equal(string.Empty, string.Empty.PascalCase());
            }

            [Fact]
            public void ValuesAreNotTouchedByPascalCase()
            {
                Assert.Equal("Foo", "Foo".PascalCase());
                Assert.Equal("FOO", "FOO".PascalCase());
            }

            [Fact]
            public void PascalCasedStringRemainsPascalCased()
            {
                Assert.Equal("PascalCase", "PascalCase".PascalCase());
            }

            [Fact]
            public void CamelCaseStringIsPascalCased()
            {
                Assert.Equal("FooBar", "fooBar".PascalCase());
                Assert.Equal("FooBAR", "fooBAR".PascalCase());
            }

            [Fact]
            public void UnderscoreSeparatedStringsArePascalCased()
            {
                Assert.Equal("FooBar", "foo_bar".PascalCase());
                Assert.Equal("FooBar", "foo_Bar".PascalCase());
                Assert.Equal("FooBar", "Foo_Bar".PascalCase());
                Assert.Equal("FOOBar", "FOO_Bar".PascalCase());
            }
        }

        public class ScreamingSnakeCaseTests
        {
            [Fact]
            public void NullStringsCanBeScreamingSnakeCased()
            {
                string? input = null;
                Assert.Null(input!.ScreamingSnakeCase());
            }

            [Fact]
            public void EmptyStringCanBeScreamingSnakeCased()
            {
                Assert.Equal(string.Empty, string.Empty.ScreamingSnakeCase());
            }

            [Fact]
            public void AcronymsAreAlreadyScreamingSnakeCased()
            {
                Assert.Equal("ABC", "ABC".ScreamingSnakeCase());
            }

            [Fact]
            public void CamelCasedStringsAreScreamingSnakeCased()
            {
                Assert.Equal("FOO", "foo".ScreamingSnakeCase());
                Assert.Equal("CAMEL_CASE_STRING", "camelCaseString".ScreamingSnakeCase());
            }

            [Fact]
            public void UnderscoreSeparatedStringsAreScreamingSnakeCased()
            {
                Assert.Equal("FOO_BAR", "foo_bar".ScreamingSnakeCase());
                Assert.Equal("FOO_BAR", "foo_Bar".ScreamingSnakeCase());
                Assert.Equal("FOO_BAR", "Foo_Bar".ScreamingSnakeCase());
                Assert.Equal("FOO_BAR", "FOO_Bar".ScreamingSnakeCase());
            }

            [Fact]
            public void PascalCasedStringsAreScreamingSnakeCased()
            {
                Assert.Equal("FOO_BAR", "FooBar".ScreamingSnakeCase());
                Assert.Equal("FOO_BAR", "FooBAr".ScreamingSnakeCase());
            }
        }
    }
}