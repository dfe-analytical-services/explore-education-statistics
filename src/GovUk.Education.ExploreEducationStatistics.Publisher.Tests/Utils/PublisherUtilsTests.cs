using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Utils.PublisherUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Utils
{
    public class PublisherUtilsTests
    {
        [Fact]
        public void IsLatestPublishedVersionOfRelease_LatestVersion()
        {
            var release1Version1 = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            var release1Version2 = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow.AddSeconds(-1),
                PreviousVersionId = release1Version1.Id
            };
            
            var release1Version3 = new Release
            {
                Id = Guid.NewGuid(),
                Published = null,
                PreviousVersionId = release1Version2.Id
            };

            var release2Version1 = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            // TODO EES-1145 Don't need to do this when PreviousVersionId is made optional
            SelfReferenceOnlyVersions(release1Version1, release2Version1);

            var releases = new List<Release>
            {
                release1Version1,
                release1Version2,
                release1Version3,
                release2Version1
            };

            var includedReleaseIds = new Guid[] { };

            Assert.False(IsLatestVersionOfRelease(releases, release1Version1.Id, includedReleaseIds));
            Assert.True(IsLatestVersionOfRelease(releases, release1Version2.Id, includedReleaseIds));
            Assert.False(IsLatestVersionOfRelease(releases, release1Version3.Id, includedReleaseIds));
            Assert.True(IsLatestVersionOfRelease(releases, release2Version1.Id, includedReleaseIds));
        }

        [Fact]
        public void IsLatestPublishedVersionOfRelease_LatestVersion_VersionNotPublishedButIncluded()
        {
            var release1Version1 = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            var release1Version2 = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow.AddSeconds(-1),
                PreviousVersionId = release1Version1.Id
            };

            var release1Version3 = new Release
            {
                Id = Guid.NewGuid(),
                Published = null,
                PreviousVersionId = release1Version2.Id
            };

            var release1Version4 = new Release
            {
                Id = Guid.NewGuid(),
                Published = null,
                PreviousVersionId = release1Version3.Id
            };

            var release2Version1 = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            // TODO EES-1145 Don't need to do this when PreviousVersionId is made optional
            SelfReferenceOnlyVersions(release1Version1, release2Version1);

            var releases = new List<Release>
            {
                release1Version1,
                release1Version2,
                release1Version3,
                release1Version4,
                release2Version1
            };

            var includedReleaseIds = new[] {release1Version3.Id};

            Assert.False(IsLatestVersionOfRelease(releases, release1Version1.Id, includedReleaseIds));
            Assert.False(IsLatestVersionOfRelease(releases, release1Version2.Id, includedReleaseIds));
            Assert.True(IsLatestVersionOfRelease(releases, release1Version3.Id, includedReleaseIds));
            Assert.False(IsLatestVersionOfRelease(releases, release1Version4.Id, includedReleaseIds));
            Assert.True(IsLatestVersionOfRelease(releases, release2Version1.Id, includedReleaseIds));
        }

        [Fact]
        public void IsReleasePublished_ReleasePublished()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow.AddSeconds(-1)
            };

            Assert.True(IsReleasePublished(release, new List<Guid>()));
        }

        [Fact]
        public void IsReleasePublished_ReleaseNotPublished()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Published = null
            };

            Assert.False(IsReleasePublished(release, new List<Guid>()));
        }

        [Fact]
        public void IsReleasePublished_ReleaseNotPublishedButIncluded()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Published = null
            };

            Assert.True(IsReleasePublished(release, new List<Guid>
            {
                release.Id
            }));
        }

        /// <summary>
        /// TODO EES-1145 Remove this when PreviousVersionId is made optional
        /// </summary>
        /// <param name="releases"></param>
        private static void SelfReferenceOnlyVersions(params Release[] releases)
        {
            foreach (var release in releases)
            {
                release.PreviousVersionId = release.Id;
            }
        }
    }
}