using System;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services
{
    public class FileStoragePathUtilsTests
    {
        [Fact]
        public void TestAdminFilesPath()
        {
            var rootPath = Guid.NewGuid();
            Assert.Equal($"{rootPath}/ancillary/", AdminFilesPath(rootPath, Ancillary));
            Assert.Equal($"{rootPath}/chart/", AdminFilesPath(rootPath, Chart));
            Assert.Equal($"{rootPath}/data/", AdminFilesPath(rootPath, Data));
            Assert.Equal($"{rootPath}/zip/", AdminFilesPath(rootPath, DataZip));
            Assert.Equal($"{rootPath}/image/", AdminFilesPath(rootPath, Image));
            Assert.Equal($"{rootPath}/data/", AdminFilesPath(rootPath, Metadata));
        }

        [Fact]
        public void TestPublicReleaseDirectoryPath()
        {
            const string releaseSlug = "release-slug";
            const string publicationSlug = "publication-slug";
            Assert.Equal($"{publicationSlug}/{releaseSlug}/", PublicReleaseDirectoryPath(publicationSlug, releaseSlug));
            Assert.Equal($"{publicationSlug}/{releaseSlug}/ancillary/", PublicReleaseDirectoryPath(publicationSlug, releaseSlug, Ancillary));
            Assert.Equal($"{publicationSlug}/{releaseSlug}/data/", PublicReleaseDirectoryPath(publicationSlug, releaseSlug, Data));
            Assert.Equal($"{publicationSlug}/{releaseSlug}/chart/", PublicReleaseDirectoryPath(publicationSlug, releaseSlug, Chart));
        }

        [Fact]
        public void TestPublicReleasePath()
        {
            const string releaseSlug = "release-slug";
            const string publicationSlug = "publication-slug";
            var fileId = Guid.NewGuid();
            Assert.Equal($"{publicationSlug}/{releaseSlug}/ancillary/{fileId}", PublicReleasePath(publicationSlug, releaseSlug, Ancillary, fileId));
            Assert.Equal($"{publicationSlug}/{releaseSlug}/data/{fileId}", PublicReleasePath(publicationSlug, releaseSlug, Data, fileId));
            Assert.Equal($"{publicationSlug}/{releaseSlug}/chart/{fileId}", PublicReleasePath(publicationSlug, releaseSlug, Chart, fileId));
        }

        [Fact]
        public void TestPublicReleaseAllFilesZipPath()
        {
            const string releaseSlug = "release-slug";
            const string publicationSlug = "publication-slug";
            Assert.Equal($"{publicationSlug}/{releaseSlug}/ancillary/{publicationSlug}_{releaseSlug}.zip",
                PublicReleaseAllFilesZipPath(publicationSlug, releaseSlug));
        }
    }
}
