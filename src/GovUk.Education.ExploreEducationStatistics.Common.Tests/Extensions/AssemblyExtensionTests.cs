#nullable enable
using System.IO;
using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class AssemblyExtensionTests
{
    public class DirectoryTests
    {
        [Fact]
        public void GetDirectory()
        {
            var directory = Assembly.GetExecutingAssembly().GetDirectory();
            var expectedPath = Path.Combine(
                "src", "artifacts", "bin", "GovUk.Education.ExploreEducationStatistics.Common.Tests");

            Assert.Contains(expectedPath, directory.ToString());
            Assert.True(directory.Exists);
        }

        [Fact]
        public void GetDirectoryPath()
        {
            var path = Assembly.GetExecutingAssembly().GetDirectoryPath();
            var expectedPath = Path.Combine(
                "src", "artifacts", "bin", "GovUk.Education.ExploreEducationStatistics.Common.Tests");

            Assert.Contains(expectedPath, path);
            Assert.True(Directory.Exists(path));
        }
    }
}
