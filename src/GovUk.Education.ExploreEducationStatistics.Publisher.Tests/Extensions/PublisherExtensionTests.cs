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
    }
}