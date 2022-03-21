﻿#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

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
            var releaseFile = new ReleaseFile
            {
                Release = new Release(),
                Name = "Test ancillary file",
                Summary = "Test summary",
                File = new File
                {
                    Id = Guid.NewGuid(),
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary,
                    Created = new DateTime(),
                    CreatedBy = new User
                    {
                        Email = "test@test.com"
                    }
                }
            };

            var info = releaseFile.ToFileInfo(new BlobInfo
            (
                path: "Ignored",
                size: "400 B",
                contentType: "Ignored",
                contentLength: -1L,
                meta: new Dictionary<string, string>()
            ));

            Assert.Equal(releaseFile.FileId, info.Id);
            Assert.Equal("pdf", info.Extension);
            Assert.Equal("ancillary.pdf", info.FileName);
            Assert.Equal("Test ancillary file", info.Name);
            Assert.Equal("Test summary", info.Summary);
            Assert.Equal("400 B", info.Size);
            Assert.Equal(Ancillary, info.Type);
            Assert.Equal(releaseFile.File.Created, info.Created);
            Assert.Equal("test@test.com", info.UserName);
        }

        [Fact]
        public void ToFileInfo_NoCreatedBy()
        {
            var releaseFile = new ReleaseFile
            {
                Release = new Release(),
                Name = "Test ancillary file",
                File = new File
                {
                    Id = Guid.NewGuid(),
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary,
                }
            };

            var info = releaseFile.ToFileInfo(new BlobInfo
            (
                path: "Ignored",
                size: "400 B",
                contentType: "Ignored",
                contentLength: -1L,
                meta: new Dictionary<string, string>()
            ));

            Assert.Null(info.Created);
            Assert.Null(info.UserName);
        }
    }
}
