using System;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.ReleaseFileTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services
{
    public class FileStoragePathUtilsTests
    {
        [Fact]
        public void TestAdminReleaseDirectoryPath()
        {
            var releaseId = Guid.NewGuid();
            Assert.Equal($"{releaseId}/", AdminReleaseDirectoryPath(releaseId));
            Assert.Equal($"{releaseId}/ancillary/", AdminReleaseDirectoryPath(releaseId, Ancillary));
            Assert.Equal($"{releaseId}/data/", AdminReleaseDirectoryPath(releaseId, Data));
            Assert.Equal($"{releaseId}/chart/", AdminReleaseDirectoryPath(releaseId, Chart));
        }

        [Fact]
        public void TestAdminReleasePath()
        {
            var releaseId = Guid.NewGuid();
            var fileId = Guid.NewGuid();
            Assert.Equal($"{releaseId}/ancillary/{fileId}", AdminReleasePath(releaseId, Ancillary, fileId));
            Assert.Equal($"{releaseId}/data/{fileId}", AdminReleasePath(releaseId, Data, fileId));
            Assert.Equal($"{releaseId}/chart/{fileId}", AdminReleasePath(releaseId, Chart, fileId));
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

        [Fact]
        public void TestIsBatchFileForDataFile()
        {
            var releaseId = Guid.NewGuid();
            const string originalDataFileName = "my_data_file.csv";
         
            Assert.True(IsBatchFileForDataFile(releaseId, originalDataFileName, 
                $"{AdminReleaseBatchesDirectoryPath(releaseId)}{originalDataFileName}_000001"));
            Assert.True(IsBatchFileForDataFile(releaseId, originalDataFileName, 
                $"{AdminReleaseBatchesDirectoryPath(releaseId)}{originalDataFileName}_999999"));
            
            Assert.False(IsBatchFileForDataFile(Guid.NewGuid(), originalDataFileName, 
                $"{AdminReleaseBatchesDirectoryPath(releaseId)}{originalDataFileName}_000001"));
            Assert.False(IsBatchFileForDataFile(releaseId, originalDataFileName, 
                $"{AdminReleaseBatchesDirectoryPath(releaseId)}another_file_name.csv_000001"));
            Assert.False(IsBatchFileForDataFile(releaseId, originalDataFileName, 
                $"{AdminReleaseBatchesDirectoryPath(releaseId)}{originalDataFileName}_000001.csv"));
            Assert.False(IsBatchFileForDataFile(releaseId, originalDataFileName, 
                $"{AdminReleaseBatchesDirectoryPath(releaseId)}{originalDataFileName}.csv_000001"));
        }
    }
}