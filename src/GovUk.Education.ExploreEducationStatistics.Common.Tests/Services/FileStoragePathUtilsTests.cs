using System;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services
{
    public class FileStoragePathUtilsTests
    {
        [Fact]
        public void TestAdminReleaseBatchesDirectoryPath()
        {
            var blobPath = Guid.NewGuid();
            var fileId = Guid.NewGuid();
            Assert.Equal($"{blobPath}/data/batches/{fileId}/", AdminReleaseBatchesDirectoryPath(blobPath, fileId));
        }
        
        [Fact]
        public void TestAdminReleaseDirectoryPath()
        {
            var blobPath = Guid.NewGuid();
            Assert.Equal($"{blobPath}/ancillary/", AdminReleaseDirectoryPath(blobPath, Ancillary));
            Assert.Equal($"{blobPath}/data/", AdminReleaseDirectoryPath(blobPath, Data));
            Assert.Equal($"{blobPath}/chart/", AdminReleaseDirectoryPath(blobPath, Chart));
            Assert.Equal($"{blobPath}/image/", AdminReleaseDirectoryPath(blobPath, Image));
        }

        [Fact]
        public void TestAdminReleasePath()
        {
            var blobPath = Guid.NewGuid();
            var fileId = Guid.NewGuid();
            Assert.Equal($"{blobPath}/ancillary/{fileId}", AdminReleasePath(blobPath, Ancillary, fileId));
            Assert.Equal($"{blobPath}/data/{fileId}", AdminReleasePath(blobPath, Data, fileId));
            Assert.Equal($"{blobPath}/chart/{fileId}", AdminReleasePath(blobPath, Chart, fileId));
            Assert.Equal($"{blobPath}/image/{fileId}", AdminReleasePath(blobPath, Image, fileId));
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
