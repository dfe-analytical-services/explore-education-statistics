#nullable enable
using System;
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
                "src", "GovUk.Education.ExploreEducationStatistics.Common.Tests", "bin");

            Assert.Contains(expectedPath, directory.ToString());
            Assert.True(directory.Exists);
        }

        [Fact]
        public void GetDirectoryName()
        {
            var path = Assembly.GetExecutingAssembly().GetDirectoryPath();
            var expectedPath = Path.Combine(
                "src", "GovUk.Education.ExploreEducationStatistics.Common.Tests", "bin");

            Assert.Contains(expectedPath, path);
            Assert.True(Directory.Exists(path));
        }
    }
}
