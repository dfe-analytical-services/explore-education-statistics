#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public static class JsonPathUtilsTests
{
    public class ConcatTests
    {
        [Theory]
        [InlineData("path.to.something", "path", "to", "something")]
        [InlineData("path.to.something", "$.path", "$.to", "$.something")]
        [InlineData("path.to.something", "$.path", "to", "something")]
        [InlineData("path.to.something", "$.path", "$.to", "something")]
        [InlineData("path.to.something", "path", "to", "$.something")]
        [InlineData("a.b.c.d[0].e", "a", "b.c", "d[0].e")]
        [InlineData("a.b.c.d[0].e", "$.a", "$.b.c", "$.d[0].e")]
        [InlineData("a.b[0].c[1].d", "$.a", "$.b[0]", "$.c[1].d")]
        [InlineData("a[0].b[1].c[2].d", "$.a[0]", "$.b[1]", "$.c[2].d")]
        [InlineData("a[0].b[1].c", "$", "$.a[0]", "$.b[1].c")]
        public void Success(string expectedPath, params string[] inputPaths)
        {
            Assert.Equal(expectedPath, JsonPathUtils.Concat(inputPaths));
        }

        [Theory]
        [InlineData(" path ", " to ", " something ")]
        [InlineData("   path", "  to ", "  something")]
        [InlineData("  path  ", "  to ", "something  ")]
        public void TrimsPaths(params string[] inputPaths)
        {
            Assert.Equal("path.to.something", JsonPathUtils.Concat(inputPaths));
        }
    }
}
