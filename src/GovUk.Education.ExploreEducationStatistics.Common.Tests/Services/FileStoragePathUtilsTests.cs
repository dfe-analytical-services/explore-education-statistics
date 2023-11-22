#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services;

public class FileStoragePathUtilsTests
{
    public class PublicPathTests
    {
        public static TheoryData<string> PublicationData =
            new()
            {
                "publication-slug",
                "  publication-SLUG  "
            };

        public static TheoryData<string, string> PublicationAndReleaseData =
            new()
            {
                { "publication-slug", "release-slug" },
                { "  publication-SLUG  ", "  release-SLUG  " }
            };

        public static TheoryData<string, string, Guid> PublicationReleaseAndIdData =
            new()
            {
                { "publication-slug", "release-slug", Guid.NewGuid() },
                { "  publication-SLUG  ", "  release-SLUG  ", Guid.NewGuid() }
            };

        [Theory]
        [MemberData(nameof(PublicationData))]
        public void PublicContentPublicationPath(string publicationSlug)
        {
            Assert.Equal("publications/publication-slug/publication.json",
                FileStoragePathUtils.PublicContentPublicationPath(publicationSlug: publicationSlug));
        }

        [Theory]
        [MemberData(nameof(PublicationData))]
        public void PublicContentLatestReleasePath(string publicationSlug)
        {
            Assert.Equal("publications/publication-slug/latest-release.json",
                FileStoragePathUtils.PublicContentLatestReleasePath(publicationSlug: publicationSlug));

            Assert.Equal("staging/publications/publication-slug/latest-release.json",
                FileStoragePathUtils.PublicContentLatestReleasePath(publicationSlug: publicationSlug,
                    staging: true));
        }

        [Theory]
        [MemberData(nameof(PublicationAndReleaseData))]
        public void PublicContentReleasePath(string publicationSlug,
            string releaseSlug)
        {
            Assert.Equal("publications/publication-slug/releases/release-slug.json",
                FileStoragePathUtils.PublicContentReleasePath(publicationSlug: publicationSlug,
                    releaseSlug: releaseSlug));

            Assert.Equal("staging/publications/publication-slug/releases/release-slug.json",
                FileStoragePathUtils.PublicContentReleasePath(publicationSlug: publicationSlug,
                    releaseSlug: releaseSlug,
                    staging: true));
        }

        [Theory]
        [MemberData(nameof(PublicationReleaseAndIdData))]
        public void PublicContentDataBlockPath(string publicationSlug,
            string releaseSlug,
            Guid dataBlockId)
        {
            Assert.Equal($"publications/publication-slug/releases/release-slug/data-blocks/{dataBlockId}.json",
                FileStoragePathUtils.PublicContentDataBlockPath(publicationSlug: publicationSlug,
                    releaseSlug: releaseSlug,
                    dataBlockId));
        }

        [Theory]
        [MemberData(nameof(PublicationAndReleaseData))]
        public void PublicContentDataBlockParentPath(string publicationSlug,
            string releaseSlug)
        {
            Assert.Equal($"publications/publication-slug/releases/release-slug/data-blocks",
                FileStoragePathUtils.PublicContentDataBlockParentPath(publicationSlug: publicationSlug,
                    releaseSlug: releaseSlug));
        }

        [Theory]
        [MemberData(nameof(PublicationAndReleaseData))]
        public void PublicContentReleaseSubjectsPath(string publicationSlug,
            string releaseSlug)
        {
            Assert.Equal($"publications/publication-slug/releases/release-slug/subjects.json",
                FileStoragePathUtils.PublicContentReleaseSubjectsPath(publicationSlug: publicationSlug,
                    releaseSlug: releaseSlug));
        }

        [Theory]
        [MemberData(nameof(PublicationReleaseAndIdData))]
        public void PublicContentSubjectMetaPath(string publicationSlug,
            string releaseSlug,
            Guid subjectId)
        {
            Assert.Equal($"publications/publication-slug/releases/release-slug/subject-meta/{subjectId}.json",
                FileStoragePathUtils.PublicContentSubjectMetaPath(publicationSlug: publicationSlug,
                    releaseSlug: releaseSlug,
                    subjectId));
        }

        [Theory]
        [MemberData(nameof(PublicationAndReleaseData))]
        public void PublicContentSubjectMetaParentPath(string publicationSlug,
            string releaseSlug)
        {
            Assert.Equal($"publications/publication-slug/releases/release-slug/subject-meta",
                FileStoragePathUtils.PublicContentSubjectMetaParentPath(publicationSlug: publicationSlug,
                    releaseSlug: releaseSlug));
        }
    }

    public class PrivatePathTests
    {
        private class TestData : TheoryData<Guid, Guid>
        {
            public TestData()
            {
                Add(Guid.NewGuid(), Guid.NewGuid());
            }
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void PrivateContentDataBlockPath(Guid releaseId,
            Guid dataBlockId)
        {
            Assert.Equal($"releases/{releaseId}/data-blocks/{dataBlockId}.json",
                FileStoragePathUtils.PrivateContentDataBlockPath(releaseId: releaseId,
                    dataBlockId: dataBlockId));
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void PrivateContentSubjectMetaPath(Guid releaseId,
            Guid subjectId)
        {
            Assert.Equal($"releases/{releaseId}/subject-meta/{subjectId}.json",
                FileStoragePathUtils.PrivateContentSubjectMetaPath(releaseId: releaseId,
                    subjectId: subjectId));
        }
    }

    [Fact]
    public void FilesPath()
    {
        var rootPath = Guid.NewGuid();
        Assert.Equal($"{rootPath}/ancillary/", FileStoragePathUtils.FilesPath(rootPath, FileType.Ancillary));
        Assert.Equal($"{rootPath}/chart/", FileStoragePathUtils.FilesPath(rootPath, FileType.Chart));
        Assert.Equal($"{rootPath}/data/", FileStoragePathUtils.FilesPath(rootPath, FileType.Data));
        Assert.Equal($"{rootPath}/data-zip/", FileStoragePathUtils.FilesPath(rootPath, FileType.DataZip));
        Assert.Equal($"{rootPath}/image/", FileStoragePathUtils.FilesPath(rootPath, FileType.Image));
        Assert.Equal($"{rootPath}/data/", FileStoragePathUtils.FilesPath(rootPath, FileType.Metadata));
        Assert.Equal($"{rootPath}/zip/", FileStoragePathUtils.FilesPath(rootPath, FileType.AllFilesZip));
    }
}
