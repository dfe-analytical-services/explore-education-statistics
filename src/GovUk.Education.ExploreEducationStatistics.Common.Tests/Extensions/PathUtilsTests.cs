#nullable enable
using System.IO;
using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class PathUtilsTests
{
    public class ProjectRootPathTests
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
}
