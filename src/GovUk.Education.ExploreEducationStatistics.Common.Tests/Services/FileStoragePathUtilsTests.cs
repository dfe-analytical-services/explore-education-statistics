using System;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services
{
    public class FileStoragePathUtilsTests
    {
        [Fact]
        public void TestAdminDataFileBatchesDirectoryPath()
        {
            var releaseId = Guid.NewGuid();
            var fileId = Guid.NewGuid();
            Assert.Equal($"{releaseId}/data/batches/{fileId}/", AdminDataFileBatchesDirectoryPath(releaseId, fileId));
        }

        [Fact]
        public void TestAdminReleaseDirectoryPath()
        {
            var rootPath = Guid.NewGuid();
            Assert.Equal($"{rootPath}/ancillary/", AdminReleaseDirectoryPath(rootPath, Ancillary));
            Assert.Equal($"{rootPath}/data/", AdminReleaseDirectoryPath(rootPath, Data));
            Assert.Equal($"{rootPath}/chart/", AdminReleaseDirectoryPath(rootPath, Chart));
            Assert.Equal($"{rootPath}/image/", AdminReleaseDirectoryPath(rootPath, Image));
        }

        [Fact]
        public void TestAdminReleasePath()
        {
            var rootPath = Guid.NewGuid();
            var fileId = Guid.NewGuid();
            Assert.Equal($"{rootPath}/ancillary/{fileId}", AdminReleasePath(rootPath, Ancillary, fileId));
            Assert.Equal($"{rootPath}/data/{fileId}", AdminReleasePath(rootPath, Data, fileId));
            Assert.Equal($"{rootPath}/chart/{fileId}", AdminReleasePath(rootPath, Chart, fileId));
            Assert.Equal($"{rootPath}/image/{fileId}", AdminReleasePath(rootPath, Image, fileId));
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
