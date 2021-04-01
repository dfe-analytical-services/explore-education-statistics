using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions
{
    public class ReleaseFileExtensionTests
    {
        [Fact]
        public void BatchesPath()
        {
            var releaseFile = new ReleaseFile
            {
                File = new File
                {
                    Id = Guid.NewGuid(),
                    RootPath = Guid.NewGuid(),
                    Filename = "data.csv",
                    Type = FileType.Data
                }
            };

            Assert.Equal(releaseFile.File.BatchesPath(), releaseFile.BatchesPath());
        }
        
        [Fact]
        public void Path()
        {
            var releaseFile = new ReleaseFile
            {
                File = new File
                {
                    Id = Guid.NewGuid(),
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = FileType.Ancillary
                }
            };

            Assert.Equal(releaseFile.File.Path(), releaseFile.Path());
        }
        
        [Fact]
        public void PublicPath()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Id = Guid.NewGuid(),
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = FileType.Ancillary
                }
            };

            Assert.Equal(releaseFile.File.PublicPath(release), releaseFile.PublicPath());
        }
    }
}
