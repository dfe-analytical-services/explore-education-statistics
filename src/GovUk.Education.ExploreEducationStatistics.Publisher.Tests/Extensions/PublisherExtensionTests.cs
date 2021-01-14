using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Extensions
{
    public class PublisherExtensionTests
    {
        [Fact]
        public void IsReleasePublished_ReleasePublished()
        {
            var publication = new Publication();

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            publication.Releases = new List<Release>
            {
                release
            };

            Assert.True(release.IsReleasePublished());
        }

        [Fact]
        public void IsReleasePublished_ReleaseNotPublished()
        {
            var publication = new Publication();

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = null
            };

            publication.Releases = new List<Release>
            {
                release
            };

            Assert.False(release.IsReleasePublished());
        }

        [Fact]
        public void IsReleasePublished_ReleaseNotPublishedButIncluded()
        {
            var publication = new Publication();

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = null
            };

            publication.Releases = new List<Release>
            {
                release
            };

            Assert.True(release.IsReleasePublished(new List<Guid>
            {
                release.Id
            }));
        }

        [Fact]
        public void IsReleasePublished_AmendmentReleaseNotPublished()
        {
            var publication = new Publication();

            var originalRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Version = 0,
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            var amendmentRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                PreviousVersionId = originalRelease.Id,
                Version = 1
            };

            publication.Releases = new List<Release>
            {
                originalRelease,
                amendmentRelease
            };

            Assert.True(originalRelease.IsReleasePublished());
            Assert.False(amendmentRelease.IsReleasePublished());
        }

        [Fact]
        public void IsReleasePublished_AmendmentReleasePublished()
        {
            var publication = new Publication();

            var originalRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Version = 0,
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            var amendmentRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                PreviousVersionId = originalRelease.Id,
                Version = 1,
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            publication.Releases = new List<Release>
            {
                originalRelease,
                amendmentRelease
            };

            Assert.False(originalRelease.IsReleasePublished());
            Assert.True(amendmentRelease.IsReleasePublished());
        }

        [Fact]
        public void IsReleasePublished_AmendmentReleaseNotPublishedButIncluded()
        {
            var publication = new Publication();

            var originalRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Version = 0,
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            var amendmentRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                PreviousVersionId = originalRelease.Id,
                Version = 1
            };

            publication.Releases = new List<Release>
            {
                originalRelease,
                amendmentRelease
            };

            Assert.False(originalRelease.IsReleasePublished(new List<Guid>
            {
                amendmentRelease.Id
            }));

            Assert.True(amendmentRelease.IsReleasePublished(new List<Guid>
            {
                amendmentRelease.Id
            }));
        }
    }
}