#nullable enable
using System;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class StringBuilderExtensionTests
{
    public class AppendCrlfLineTests
    {
        [Fact]
        public void NoArgs()
        {
            var builder = new StringBuilder();
            builder.AppendCrlfLine();

            Assert.Equal("\r\n", builder.ToString());
        }

        [Fact]
        public void WithStringArg()
        {
            var builder = new StringBuilder();
            builder.AppendCrlfLine("test");

            Assert.Equal("test\r\n", builder.ToString());
        }
    }

    public class SubstringTests
    {
        public static TheoryData<string, Range> RangeInsideLengthData => new()
        {
            { "al", 1..3 },
            { "alidating", 1.. },
            { "alidatin", 1..^1 },
            { "lidat", 2..^3 },
            { "idating", 3.. },
            { "dat", ^6..^3 },
            { "da", 4..^4 },
            { "valida", ..^4 },
        };

        [Theory]
        [MemberData(nameof(RangeInsideLengthData))]
        public void RangeInsideLength(string expected, Range range)
        {
            var builder = new StringBuilder();
            builder.Append("validating");

            Assert.Equal(expected, builder.Substring(range));
        }

        public static TheoryData<string, Range> RangeOutsideLengthData => new()
        {
            { "s", ^7..^5 },
            { "s", ^6..^5 },
            { "string", ^7.. },
            { "string", ^6.. },
            { "string", ..6 },
            { "string", ..7 },
            { "g", 5.. },
            { "g", 5..6 },
        };

        [Theory]
        [MemberData(nameof(RangeOutsideLengthData))]
        public void RangeOutsideLength(string expected, Range range)
        {
            var builder = new StringBuilder();
            builder.Append("string");

            Assert.Equal(expected, builder.Substring(range));
        }

        public static TheoryData<Range> InvalidRangeData => new()
        {
            5..7,
            7..5,
            ^5..^6,
            ^6..^5,
        };

        [Theory]
        [MemberData(nameof(InvalidRangeData))]
        public void InvalidRange_Throws(Range range)
        {
            var builder = new StringBuilder();
            builder.Append("test");

            Assert.Throws<ArgumentOutOfRangeException>(() => builder.Substring(range));
        }
    }
}
