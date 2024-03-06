#nullable enable
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

            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            publication.Releases = new List<ReleaseVersion>
            {
                releaseVersion
            };

            Assert.True(releaseVersion.IsReleasePublished());
        }

        [Fact]
        public void IsReleasePublished_ReleaseNotPublished()
        {
            var publication = new Publication();

            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = null
            };

            publication.Releases = new List<ReleaseVersion>
            {
                releaseVersion
            };

            Assert.False(releaseVersion.IsReleasePublished());
        }

        [Fact]
        public void IsReleasePublished_ReleaseNotPublishedButIncluded()
        {
            var publication = new Publication();

            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = null
            };

            publication.Releases = new List<ReleaseVersion>
            {
                releaseVersion
            };

            Assert.True(releaseVersion.IsReleasePublished(new List<Guid>
            {
                releaseVersion.Id
            }));
        }

        [Fact]
        public void IsReleasePublished_AmendmentReleaseNotPublished()
        {
            var publication = new Publication();

            var originalReleaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Version = 0,
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            var amendmentReleaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                PreviousVersionId = originalReleaseVersion.Id,
                Version = 1
            };

            publication.Releases = new List<ReleaseVersion>
            {
                originalReleaseVersion,
                amendmentReleaseVersion
            };

            Assert.True(originalReleaseVersion.IsReleasePublished());
            Assert.False(amendmentReleaseVersion.IsReleasePublished());
        }

        [Fact]
        public void IsReleasePublished_AmendmentReleasePublished()
        {
            var publication = new Publication();

            var originalReleaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Version = 0,
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            var amendmentReleaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                PreviousVersionId = originalReleaseVersion.Id,
                Version = 1,
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            publication.Releases = new List<ReleaseVersion>
            {
                originalReleaseVersion,
                amendmentReleaseVersion
            };

            Assert.False(originalReleaseVersion.IsReleasePublished());
            Assert.True(amendmentReleaseVersion.IsReleasePublished());
        }

        [Fact]
        public void IsReleasePublished_AmendmentReleaseNotPublishedButIncluded()
        {
            var publication = new Publication();

            var originalReleaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Version = 0,
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            var amendmentReleaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                PreviousVersionId = originalReleaseVersion.Id,
                Version = 1
            };

            publication.Releases = new List<ReleaseVersion>
            {
                originalReleaseVersion,
                amendmentReleaseVersion
            };

            Assert.False(originalReleaseVersion.IsReleasePublished(new List<Guid>
            {
                amendmentReleaseVersion.Id
            }));

            Assert.True(amendmentReleaseVersion.IsReleasePublished(new List<Guid>
            {
                amendmentReleaseVersion.Id
            }));
        }
    }
}
