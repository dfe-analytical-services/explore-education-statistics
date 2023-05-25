#nullable enable
using System.Text;
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

        public class IndentWidth
        {
            [Fact]
            public void WithCharacters()
            {
                Assert.Equal(4, "    Test".IndentWidth());
                Assert.Equal(8, "        Test".IndentWidth());
            }

            [Fact]
            public void NoCharacters()
            {
                Assert.Equal(4, "    ".IndentWidth());
                Assert.Equal(8, "        ".IndentWidth());
            }
        }

        public class TrimIndent
        {
            [Fact]
            public void SingleLine()
            {
                Assert.Equal("Test", "    Test".TrimIndent());
                Assert.Equal("Test", "        Test".TrimIndent());
            }

            [Fact]
            public void SingleLine_Empty()
            {
                Assert.Equal("", "    ".TrimIndent());
                Assert.Equal("", "        ".TrimIndent());
            }

            [Fact]
            public void MultiLine_OneIndent()
            {
                Assert.Equal(
                    @"Test
                    Test",
                    @"Test
                    Test".TrimIndent()
                );
            }

            [Fact]
            public void MultiLine_EqualIndents()
            {
                Assert.Equal(
                    @"
Test
Test",
                    @"
                    Test
                    Test".TrimIndent()
                );
            }

            [Fact]
            public void MultiLine_DifferentIndents()
            {
                Assert.Equal(
                    @"
Test
    Test",
                    @"
                    Test
                        Test".TrimIndent()
                );
            }

            [Fact]
            public void MultiLine_Empty()
            {
                Assert.Equal(
                    "\n\n",
                    @"
                    
                    ".TrimIndent()
                );
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

        public class SnakeCaseTests
        {
            [Fact]
            public void Null()
            {
                string? input = null;
                Assert.Null(input!.SnakeCase());
            }

            [Fact]
            public void Empty()
            {
                Assert.Equal(string.Empty, string.Empty.SnakeCase());
            }

            [Theory]
            [InlineData("foo bar  baz")]
            [InlineData("foo-bar--baz")]
            [InlineData("foo_bar__baz")]
            [InlineData("foo,bar,,baz")]
            [InlineData("foo.bar..baz")]
            public void NonAlphaNumeric(string input)
            {
                Assert.Equal("foo_bar_baz", input.SnakeCase());
            }

            [Theory]
            [InlineData("foo 2 bar1  baz 3")]
            [InlineData("foo-2-bar1--baz-3")]
            [InlineData("foo_2_bar1__baz_3")]
            [InlineData("foo,2,bar1,,baz,3")]
            [InlineData("foo.2.bar1..baz.3")]
            public void WithNumbers(string input)
            {
                Assert.Equal("foo_2_bar1_baz_3", input.SnakeCase());
            }

            [Theory]
            [InlineData("foo")]
            [InlineData("Foo")]
            [InlineData("FOO")]
            public void SingleWord(string input)
            {
                Assert.Equal("foo", input.SnakeCase());
            }

            [Theory]
            [InlineData("foo_bar")]
            [InlineData("foo-bar")]
            [InlineData("FOO_BAR")]
            [InlineData("fooBar")]
            [InlineData("fooBAR")]
            [InlineData("FooBar")]
            public void ExistingCasings(string input)
            {
                Assert.Equal("foo_bar", input.SnakeCase());
            }
        }

        public class ToMd5HashTests
        {
            [Fact]
            public void Utf8Encoding()
            {
                var input = "hey";
                Assert.Equal("6057f13c496ecf7fd777ceb9e79ae285", input.ToMd5Hash());
            }

            [Fact]
            public void Utf8Encoding_Utf8Chars()
            {
                var input = "héy";
                Assert.Equal("55adad7d576b41782831a8815778a74a", input.ToMd5Hash());
            }

            [Fact]
            public void AsciiEncoding()
            {
                var input = "hey";
                Assert.Equal("6057f13c496ecf7fd777ceb9e79ae285", input.ToMd5Hash(Encoding.ASCII));
            }

            [Fact]
            public void AsciiEncoding_Utf8Chars()
            {
                var input = "héy";
                Assert.Equal("f7d6171b6bcfd8923a50b65721064b27", input.ToMd5Hash(Encoding.ASCII));
            }

        }
    }
}
