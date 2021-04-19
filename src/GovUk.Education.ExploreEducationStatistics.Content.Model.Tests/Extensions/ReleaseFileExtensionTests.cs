using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.BlobInfo;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions
{
    public class ReleaseFileExtensionTests
    {
        private readonly ReleaseFile _releaseFile = new ReleaseFile
        {
            Release = new Release(),
            File = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };

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
                    Type = Data
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
                    Type = Ancillary
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
                    Type = Ancillary
                }
            };

            Assert.Equal(releaseFile.File.PublicPath(release), releaseFile.PublicPath());
        }

        [Fact]
        public void ToFileInfo()
        {
            var result = _releaseFile.ToFileInfo(new BlobInfo
            (
                path: "Ignored",
                size: "400 B",
                contentType: "Ignored",
                contentLength: -1L,
                meta: new Dictionary<string, string>
                {
                    {NameKey, "Test ancillary file"}
                }
            ));

            Assert.Equal(_releaseFile.FileId, result.Id);
            Assert.Equal("pdf", result.Extension);
            Assert.Equal("ancillary.pdf", result.FileName);
            Assert.Equal("Test ancillary file", result.Name);
            Assert.Equal("400 B", result.Size);
            Assert.Equal(Ancillary, result.Type);
        }

        [Fact]
        // TODO EES-2052
        // As part of the subtasks of EES-1512, subject names are no longer added to and access via blob files, but 
        // via the ReleaseFile table. At the time of writing, this hasn't been done for ancillary files, and
        // therefore there is still some logic in BlobInfo and releaseFile.ToFileInfo() to handle getting names from
        // blob files. Once ancillary files also add and access their names from the ReleaseFiles table, this logic 
        // can be removed and this test removed.
        public void ToFileInfo_BlobWithNoNameKey()
        {
            var result = _releaseFile.ToFileInfo(new BlobInfo
            (
                path: "Ignored",
                size: "400 B",
                contentType: "Ignored",
                contentLength: -1L,
                meta: new Dictionary<string, string>()
            ));

            Assert.Equal(_releaseFile.FileId, result.Id);
            Assert.Equal("pdf", result.Extension);
            Assert.Equal("ancillary.pdf", result.FileName);
            Assert.Equal("ancillary.pdf", result.Name);
            Assert.Equal("400 B", result.Size);
            Assert.Equal(Ancillary, result.Type);
        }
    }
}
