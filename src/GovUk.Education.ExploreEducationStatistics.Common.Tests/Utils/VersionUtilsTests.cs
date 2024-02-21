using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public abstract class VersionUtilsTests
{
    public class TryParseTests : VersionUtilsTests
    {
        [Theory]
        [InlineData("1.0.0", 1, 0, 0)]
        [InlineData("1.1.0", 1, 1, 0)]
        [InlineData("1.1.1", 1, 1, 1)]
        [InlineData("2.0.0", 2, 0, 0)]
        [InlineData("1.0", 1, 0)]
        [InlineData("1.1", 1, 1)]
        [InlineData("2.0", 2, 0)]
        [InlineData("1", 1)]
        [InlineData("2", 2)]
        public void ValidVersion_SuccessfullyParsed(string versionString, int expectedMajor, int expectedMinor = default, int expectedPatch = default)
        {
            Assert.True(VersionUtils.TryParse(versionString, out int major, out int minor, out int patch));

            Assert.Equal(expectedMajor, major);
            Assert.Equal(expectedMinor, minor);
            Assert.Equal(expectedPatch, patch);
        }

        [Theory]
        [InlineData(" 1.1.1", 1, 1, 1)]
        [InlineData("1.1.1 ", 1, 1, 1)]
        [InlineData(" 1.1.1 ", 1, 1, 1)]
        public void VersionWithEmptySpaces_SuccessfullyParsed(string versionString, int expectedMajor, int expectedMinor = default, int expectedPatch = default)
        {
            Assert.True(VersionUtils.TryParse(versionString, out int major, out int minor, out int patch));

            Assert.Equal(expectedMajor, major);
            Assert.Equal(expectedMinor, minor);
            Assert.Equal(expectedPatch, patch);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("a")]
        [InlineData("1.a")]
        [InlineData("1.")]
        [InlineData("a 1")]
        [InlineData("1 a")]
        [InlineData("1.1.1.1")]

        public void InvalidVersion_FailsToParse(string versionString)
        {
            Assert.False(VersionUtils.TryParse(versionString, out int _, out int _, out int _));
        }
    }
}
