using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;


namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services
{
    public class FileStoragePathUtilsTests
    {
        [Fact]
        public void TestAdminReleaseDirectoryPath()
        {
            var releaseId = Guid.NewGuid();
            Assert.Equal(releaseId + "/", AdminReleaseDirectoryPath(releaseId));
            Assert.Equal(releaseId + "/ancillary/", AdminReleaseDirectoryPath(releaseId, ReleaseFileTypes.Ancillary));
            Assert.Equal(releaseId + "/data/", AdminReleaseDirectoryPath(releaseId, ReleaseFileTypes.Data));
            Assert.Equal(releaseId + "/chart/", AdminReleaseDirectoryPath(releaseId, ReleaseFileTypes.Chart));
        }
        
        [Fact]
        public void TestAdminReleasePath()
        {
            var releaseId = Guid.NewGuid();
            Assert.Equal(releaseId + "/ancillary/file.png", AdminReleasePath(releaseId, ReleaseFileTypes.Ancillary, "file.png"));
            Assert.Equal(releaseId + "/data/file.csv", AdminReleasePath(releaseId, ReleaseFileTypes.Data, "file.csv"));
            Assert.Equal(releaseId + "/chart/file.doc", AdminReleasePath(releaseId, ReleaseFileTypes.Chart, "file.doc"));
        }
        
        [Fact]
        public void TestPublicReleaseDirectoryPath()
        {
            const string releaseSlug = "releaseslug";
            const string publicationSlug = "publicationslug";
            Assert.Equal(publicationSlug + "/" + releaseSlug + "/", PublicReleaseDirectoryPath(publicationSlug, releaseSlug));
            Assert.Equal(publicationSlug + "/" + releaseSlug + "/ancillary/", PublicReleaseDirectoryPath(publicationSlug, releaseSlug, ReleaseFileTypes.Ancillary));
            Assert.Equal(publicationSlug + "/" + releaseSlug + "/data/", PublicReleaseDirectoryPath(publicationSlug, releaseSlug, ReleaseFileTypes.Data));
            Assert.Equal(publicationSlug + "/" + releaseSlug + "/chart/", PublicReleaseDirectoryPath(publicationSlug, releaseSlug, ReleaseFileTypes.Chart));
        }
        
        [Fact]
        public void TestPublicReleasePath()
        {
            const string releaseSlug = "releaseslug";
            const string publicationSlug = "publicationslug";
            Assert.Equal(publicationSlug + "/" + releaseSlug + "/ancillary/file.png", PublicReleasePath(publicationSlug, releaseSlug, ReleaseFileTypes.Ancillary, "file.png"));
            Assert.Equal(publicationSlug+ "/" + releaseSlug +  "/data/file.csv", PublicReleasePath(publicationSlug, releaseSlug, ReleaseFileTypes.Data, "file.csv"));
            Assert.Equal(publicationSlug+ "/" + releaseSlug +  "/chart/file.doc", PublicReleasePath(publicationSlug, releaseSlug, ReleaseFileTypes.Chart, "file.doc"));
        } 
        
        
    }
    
}