#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public static class VersionUtilsTests
{
    public class TryParseTests
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
        [InlineData("v2.0.0", 2, 0, 0)]
        [InlineData("v2.0", 2, 0)]
        [InlineData("v2", 2)]
        public void ValidVersion_SuccessfullyParsed(
            string versionString,
            int expectedMajor,
            int expectedMinor = default,
            int expectedPatch = default)
        {
            Assert.True(VersionUtils.TryParse(versionString, out var version));

            Assert.Equal(expectedMajor, version.Major);
            Assert.Equal(expectedMinor, version.Minor);
            Assert.Equal(expectedPatch, version.Patch);
        }

        [Theory]
        [InlineData(" 1.1.1", 1, 1, 1)]
        [InlineData("1.1.1 ", 1, 1, 1)]
        [InlineData(" 1.1.1 ", 1, 1, 1)]
        public void VersionWithEmptySpaces_SuccessfullyParsed(
            string versionString,
            int expectedMajor,
            int expectedMinor = default,
            int expectedPatch = default)
        {
            Assert.True(VersionUtils.TryParse(versionString, out var version));

            Assert.Equal(expectedMajor, version.Major);
            Assert.Equal(expectedMinor, version.Minor);
            Assert.Equal(expectedPatch, version.Patch);
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
        [InlineData("V1.1.1")]
        [InlineData("V1.1")]
        [InlineData("V1")]

        public void InvalidVersion_FailsToParse(string versionString)
        {
            Assert.False(VersionUtils.TryParse(versionString, out _));
        }

        /// <summary>
        /// Only testing validation as the object returned from 'TryParseWildcard' returns a 'SemVersionRange' which doesn't contain a specific version that we can test against `versionString`
        /// </summary>
        /// <param name="versionString"></param>
        [Theory]
        [InlineData("*", null, null, null)]
        [InlineData("1.*.*", 1, null, null)]
        [InlineData("  1.*.*", 1, null, null)]
        [InlineData("  1.*.*  ", 1, null, null)]
        [InlineData("1.*", 1, null, null)]
        [InlineData("1.2.*", 1, 2, null)]
        [InlineData("v*", null, null, null)]
        [InlineData("v1.*.*", 1, null, null)]
        [InlineData("v1.*", 1, null, null)]
        [InlineData("v1.2.*", 1, 2, null)]
        public void ValidWildcardVersionString_SuccessfullyValidated(
            string versionString,
            int? major,
            int? minor,
            int? patch
            )
        {
            Assert.True(VersionUtils.TryParseWildcard(versionString, out var wildcardVersion));
            
            Assert.NotNull(wildcardVersion);
            Assert.Equal((uint?)major, wildcardVersion.VersionMajor);
            Assert.Equal((uint?)major, wildcardVersion.VersionMajor);
            Assert.Equal((uint?)minor, wildcardVersion.VersionMinor);
            Assert.Equal((uint?)patch, wildcardVersion.VersionPatch);
        }

        [Theory]
        [InlineData("2.1*.0")]
        [InlineData("2.**.*")]
        [InlineData("2*.*.0")]
        [InlineData("2*.1.0")]
        [InlineData("*.1.0")]
        [InlineData("1.*.4")]
        public void InvalidWildcardVersionString_FailsValidation(string versionString)
        {
            Assert.False(VersionUtils.TryParseWildcard(versionString, out var wildcardVersion));
            Assert.Null(wildcardVersion);
        }
    }
}
