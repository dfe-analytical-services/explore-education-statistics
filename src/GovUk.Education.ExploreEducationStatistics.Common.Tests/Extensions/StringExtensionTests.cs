using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public class StringExtensionTests
    {
        [Fact]
        public void NullStringCanBeCamelCased()
        {
            string input = null;
            Assert.Null(input.CamelCase());
        }

        [Fact]
        public void EmptyStringCanBeCamelCased()
        {
            Assert.Equal(string.Empty, string.Empty.CamelCase());
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

        [Fact]
        public void NullStringCanBePascalCased()
        {
            string input = null;
            Assert.Null(input.PascalCase());
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

        [Fact]
        public void NullStringsCanBeScreamingSnakeCased()
        {
            string input = null;
            Assert.Null(input.ScreamingSnakeCase());
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