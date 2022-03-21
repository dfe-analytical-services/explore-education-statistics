using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services
{
    public class FileStoragePathUtilsTests
    {
        [Fact]
        public void TestFilesPath()
        {
            var rootPath = Guid.NewGuid();
            Assert.Equal($"{rootPath}/ancillary/", FilesPath(rootPath, Ancillary));
            Assert.Equal($"{rootPath}/chart/", FilesPath(rootPath, Chart));
            Assert.Equal($"{rootPath}/data/", FilesPath(rootPath, FileType.Data));
            Assert.Equal($"{rootPath}/data-zip/", FilesPath(rootPath, DataZip));
            Assert.Equal($"{rootPath}/image/", FilesPath(rootPath, Image));
            Assert.Equal($"{rootPath}/data/", FilesPath(rootPath, Metadata));
            Assert.Equal($"{rootPath}/zip/", FilesPath(rootPath, AllFilesZip));
        }
    }
}
