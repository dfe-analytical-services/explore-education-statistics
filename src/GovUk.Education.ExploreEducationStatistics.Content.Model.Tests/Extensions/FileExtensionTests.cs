using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.BlobInfo;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions
{
    public class FileExtensionTests
    {
        private const string PublicationSlug = "publication-slug";
        private const string ReleaseSlug = "release-slug";

        private readonly ReleaseFileReference _file = new ReleaseFileReference
        {
            Id = Guid.NewGuid(),
            Filename = "ancillary.pdf",
            ReleaseId = Guid.NewGuid(),
            ReleaseFileType = Ancillary
        };

        [Fact]
        public void Path_ReleaseFile()
        {
            var releaseFile = new ReleaseFile
            {
                ReleaseFileReference = _file
            };

            Assert.Equal(_file.Path(), releaseFile.Path());
        }

        [Fact]
        public void Path()
        {
            Assert.Equal(AdminReleasePath(_file.ReleaseId, _file.ReleaseFileType, _file.BlobStorageName), _file.Path());
        }

        [Fact]
        public void PublicPath_ReleaseFile()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = PublicationSlug
                },
                Slug = ReleaseSlug
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = _file
            };

            Assert.Equal(_file.PublicPath(release), releaseFile.PublicPath());
        }

        [Fact]
        public void PublicPath()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = PublicationSlug
                },
                Slug = ReleaseSlug
            };
            
            Assert.Equal(PublicReleasePath(PublicationSlug, ReleaseSlug, _file.ReleaseFileType, _file.BlobStorageName),
                _file.PublicPath(release));
        }

        [Fact]
        public void ToFileInfo()
        {
            var result = _file.ToFileInfo(new BlobInfo
            (
                path: "Ignored",
                size: "400 B",
                contentType: "Ignored",
                contentLength: -1L,
                meta: new Dictionary<string, string>
                {
                    {FilenameKey, "Ignored"},
                    {NameKey, "Test ancillary file"}
                }
            ));

            Assert.Equal(_file.Id, result.Id);
            Assert.Equal("pdf", result.Extension);
            Assert.Equal("ancillary.pdf", result.FileName);
            Assert.Equal("Test ancillary file", result.Name);
            Assert.Equal(_file.Path(), result.Path);
            Assert.Equal("400 B", result.Size);
            Assert.Equal(Ancillary, result.Type);
        }

        [Fact]
        public void ToFileInfo_BlobWithNoNameKey()
        {
            // Name is retrieved from the blob meta properties due to EES-1637 (Subject doesn't exist early on and Name on Data files is Subject name persisted on blob)
            // Chart files don't have any meta properties added to avoid duplicating data unnecessarily since nothing requires the filename or name of them
            // Nevertheless test that we populate the name from the database file reference when the blob name property is missing

            var result = _file.ToFileInfo(new BlobInfo
            (
                path: "Ignored",
                size: "400 B",
                contentType: "Ignored",
                contentLength: -1L,
                meta: new Dictionary<string, string>()
            ));

            Assert.Equal(_file.Id, result.Id);
            Assert.Equal("pdf", result.Extension);
            Assert.Equal("ancillary.pdf", result.FileName);
            Assert.Equal("ancillary.pdf", result.Name);
            Assert.Equal(_file.Path(), result.Path);
            Assert.Equal("400 B", result.Size);
            Assert.Equal(Ancillary, result.Type);
        }

        [Fact]
        public void ToFileInfoNotFound()
        {
            var result = _file.ToFileInfoNotFound();

            Assert.Equal(_file.Id, result.Id);
            Assert.Equal("pdf", result.Extension);
            Assert.Equal("ancillary.pdf", result.FileName);
            Assert.Equal("Unknown", result.Name);
            Assert.Null(result.Path);
            Assert.Equal("0.00 B", result.Size);
            Assert.Equal(Ancillary, result.Type);
        }

        [Fact]
        public void ToPublicFileInfo()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = PublicationSlug
                },
                Slug = ReleaseSlug
            };

            var result = _file.ToPublicFileInfo(new BlobInfo
            (
                path: _file.PublicPath(release),
                size: "400 B",
                contentType: "Ignored",
                contentLength: -1L,
                meta: new Dictionary<string, string>
                {
                    {FilenameKey, "Ignored"},
                    {NameKey, "Test ancillary file"}
                }
            ));

            Assert.Equal(_file.Id, result.Id);
            Assert.Equal("pdf", result.Extension);
            Assert.Equal("ancillary.pdf", result.FileName);
            Assert.Equal("Test ancillary file", result.Name);
            Assert.Equal(_file.PublicPath(release), result.Path);
            Assert.Equal("400 B", result.Size);
            Assert.Equal(Ancillary, result.Type);
        }
    }
}