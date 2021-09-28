using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions
{
    public class ReleaseExtensionTests
    {
        [Fact]
        public void IsLatestPublishedVersionOfRelease_PublicationHasSingleUnpublishedRelease()
        {
            var publication = new Publication();

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = null,
                PreviousVersionId = null
            };

            publication.Releases = new List<Release>
            {
                release
            };

            Assert.False(release.IsLatestPublishedVersionOfRelease());
        }

        [Fact]
        public void IsLatestPublishedVersionOfRelease_PublicationHasSinglePublishedRelease()
        {
            var publication = new Publication();

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = DateTime.UtcNow.AddSeconds(-1),
                PreviousVersionId = null
            };

            publication.Releases = new List<Release>
            {
                release
            };

            Assert.True(release.IsLatestPublishedVersionOfRelease());
        }

        [Fact]
        public void IsLatestPublishedVersionOfRelease_PublicationHasMultipleReleases()
        {
            var publication = new Publication();

            // (Release 1) Published but superseded by another published Release
            var release1Version1 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = DateTime.UtcNow.AddSeconds(-1),
                PreviousVersionId = null
            };

            // (Release 1)  Latest published version (superseded by an unpublished Release)
            var release1Version2 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = DateTime.UtcNow.AddSeconds(-1),
                PreviousVersionId = release1Version1.Id
            };

            // (Release 1) Unpublished
            var release1Version3 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = null,
                PreviousVersionId = release1Version2.Id
            };

            // (Release 2) Latest published version
            var release2 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = DateTime.UtcNow.AddSeconds(-1),
                PreviousVersionId = null
            };

            // (Release 3) Unpublished
            var release3 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = null,
                PreviousVersionId = null
            };

            publication.Releases = new List<Release>
            {
                release1Version1,
                release1Version2,
                release1Version3,
                release2,
                release3
            };

            Assert.False(release1Version1.IsLatestPublishedVersionOfRelease());
            Assert.True(release1Version2.IsLatestPublishedVersionOfRelease());
            Assert.False(release1Version3.IsLatestPublishedVersionOfRelease());
            Assert.True(release2.IsLatestPublishedVersionOfRelease());
            Assert.False(release3.IsLatestPublishedVersionOfRelease());
        }

        [Fact]
        public void AllFilesZipPath()
        {
            const string releaseSlug = "release-slug";
            const string publicationSlug = "publication-slug";

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Slug = releaseSlug,
                Publication = new Publication
                {
                    Slug = publicationSlug
                }
            };

            Assert.Equal($"{release.Id}/zip/{publicationSlug}_{releaseSlug}.zip", release.AllFilesZipPath());
        }
    }
}
