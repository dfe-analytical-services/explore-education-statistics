#nullable enable
using System.Reflection;
using System.Runtime.InteropServices;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public abstract class PathUtilsTests
{
    public class ProjectRootPathTests : PathUtilsTests
    {
        [Fact]
        public void ProjectRootPath()
        {
            Assert.True(Directory.Exists(PathUtils.ProjectRootPath));
            Assert.StartsWith(PathUtils.ProjectRootPath, Assembly.GetExecutingAssembly().Location);

            Assert.True(Directory.Exists(Path.Combine(PathUtils.ProjectRootPath, "src")));
            Assert.True(File.Exists(Path.Combine(PathUtils.ProjectRootPath, "README.md")));
            Assert.True(File.Exists(Path.Combine(PathUtils.ProjectRootPath, "LICENSE")));
        }
    }

    public class OsPathTests : PathUtilsTests
    {
        [Theory]
        [InlineData(@"\absolute\path\to\file", "absolute", "path", "to", "file")]
        [InlineData("/absolute/path/to/file", "absolute", "path", "to", "file")]
        public void CurrentOS_CorrectAbsolutePath(string path, params string[] segments)
        {
            var expectedPath = Path.Combine(segments.Prepend(Path.DirectorySeparatorChar.ToString()).ToArray());

            Assert.Equal(expectedPath, PathUtils.OsPath(path));
        }

        [Theory]
        [InlineData(@"relative\path\to\file", "relative", "path", "to", "file")]
        [InlineData("relative/path/to/file", "relative", "path", "to", "file")]
        public void CurrentOS_CorrectRelativePath(string path, params string[] segments)
        {
            var expectedPath = Path.Combine(segments);

            Assert.Equal(expectedPath, PathUtils.OsPath(path));
        }

        [Theory]
        [InlineData(@"C:\path\to\file", @"C:\path\to\file")]
        [InlineData(@"C:path\to\file", @"C:path\to\file")]
        [InlineData(@"\absolute\path\to\file", @"\absolute\path\to\file")]
        [InlineData(@"relative\path\to\file", @"relative\path\to\file")]
        [InlineData("/absolute/path/to/file", @"\absolute\path\to\file")]
        [InlineData("relative/path/to/file", @"relative\path\to\file")]
        public void Windows_CorrectPath(string inputPath, string expectedPath)
        {
            Assert.Equal(expectedPath, PathUtils.OsPath(inputPath, OSPlatform.Windows));
        }

        [Theory]
        [InlineData(@"C:\path\to\file", "/c/path/to/file")]
        [InlineData(@"C:path\to\file", "/c/path/to/file")]
        [InlineData(@"\absolute\path\to\file", "/absolute/path/to/file")]
        [InlineData(@"relative\path\to\file", "relative/path/to/file")]
        [InlineData("/absolute/path/to/file", "/absolute/path/to/file")]
        [InlineData("relative/path/to/file", "relative/path/to/file")]
        public void Linux_CorrectPath(string inputPath, string expectedPath)
        {
            Assert.Equal(expectedPath, PathUtils.OsPath(inputPath, OSPlatform.Linux));
        }
    }
}
